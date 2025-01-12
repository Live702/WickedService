param(
    [Parameter(Mandatory=$true)]
    [string]$BucketName

)

Import-Module powershell-yaml

# Load configuration from YAML file
$filePath = "..\..\serviceconfig.yaml"
if(-not (Test-Path $filePath)) {
    Write-Host "Please create a serviceconfig.yaml file above the solution folder."
    Write-Host "Copy the serviceconfig.yaml.template file and update the values in the new file."
    exit
}

$config = Get-Content $filePath | ConvertFrom-Yaml
$Profile = $config.Profile

# Enable verbose output
$VerbosePreference = "SilentlyContinue"

# Ensure AWS CLI is available
if (!(Get-Command aws -ErrorAction SilentlyContinue)) {
    Write-Error "AWS CLI is not installed or not in PATH"
    exit 1
}

# Clean bucket name - remove any S3 URL components
$cleanBucketName = $BucketName -replace '\.s3\.amazonaws\.com$', ''

function Test-S3BucketExists {
    param (
        [string]$BucketName,
        [string]$Profile
    )
    
    try {
        $result = aws s3api head-bucket --bucket $BucketName --profile $Profile 2>&1
        if ($LASTEXITCODE -eq 0) {
            return $true
        }
        return $false
    }
    catch {
        return $false
    }
}

function Wait-ForBucket {
    param (
        [string]$BucketName,
        [string]$Profile,
        [int]$MaxAttempts = 10
    )
    
    for ($i = 1; $i -le $MaxAttempts; $i++) {
        Write-Verbose "Attempt $i of $MaxAttempts to verify bucket existence..."
        if (Test-S3BucketExists -BucketName $BucketName -Profile $Profile) {
            Write-Verbose "Bucket verified as accessible"
            return $true
        }
        Write-Verbose "Bucket not yet accessible"
        if ($i -lt $MaxAttempts) {
            Start-Sleep -Seconds ($i * 2)
        }
    }
    return $false
}

try {
    # Get Region from profile
    Write-Verbose "Getting region from profile $Profile"
    $Region = aws configure get region --profile $Profile
    if (-not $Region) {
        Write-Error "Could not get region from profile $Profile"
        exit 1
    }
    Write-Verbose "Using region: $Region"

    # Get AWS Account ID from profile
    Write-Verbose "Getting AWS Account ID from profile $Profile"
    $AccountId = aws sts get-caller-identity --profile $Profile --query "Account" --output text
    if (-not $AccountId) {
        Write-Error "Could not get Account ID from profile $Profile"
        exit 1
    }
    Write-Verbose "Using Account ID: $AccountId"

    Write-Verbose "Using cleaned bucket name: $cleanBucketName"

    # Check if bucket already exists
    if (Test-S3BucketExists -BucketName $cleanBucketName -Profile $Profile) {
        Write-Verbose "Bucket already exists"
    }
    else {
        # Try to create bucket
        Write-Verbose "Attempting to create bucket: $cleanBucketName"
        try {
            if ($Region -eq "us-east-1") {
                $createCmd = "aws s3api create-bucket --bucket $cleanBucketName --profile $Profile --region $Region"
            }
            else {
                $createCmd = "aws s3api create-bucket --bucket $cleanBucketName --profile $Profile --region $Region --create-bucket-configuration LocationConstraint=$Region"
            }
            Write-Verbose "Running command: $createCmd"
            $output = Invoke-Expression $createCmd
            Write-Verbose "Create bucket output: $output"
        }
        catch {
            Write-Error "Failed to create bucket: $_"
            exit 1
        }

        # Wait for bucket to be accessible
        Write-Verbose "Waiting for bucket to be accessible..."
        $bucketReady = Wait-ForBucket -BucketName $cleanBucketName -Profile $Profile
        if (-not $bucketReady) {
            Write-Error "Bucket $cleanBucketName is not accessible after multiple attempts"
            exit 1
        }
    }

    Write-Verbose "Bucket is accessible"

    # Create bucket policy
    Write-Verbose "Creating bucket policy"
    $bucketPolicy = @{
        Version = "2012-10-17"
        Statement = @(
            @{
                Sid = "AllowCloudFrontRead"
                Effect = "Allow"
                Principal = @{
                    Service = "cloudfront.amazonaws.com"
                }
                Action = "s3:GetObject"
                Resource = "arn:aws:s3:::${cleanBucketName}/*"
                Condition = @{
                    StringEquals = @{
                        "AWS:SourceAccount" = $AccountId
                    }
                }
            }
        )
    }

    # Convert policy to JSON and save to temporary file
    $tempFile = New-TemporaryFile
    $bucketPolicy | ConvertTo-Json -Depth 10 | Set-Content $tempFile
    Write-Verbose "Policy saved to temporary file: $($tempFile.FullName)"

    # Apply bucket policy
    Write-Verbose "Applying bucket policy..."
    try {
        $policyOutput = aws s3api put-bucket-policy `
            --bucket $cleanBucketName `
            --policy file://$tempFile `
            --profile $Profile
        Write-Verbose "Policy application output: $policyOutput"
        Write-Host "Successfully created/updated bucket and applied policy using profile $Profile"
    }
    catch {
        Write-Error "Failed to apply bucket policy: $_"
        Remove-Item $tempFile
        exit 1
    }

    # Clean up temporary file
    Remove-Item $tempFile
}
catch {
    Write-Error "An error occurred: $_"
    exit 1
}
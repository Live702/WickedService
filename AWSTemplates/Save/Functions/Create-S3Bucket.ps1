function Create-S3Bucket {
    param(
        [Parameter(Mandatory=$true)]
        [string]$BucketName,
        [Parameter(Mandatory=$true)]
        [string]$Region,
        [Parameter(Mandatory=$true)]
        [string]$Account
    )

    . ./Functions/Test-S3BucketExists.ps1

    Write-Verbose "Creating S3 bucket $BucketName in region $Region" 

    try {
   
        # Clean bucket name - remove any S3 URL components
        $cleanBucketName = $BucketName.Split('.')[0]

        # Check if bucket already exists
        $bucketExists = Test-S3BucketExists -BucketName $cleanBucketName

        if($bucketExists) {
            return
        }


        Write-Verbose "Attempting to create bucket: $cleanBucketName in region $Region"
        try {
            $result = New-S3Bucket -BucketName $cleanBucketName -Region $Region -ErrorAction Stop
        
            # Verify bucket was created
            $verifyExists = Test-S3BucketExists -BucketName $cleanBucketName
            if (-not $verifyExists) {
                Write-Error "Bucket creation failed - bucket does not exist after creation attempt"
                exit 1
            }
        }
        catch {
            Write-Error "Failed to create bucket: $_"
            exit 1
        }

        # Create bucket policy
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
                    Resource = "arn:aws:s3:::" + $cleanBucketName + "/*"
                    Condition = @{
                        StringEquals = @{
                            "AWS:SourceAccount" = $Account
                        }
                    }
                }
            )
        }

        # Convert policy to JSON
        $policyJson = $bucketPolicy | ConvertTo-Json -Depth 10

        # Apply bucket policy
        Write-Verbose "Applying bucket policy..."
        try {
            Write-S3BucketPolicy -BucketName $cleanBucketName -Policy $policyJson
        }
        catch {
            Write-Error "Failed to apply bucket policy: $_"
            exit 1
        }
        Write-Verbose "Successfully created/updated bucket $cleanBucketName"
    }
    catch {
        Write-Error "An error occurred: $_"
        exit 1
    }
}
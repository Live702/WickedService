# This script gets the latest CloudFront log file from the S3 bucket and converts it to a simplified CSV file.
# https://docs.aws.amazon.com/powershell/latest/reference/
param( 
    [Parameter(Mandatory=$true)]
    [string]$TenantKey, 
    [string]$Guid
)

# Load configuration from YAML file
$filePath = "..\..\serviceconfig.yaml"
if(-not (Test-Path $filePath))
{
	Write-Host "Please create a serviceconfig.yaml file above the solution folder."
	Write-Host "Copy the serviceconfig.yaml.template file and update the values in the new file."
	exit
}

$config = Get-Content -Path $filePath | ConvertFrom-Yaml
$SystemGuid = $config.SystemGuid
if(-not $Guid.HasValue) {
	$Guid = $SystemGuid
}
$Profile = $config.Profile

$bucketName = "cdnlogs-" + $TenantKey + "-" + $Guid
Write-Host "Bucket Name: $bucketName"
Import-Module powershell-yaml
Import-Module AWSPowerShell.NetCore

function Display-OutputDictionary {
    param (
        [Parameter(Mandatory=$true)]
        [hashtable]$Dictionary,
        
        [Parameter(Mandatory=$false)]
        [string]$Title = "Stack Outputs"
    )
    
    Write-Host $Title -ForegroundColor Cyan
    Write-Host "------------------------" -ForegroundColor Cyan
    
    $Dictionary.GetEnumerator() | Sort-Object Key | ForEach-Object {
        Write-Host "$($_.Key):" -ForegroundColor Green -NoNewline
        Write-Host " $($_.Value)"
    }
    
    Write-Host "------------------------`n" -ForegroundColor Cyan
}
function Convert-CloudFrontLogToCSV {
    param (
        [string]$LogFilePath
    )

    # Read the file line by line
    $lines = Get-Content -Path $LogFilePath
    $versionLine = $lines | Where-Object { $_ -match '^#Version:' }
    $fieldsLine = $lines | Where-Object { $_ -match '^#Fields:' }
    $dataLines = $lines | Where-Object { $_ -notmatch '^#' -and $_ -ne '' }

    if (-not $fieldsLine) {
        throw "Fields line not found in the log file."
    }

    $fields = ($fieldsLine -split ':')[1].Trim() -split '\s+'
    $header = $fields

    $csvData = foreach ($line in $dataLines) {
        $values = $line -split "`t"
        $lineObj = [ordered]@{}
        for ($i = 0; $i -lt $header.Count; $i++) {
            $lineObj[$header[$i]] = $values[$i]
        }
        [PSCustomObject]$lineObj
    }

    return @{
        Version = if ($versionLine) { ($versionLine -split ' ')[1] } else { "Unknown" }
        Fields = $fields
        Data = $csvData
    }
}

function Expand-GZipFile {
    param (
        [string]$infile,
        [string]$outfile = ($infile -replace '\.gz$','')
    )
    
    $input = New-Object System.IO.FileStream $infile, ([IO.FileMode]::Open), ([IO.FileAccess]::Read), ([IO.FileShare]::Read)
    $output = New-Object System.IO.FileStream $outfile, ([IO.FileMode]::Create), ([IO.FileAccess]::Write), ([IO.FileShare]::None)
    $gzipStream = New-Object System.IO.Compression.GzipStream $input, ([IO.Compression.CompressionMode]::Decompress)
    
    $buffer = New-Object byte[](1024)
    while($true) {
        $read = $gzipStream.Read($buffer, 0, 1024)
        if ($read -le 0) {break}
        $output.Write($buffer, 0, $read)
    }
    
    $gzipStream.Close()
    $output.Close()
    $input.Close()
}

# Get the latest file from the S3 bucket
$latestFile = Get-S3Object -BucketName $bucketName  -ProfileName $Profile | 
    Sort-Object LastModified -Descending | 
    Select-Object -First 1

# Download the compressed file
$tempCompressedFile = [System.IO.Path]::GetTempFileName()
Read-S3Object -BucketName $bucketName -Key $latestFile.Key -File $tempCompressedFile -ProfileName $Profile

# Decompress the file
$tempDecompressedFile = [System.IO.Path]::GetTempFileName()
Expand-GZipFile -infile $tempCompressedFile -outfile $tempDecompressedFile

# Read the decompressed file content
$logContent = Get-Content -Path $tempDecompressedFile -Raw

# Convert to CSV
$result = Convert-CloudFrontLogToCSV -LogFilePath $tempDecompressedFile

# Export to CSV file
$outputPath = Join-Path $PSScriptRoot "cloudfront_log_simplified.csv"
$result.Data | Export-Csv -Path $outputPath -NoTypeInformation

# Clean up temporary file
Remove-Item -Path $tempDecompressedFile

Write-Host "Processing complete. Simplified CSV file saved to: $outputPath"
Write-Host "Log file version: $($result.Version)"
Write-Host "Fields found: $($result.Fields -join ', ')"


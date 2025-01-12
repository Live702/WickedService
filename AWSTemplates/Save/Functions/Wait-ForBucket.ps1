function Wait-ForBucket {
    param (
        [string]$BucketName,
        [int]$MaxAttempts = 10
    )
    
    for ($i = 1; $i -le $MaxAttempts; $i++) {
        Write-Verbose "Attempt $i of $MaxAttempts to verify bucket existence..."
        if (. ./Functions/Test-S3BucketExists -BucketName $BucketName) {
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
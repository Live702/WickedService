

function Update-KVSEntry {
    param (
        [Parameter(Mandatory = $true)]
        [string]$KvsARN,
        
        [Parameter(Mandatory = $true)]
        [string]$Key,

        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    # Retrieve the entire response object
    $response = Get-CFKVKeyValueStore -KvsARN $KvsARN
    
    # Extract just the ETag from the response
    $etag = $response.ETag

    # Pass that ETag to IfMatch
    $response = Write-CFKVKey -KvsARN $KvsARN -Key $Key -Value $Value -IfMatch $etag 

    return
}

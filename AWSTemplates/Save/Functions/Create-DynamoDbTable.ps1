Import-Module AWS.Tools.DynamoDBv2

function Create-DynamoDbTable {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$TableName
    )
    
    try {
        # Check if table exists
        try {
            $existingTable = Get-DDBTable -TableName $TableName -Region $Region -ErrorAction SilentlyContinue
        }
        catch {
            $existingTable = $null
        }
        
        if ($existingTable) {
            Write-Verbose "Table '$TableName' already exists."
            return
        }

        # Create a table schema compatible with the LazyMagic DynamoDb library
        # This library provides an entity abstraction with CRUDL support for DynamoDb

        $schema = New-DDBTableSchema
        $schema | Add-DDBKeySchema -KeyName "PK" -KeyDataType "S" -KeyType "HASH"
        $schema | Add-DDBKeySchema -KeyName "SK" -KeyDataType "S" -KeyType "RANGE"
        $schema | Add-DDBIndexSchema -IndexName "PK-SK1-Index" -RangeKeyName "SK1" -RangeKeyDataType "S" -ProjectionType "include" -NonKeyAttribute "Status", "UpdateUtcTick", "CreateUtcTick", "General"
        $schema | Add-DDBIndexSchema -IndexName "PK-SK2-Index" -RangeKeyName "SK2" -RangeKeyDataType "S" -ProjectionType "include" -NonKeyAttribute "Status", "UpdateUtcTick", "CreateUtcTick", "General"
        $schema | Add-DDBIndexSchema -IndexName "PK-SK3-Index" -RangeKeyName "SK3" -RangeKeyDataType "S" -ProjectionType "include" -NonKeyAttribute "Status", "UpdateUtcTick", "CreateUtcTick", "General"
        $schema | Add-DDBIndexSchema -IndexName "PK-SK4-Index" -RangeKeyName "SK4" -RangeKeyDataType "S" -ProjectionType "include" -NonKeyAttribute "Status", "UpdateUtcTick", "CreateUtcTick", "General"
        $schema | Add-DDBIndexSchema -IndexName "PK-SK5-Index" -RangeKeyName "SK5" -RangeKeyDataType "S" -ProjectionType "include" -NonKeyAttribute "Status", "UpdateUtcTick", "CreateUtcTick", "General"
        # $schema | Add-DDBIndexSchema -Global -IndexName "GSI1" -HashKeyName "GSI1PK" -RangeKeyName "GSI1SK" -RangeKeyDataType "S" -ProjectionType "include" -NonKeyAttribute "Status", "UpdateUtcTick", "CreateUtcTick", "General" -ReadCapacity 10 -WriteCapacity 10


        $response = New-DDBTable -TableName $TableName `
            -Schema $schema `
            -BillingMode "PAY_PER_REQUEST" 

        # Wait for table to be active
        Write-Verbose "Waiting for table to become active..."
        do {
            Start-Sleep -Seconds 5
            $tableStatus = (Get-DDBTable -TableName $TableName).TableStatus
        } while ($tableStatus -ne "ACTIVE")

        # Enable TTL
        $response = Update-DDBTimeToLive -TableName $TableName `
            -TimeToLiveSpecification_AttributeName "TTL" `
            -TimeToLiveSpecification_Enable $true 

        Write-Verbose "Table '$TableName' created successfully."
        return ""
    }
    catch {
        Write-Error "Failed to create table '$TableName': $_"
        throw
    }
    return
}
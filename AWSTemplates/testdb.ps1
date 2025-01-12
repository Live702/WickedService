
Import-Module AWS.Tools.DynamoDBv2


$schema = New-DDBTableSchema
$schema | Add-DDBKeySchema -KeyName "PK" -KeyDataType "S" -KeyType "HASH"
$schema | Add-DDBKeySchema -KeyName "SK" -KeyDataType "S" -KeyType "RANGE"
$schema | Add-DDBIndexSchema -IndexName "PK-SK1-Index" -RangeKeyName "SK1" -RangeKeyDataType "S" -ProjectionType "include" -NonKeyAttribute "Status", "UpdateUtcTick", "CreateUtcTick", "General"
$schema | Add-DDBIndexSchema -IndexName "PK-SK2-Index" -RangeKeyName "SK2" -RangeKeyDataType "S" -ProjectionType "include" -NonKeyAttribute "Status", "UpdateUtcTick", "CreateUtcTick", "General"
$schema | Add-DDBIndexSchema -IndexName "PK-SK3-Index" -RangeKeyName "SK3" -RangeKeyDataType "S" -ProjectionType "include" -NonKeyAttribute "Status", "UpdateUtcTick", "CreateUtcTick", "General"
$schema | Add-DDBIndexSchema -IndexName "PK-SK4-Index" -RangeKeyName "SK4" -RangeKeyDataType "S" -ProjectionType "include" -NonKeyAttribute "Status", "UpdateUtcTick", "CreateUtcTick", "General"
$schema | Add-DDBIndexSchema -IndexName "PK-SK5-Index" -RangeKeyName "SK5" -RangeKeyDataType "S" -ProjectionType "include" -NonKeyAttribute "Status", "UpdateUtcTick", "CreateUtcTick", "General"
$schema | New-DDBTable -TableName "Thread" -BillingMode "PAY_PER_REQUEST"


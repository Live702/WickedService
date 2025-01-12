# This script generates the kvs entries for a tenant, and the tenant's subtenants
# into a [tenantKey].json file in the Generated folder.
# The tenantKey argument must match a tenant defined in the SystemConfig.yaml file.

param( 
    [Parameter(Mandatory=$true)]
    [string]$TenantKey
)
# Setup
# Don't turn on verbose here, do it below the imports if you need it
$VerbosePreference = "SilentlyContinue" # don't show Write-Verbose messages

. ./Functions/Get-SystemConfig.ps1
. ./Functions/Get-StackOutputs.ps1
. ./Functions/Get-BehaviorsHashTable.ps1
. ./Functions/Get-TenantKVSEntry.ps1
. ./Functions/Get-SubtenantKVSEntry.ps1
. ./Functions/Get-AssetNames.ps1

# Set to "Continue" to debug the script
$VerbosePreference = "SilentlyContinue" # don't show Write-Verbose messages
 
Write-Verbose "Generating kvs entries for tenant $TenantKey"  

$SystemConfig = Get-SystemConfig 
$config = $SystemConfig.Config
$Profile = $SystemConfig.Profile
$Region = $SystemConfig.Region
$SystemKey = $config.SystemKey
$SystemSuffix = $config.SystemSuffix
$Behaviors = $config.Behaviors
$Environment = $config.Environment  

# Check that the supplied tenantKey is in the SystemConfig.yaml and grab the tenant properties
if($config.Tenants.ContainsKey($TenantKey) -eq $false) {
    Write-Host "The tenant key $TenantKey is not defined in the SystemConfig.yaml file."
    exit
}
$TenantConfig = $config.Tenants[$TenantKey]

$KvsEntries = @{}

# Get service stack outputs     
$ServiceStackOutputDict = Get-StackOutputs ($config.SystemKey + "---service")

# Convert the SystemBehaviors to a HashTable (where the first element in the behavior array is the key))
$SystemBehaviors = Get-BehaviorsHashTable "{ss}" $Environment $Region $Behaviors $ServiceStackOutputDict 0

# Generate tenant kvs entry
$ProcessedTenant = Get-TenantKVSEntry $SystemKey $SystemSuffix $Environment $Region $SystemBehaviors $TenantKey $TenantConfig $ServiceStackOutputDict 

# Create and append the tenant kvs entry
# The Domain and Level are properties not contained in the ProcessedTenant KVS entry object. We 
# need the domain and Level later when publishing or processing the entry.
$KvsEntries[$TenantKey] = @{ 
        KvsEntry = $ProcessedTenant
        Domain = $TenantConfig.RootDomain
        Level = 1
}
$ProcessedTenantJson = $ProcessedTenant | ConvertTo-Json -Compress -Depth 10    
Write-Verbose $ProcessedTenantJson
Write-Verbose $ProcessedTenantJson.Length

# GetAsset Names allows us to see the asset names that will be created for the tenant kvs entry
$Assetnames = Get-AssetNames $ProcessedTenant 1 $true
Write-Verbose "Tenant Asset Names"
foreach($assetName in $Assetnames) {
    Write-Verbose $assetName
}

# Generate subtenant kvs entries
Write-Verbose "Processing SubTenants"
$Subtenants = @()
foreach($subtenant in $TenantConfig.SubTenants.GetEnumerator()) {
    $ProcessedSubtenant = Get-SubtenantKVSEntry $ProcessedTenant $subtenant.Value $subtenant.Key $ServiceStackOutputDict
    $KvsEntries[ $subtenant.Key + "-" + $TenantKey] = @{
        Domain = $subtenant.Value.Subdomain + "." + $TenantConfig.RootDomain  
        KvsEntry = $ProcessedSubtenant
        Level = 2
    }

    $ProcessedSubtenantJson = $ProcessedSubtenant | ConvertTo-Json -Compress -Depth 10  
    Write-Verbose $ProcessedSubtenantJson
    Write-Verbose $ProcessedSubtenantJson.Length
    $Subtenants += $ProcessedSubtenant

    $Assetnames = Get-AssetNames $ProcessedSubtenant 2 $true
    Write-Verbose "Subtenant Asset Names"
    foreach($assetName in $Assetnames) {
        Write-Verbose $assetName
    }
}

$KvsEntriesJson = $KvsEntries | ConvertTo-Json -Depth 10  
Write-Verbose $KvsEntriesJson

# Write the [tenant].json file
Set-Content -Path ("./Generated/" + $TenantKey + ".json") -Value $KvsEntriesJson

Write-Host "Generated kvs entries for tenant $TenantKey"

exit 0
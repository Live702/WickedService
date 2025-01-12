
. ./Functions./Get-BehaviorsHashTable.ps1
function Get-TenantKVSEntry($mySystemKey,$mySystemSuffix,$myEnvironment,$myRegion,$mySystemBehaviorsHash,$myTenantKey,$myTenant, $myServiceStackOutputDict) {
    $tenantConfig = @{
        env = $myEnvironment
        region = $myRegion 
        systemKey = $mySystemKey
        tenantKey = $myTenantKey
        ss = $mySystemSuffix
        ts = $myTenant.TenantSuffix ?? "{ss}"
        behaviors = @()
    }

    $myBehaviorsHash = @{} + $mySystemBehaviorsHash # clone
    $tenantBehaviorsHash = (Get-BehaviorsHashTable "{ts}" $myEnvironment $myRegion $myTenant.Behaviors $myServiceStackOutputDict 1)
    # append method compatible with all powershell versions
    $tenantBehaviorsHash.Keys | ForEach-Object {
        $myBehaviorsHash[$_] = $tenantBehaviorsHash[$_]
    }
    $tenantConfig.behaviors = $myBehaviorsHash.Values

    $tenantConfigJson = $tenantConfig | ConvertTo-Json
    # Write-Host $tenantConfigJson

    return $tenantConfig
}
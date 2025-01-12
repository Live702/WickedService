. ./Functions/Get-HashTableFromProcessedBehaviorArray.ps1
function Get-SubtenantKVSEntry($myProcessedTenant, $mySubtenant, $mySubTenantKey, $myServiceStackOutputDict) {
    $tenantConfig = @{
        env = $myProcessedTenant.env
        region = $myProcessedTenant.region
        systemKey = $myProcessedTenant.systemKey
        tenantKey = $myProcessedTenant.tenantKey
        subtenantKey = $mySubTenantKey
        ss = $myProcessedTenant.ss
        ts = $myProcessedTenant.ts
        sts = $mySubtenant.SubTenantSuffix ?? "{ts}"
        behaviors = @()
    }
   
    $myBehaviorsHash = Get-HashTableFromProcessedBehaviorArray $myProcessedTenant.behaviors
    $mySubtenantBehaviorsHash = (Get-BehaviorsHashTable "{sts}" $myProcessedTenant.env $myProcessedTenant.region $mySubtenant.Behaviors $myServiceStackOutputDict 2)
    $mySubtenantBehaviorsHash.Keys | ForEach-Object {
        $myBehaviorsHash[$_] = $mySubtenantBehaviorsHash[$_]
    }

    $tenantConfig.behaviors = $myBehaviorsHash.Values
    $tenantConfigJson = $tenantConfig | ConvertTo-Json
    # Write-Host $tenantConfigJson

    return $tenantConfig
}
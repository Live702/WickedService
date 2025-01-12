# Transform a config Behaviors property into a hash table of behavior entries where
# the key is the path and the values have a form suitable for the type.
# Example:
# Behaviors:
#   Assets:
#   - Path: "/tenancy"
#   - Path: "/yada"
#     Guid: "1234"
# becomes 
#  behaviors["/tenancy"] = @[ "/tenancy", "assets", "{ss}", "{region}" ]]
#  hehaviors["/yada"] = @[ "/yada", "assets", "1234", "{region}" ]
. ./Functions/Get-HashTableFromBehaviorArray.ps1
function Get-BehaviorsHashTable($myGuidRepl,$myEnvironment,$myRegion,$myBehaviors, $myServiceStackOutputDict, $myLevel)
{
    # $myGuidRepl is one of {ss}, {ts}, {sts}
    # $myBehaviors is the Behaviors property from the config file. 

    $behavors = @()
    $myAssets = Get-HashTableFromBehaviorArray($myBehaviors.Assets)
    foreach($asset in $myAssets.Values) {
        # [path,assetType,guid,region]
        $guid = $asset.Guid 
        #$region = $asset.Region
        #$region = ($asset.PSObject.Properties.Match('Region').Count) 
        #    ? $asset.Region
        #    : $myRegion

        $guid = if ($null -eq $asset.Guid) { $myGuidRepl } else { $asset.Guid }
        $region = if ($null -eq $asset.Region) { $myRegion } else { $asset.Region }

        $behaviors += ,@(
            $asset.Path,
            "assets",
            $guid,
            $region,
            $myLevel
        )
    }
    $myWebApps = Get-HashTableFromBehaviorArray($myBehaviors.WebApps)
    foreach($webApp in $myWebApps.Values) {
        # [path,assetType,appName,guid,region]
        $guid = if ($null -eq $webApp.Guid) { $myGuidRepl } else { $webApp.Guid }
        $region = if ($null -eq $webApp.Region) { $myRegion } else { $webApp.Region }
        $behaviors += ,@(
            $webApp.Path,
            "webapp",
            $webapp.AppName,
            $guid,
            $region,
            $myLevel
        )
    }
    $myApis = Get-HashTableFromBehaviorArray($myBehaviors.Apis)
    foreach($api in $myApis.Values) {
        # [path,assetType,apiname,region,env]
        $region = if ($null -eq $api.Region) { $myRegion } else { $api.Region }
        $behaviors += ,@(
            $api.Path,
            "api",
            $myServiceStackOutputDict[($api.ApiName + "Id")],
            $region
        )
    }
    $behaviorHash = @{}
    foreach($behavior in $behaviors) {
        $behaviorHash[$behavior[0]] = $behavior
    }
    return $behaviorHash
}

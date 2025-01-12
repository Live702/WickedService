# Generate asset names for the tenant or subtenant
# myLevel = 1 for tenant 
# myLevel = 2 for subtenant
# notes: 
# 1. level 0 (system) is always generated
# 2. This logic is similar to that imiplemented in the CloudFront Function 
#   handling requests. The myTenant object passed contains the same information
#   returned from the kvs for the tenant or subtenant.
function Get-AssetNames($myTenant, $myLevel, $report = $false, $s3only = $false) {
    $assetNames = @()   
    $env = $myTenant.env 
    $region = $myTenant.region
    $systemKey = $myTenant.systemKey
    $tenantKey = $myTenant.tenantKey
    $subtenantKey = $myTenant.subtenantKey
    $ss = $myTenant.ss
    $ts = $myTenant.ts
    $sts = $myTenant.sts

    Write-Verbose "Generating asset names for TenantKey: $tenantKey SubtenantKey: $subtenantKey level: $myLevel report: $report s3only: $s3only"


    # Match Precidence
    # Each behavor starts with a Path. The path is a comma delimited string containin one or more 
    # paths. We match these paths to the path on incomming calls. 
    # To emulate this behavior for this routine, when running in report mode, we generate the 
    # same array of final behavior processing used in the CloudFront function that ultimatly 
    # uses these behaviors.

    $beahviors = $myTenant.behaviors  # default treatment

    # Report treatment
    if($report) {
        $behaviors = @()
        foreach($behavior in $myTenant.behaviors) {
            $paths = $behavior[0] -split ","
            foreach($path in $paths) {
                if($behavior.length -eq 5)
                {
                    $behaviors += ,@($path, $behavior[1], $behavior[2], $behavior[3], $behavior[4])
                } else {
                    $behaviors += ,@($path, $behavior[1], $behavior[2], $behavior[3], $behavior[4], $behavior[5])
                }
            }
        }
        # now sort the behaviors by path descending
        $behaviors = $behaviors | Sort-Object -Property {$_[0].length} -Descending
    }


    foreach($behavior in $behaviors) {
        $skip = $false;
        $assetType = $behavior[1]
        $awssuffix = '.awsamazon.com'

        $behaviorString = $behavior -join ", "
        Write-Verbose "behavior: $behaviorString"

        switch($assetType) {
            "api" {
                # behavior array
                #   0 path: 
                #   1 assetType:
                #   2 apiName: 
                #   3 region:
                #   4 level:
                # maps into
                # [apiName].execute-api.[region].amazonaws.com
                if($s3Only ) 
                {
                    $skip = $true
                    continue
                }
                
                $assetName = $behavior[2] + '.execute-api.' + $behavior[3] + $awsSuffix
            }
            "assets" {
                # behavior array
                #   0 path:
                #   1 assetType:
                #   2 suffix:
                #   3 region:
                #   4 level:
                # maps into
                # [systemKey]-[tenantKey]-[subtenantKey]-[assetType]-[suffix].s3.[region].amazonaws.com
                $behaviorLevel = $behavior[4]
                $assetTenantKey = $(if($behaviorLevel -gt 0) {$tenantKey} else {''})
                $assetSubtenantKey = $(if($behaviorLevel -gt 1) {$subtenantKey} else {''}) 
                $assetName = $systemKey + "-" + $assetTenantKey + "-" + $assetSubtenantKey + "-" + $assetType + "-" + $behavior[2] + '.s3.' + $behavior[3] + $awsSuffix
            }
                
            "webapp" {
                # behavior array
                #   0 path:
                #   1 assetType:
                #   2 appName:
                #   3 suffix:
                #   4 region:
                #   5 level:
                # maps into
                # [systemKey]-[tenantKey]-[subtenantKey]-[assetType]-[appName]-[suffix].s3.[region].amazonaws.com
                $behaviorLevel = $behavior[5]
                $assetTenantKey = $(if($behaviorLevel -gt 0) {$tenantKey} else {''})
                $assetSubtenantKey = $(if($behaviorLevel -gt 1) {$subtenantKey} else {''}) 
                $assetName = $systemKey + "-" + $assetTenantKey + "-" + $assetSubtenantKey + "-" + $assetType + "-" + $behavior[2] + "-" + $behavior[3] + '.s3.' + $behavior[4] + $awsSuffix
            }
        }
        if($skip) 
        {
            continue
        }
        # perform replacements for {ss}, {ts}, {sts}
        # note the order is important here as $sts may contain "{ts}" and $ts may contain "{ss}"
        $assetName = $assetName -replace "{sts}", $sts
        $assetName = $assetName -replace "{ts}", $ts
        $assetName = $assetName -replace "{ss}", $ss    
        if($report -eq $true) {
            $assetNames += '' + $behavior[1] + ' ' + $behavior[0] + ' ' + $assetName 
        } else {
            $assetNames += $assetName   
        }
    }
    
    return $assetNames
}
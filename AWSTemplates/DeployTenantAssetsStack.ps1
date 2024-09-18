# This script deploys a StoreTenancy assets stack
# Note: This script is static. It is not generated.
# https://docs.aws.amazon.com/powershell/latest/reference/
param( 
    [Parameter(Mandatory=$true)] [string]$TenantKey, 
    [string]$Guid,
    [string]$RootDomain
)

Import-Module powershell-yaml
Import-Module AWSPowerShell.NetCore

function Display-OutputDictionary {
    param (
        [Parameter(Mandatory=$true)] [hashtable]$Dictionary,
        [Parameter(Mandatory=$false)] [string]$Title = "Stack Outputs"
    )
    
    Write-Host $Title -ForegroundColor Cyan
    Write-Host "------------------------" -ForegroundColor Cyan
    
    $Dictionary.GetEnumerator() | Sort-Object Key | ForEach-Object {
        Write-Host "$($_.Key):" -ForegroundColor Green -NoNewline
        Write-Host " $($_.Value)"
    }
    
    Write-Host "------------------------`n" -ForegroundColor Cyan
}

function Get-StackOutputs {
	param (
	    [string]$SourceStackName
        )

    $stack = Get-CFNStack -StackName $SourceStackName -ProfileName $Profile
    $outputDictionary = @{}
    foreach($output in $stack.Outputs) {
        $outputDictionary[$output.OutputKey] = $output.OutputValue
    }
    return $outputDictionary
}

function CreateBucket {
	param (
		[string]$BucketName
	)
	$bucket = Get-S3Bucket -BucketName $BucketName -ProfileName $Profile -ErrorAction SilentlyContinue
	if ($bucket -ne $null) { return 'false' }
    return 'true'
}

function CreateTable {
	param (
		[string]$TableName
	)
    try {
	    $table = Get-DDBTable -TableName $TableName -ProfileName $Profile -ErrorAction SilentlyContinue
	    return 'false'
    } catch {
		return 'true'
	}
}

function ConvertTo-ParameterOverrides {
    param ([hashtable]$parametersDict)
    $overrides = @()
    foreach($key in $parametersDict.Keys) {
        $value = $parametersDict[$key]
		$overrides += "$key='$value'"
	}
    return $overrides -join " "
}

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
$StackName = $config.SystemName + "-" + $TenantKey + "-assets"
$Profile = $config.Profile

if($SystemGuid -like "yourguid")
{
	Write-Host "Please update the serviceconfig.yaml file with your system guid"
	exit
}

#Display-OutputDictionary -Dictionary $ServiceStackOutputDict -Title "Service Stack Outputs"
# Get policy stack outputs
$targetStack = $config.SystemName + "-policies"
$PoliciesStackOutputDict = Get-StackOutputs $targetStack

$ConfigBucketName = "config-$TenantKey-$SystemGuid"
$CreateConfigBucket = CreateBucket($ConfigBucketName)
$CreateCDNLogBucketName = "cdnlogs-$TenantKey-$SystemGuid"
$CreateCDNLogBucket = CreateBucket($CreateCDNLogBucketName)
$CreateTable = CreateTable($TenantKey)

# Create the parameters dictionary
$ParametersDict = @{
    "CreateConfigBucketsParameter" = $CreateConfigBucket
    "CreateCDNLogBucketParameter" = $CreateCDNLogBucket
    "CreateTableParameter" = $CreateTable
    "TenantKeyParameter" = $TenantKey
    "GuidParameter" = $Guid
    "RootDomainParameter" = $RootDomain
    "EnvironmentParameter" = $Environment
    "OriginAccessIdentityParameter" = $PoliciesStackOutputDict["OriginAccessIdentity"]
    "OriginAccessControl" = $PoliciesStackOutputDict["OriginAccessControl"]

}
#Display-OutputDictionary -Dictionary $ParametersDict -Title "Parameters Dictionary"

$parameters = ConvertTo-ParameterOverrides -parametersDict $ParametersDict
Write-Host $parameters

#Write-Host "Deploying the stack $StackName" 
sam deploy `
--template-file Templates/sam.tenant.assets.yaml `
--stack-name $StackName `
--parameter-overrides $parameters `
--capabilities CAPABILITY_IAM CAPABILITY_AUTO_EXPAND `
--profile $Profile 




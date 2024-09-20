# This script deploys a stack that creates a S3 webapp bucket 
param( 
    [Parameter(Mandatory=$true)] [string]$AppName, 
    [string]$Guid # will use SystemGuid from serviceconfig.yaml if not provided
)

Import-Module powershell-yaml

# Load configuration from YAML file
$filePath = "..\..\serviceconfig.yaml"
if(-not (Test-Path $filePath))
{
	Write-Host "Please create a serviceconfig.yaml file above the solution folder."
	Write-Host "Copy the serviceconfig.yaml.template file and update the values in the new file."
	exit
}

$config = Get-Content -Path $filePath | ConvertFrom-Yaml
if($SystemGuid -like "yourguid")
{
	Write-Host "Please update the serviceconfig.yaml file with your system guid"
	exit
}

$SystemGuid = $config.SystemGuid
if(-not $Guid.HasValue) {
	$Guid = $SystemGuid
}
$SystemName = $config.SystemName
$StackName = $systemName + "-webapp-" + $AppName
$Profile = $config.Profile

function ConvertTo-ParameterOverrides {
    param ([hashtable]$parametersDict)
    $overrides = @()
    foreach($key in $parametersDict.Keys) {
        $value = $parametersDict[$key]
		$overrides += "$key='$value'"
	}
    return $overrides -join " "
}

# Create the parameters dictionary
$ParametersDict = @{
    "SystemNameParameter" = $SystemName
    "AppNameParameter" = $AppName
    "GuidParameter" = $Guid
}
$parameters = ConvertTo-ParameterOverrides -parametersDict $ParametersDict


Write-Host "Deploying the stack $StackName" 
sam deploy `
--template-file Templates/sam.webapp.yaml `
--stack-name $StackName `
--parameter-overrides $parameters `
--capabilities CAPABILITY_IAM CAPABILITY_AUTO_EXPAND `
--profile $Profile 



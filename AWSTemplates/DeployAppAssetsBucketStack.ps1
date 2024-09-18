# This script deploys a stack that creates the S3 bucket that holds webapp assets
param( 
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
$SystemGuid = $config.SystemGuid
if(-not $Guid.HasValue) {
	$Guid = $SystemGuid
}
$StackName = $config.SystemName + "-assets-bucket"
$Profile = $config.Profile

if($SystemGuid -like "yourguid")
{
	Write-Host "Please update the serviceconfig.yaml file with your system guid"
	exit
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

# Get the stack outputs from the Policies Stack
$targetStack = $config.SystemName + "-policies"
$PoliciesStackOutputDict = Get-StackOutputs $targetStack

# Create the parameters dictionary
$ParametersDict = @{
    "GuidParameter" = $Guid
    "OriginAccessIdentityParameter" = $PoliciesStackOutputDict["OriginAccessIdentity"]
    "OriginAccessControlParameter" = $PoliciesStackOutputDict["OriginAccessControl"]
}
$parameters = ConvertTo-ParameterOverrides -parametersDict $ParametersDict


Write-Host "Deploying the stack $StackName" 
sam deploy `
--template-file Templates/sam.webappassetsbucket.yaml `
--stack-name $StackName `
--parameter-overrides $parameters `
--capabilities CAPABILITY_IAM CAPABILITY_AUTO_EXPAND `
--profile $Profile 



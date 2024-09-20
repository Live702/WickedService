# This script deploys a stack that creates the system artifacts bucket 
param( [bool]$DeployOnly = $False )

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
$StackName = $config.SystemName + "-artifacts"
$S3Prefix = $config.S3Prefix
$Profile = $config.Profile
$Environment = $config.Environment
$BucketName = $config.SystemName + "-artifacts-" + $config.SystemGuid

if($SystemGuid -like "yourguid")
{
	Write-Host "Please update the serviceconfig.yaml file with your system guid"
	exit
}


Write-Host "Deploying the stack $StackName" 
sam deploy `
--template-file Templates/sam.artifacts.yaml `
--stack-name $StackName `
--parameter-overrides BucketNameParameter=$BucketName `
--capabilities CAPABILITY_IAM CAPABILITY_AUTO_EXPAND `
--profile $Profile 



# This script deletes the system from a dev account. 
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
$SystemSuffix = $config.SystemSuffix
$RootDomain = $config.RootDomain
$AcmCertifcateArn = $config.AcmCertifcateArn
$HostedZoneId = $config.HostedZoneId
$StackName = $config.StackName
$BucketName = $config.BucketNamePrefix + $config.SystemSuffix
$S3Prefix = $config.S3Prefix
$Profile = $config.Profile
$Environment = $config.Environment

if($SystemSuffix -like "yourguid")
{
	Write-Host "Please update the serviceconfig.yaml file with your system guid"
	exit
}
if($RootDomain -like "yourdomain")
{
	Write-Host "Please update the serviceconfig.yaml file with your root domain"
	exit
}
if($HostedZoneId -like "yourhostedzoneid")
{
	Write-Host "Please update the serviceconfig.yaml file with your hosted zone id"
	exit
}
if($AcmCertifcateArn -like "arn:aws:acm:us-east-1:account:certificate/guid")
{
	Write-Host "Please update the serviceconfig.yaml file with your ACM certificate ARN"
	exit
}

# Clear the system buckets




## Delete the system in release mode and deploy it.
#Write-Host "Building the system in release mode"
#cd ..
#dotnet build -c Release
#cd AWSTemplates

#Write-Host "Deploying the stack $StackName" 
#sam deploy `
#--template-file sam.devenv.yaml `
#--stack-name $StackName `
#--capabilities CAPABILITY_IAM CAPABILITY_AUTO_EXPAND `
#--parameter-overrides `
#AWSPROFILE=$Profile `
#EnvironmentParameter=$Environment `
#ArtifactsBucketParameter=$BucketName `
#S3PrefixParameter=$S3Prefix `
#SystemSuffixParameter=$SystemSuffix  `
#RootDomainParameter=$RootDomain `
#AcmCertifcateArnParameter=$AcmCertifcateArn `
#HostedZoneIdParameter=$HostedZoneId `
#--profile $Profile 

## Write out the outputs of the stack for use in the LocalWebApi
#$jsonObject = aws cloudformation describe-stacks `
#--stack-name lzm-dev-stack `
#--query "Stacks[0].Outputs" `
#--output json `
#--profile $Profile `
#| Out-String | ConvertFrom-Json
## Create a new hashtable to store the outputs
#$outputObject = @{}
## Populate the hastable with OutputKey and OutputValue pairs
#foreach( $item in $jsonObject) { $outputObject[$item.OutputKey] = $item.OutputValue }
##Convert the hashtable to a json string
#$outputJson = $outputObject | ConvertTo-Json | Out-File -FilePath .\outputs.json

# This script deploys a stack that defines the system service 
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
$RootDomain = $config.RootDomain
$AcmCertifcateArn = $config.AcmCertifcateArn
$HostedZoneId = $config.HostedZoneId
$SystemName = $config.SystemName
$StackName = $config.SystemName + "-service"
$ArtifactsBucket = $config.SystemName + "-artifacts-" + $config.SystemGuid
$S3Prefix = $config.S3Prefix
$Profile = $config.Profile
$Environment = $config.Environment

if($SystemGuid -like "yourguid")
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

if(-not $DeployOnly)
{

	Write-Host "Removing existing s3 artifacts files. DON'T do this in a test or prod environment! Deployed Lambda starts would fail."
	aws s3 rm s3://$ArtifactsBucket/$S3Prefix/ --recursive --profile $Profile

	Write-Host "Building Lambdas"
	cd ..
	dotnet build -c Release
	cd AWSTemplates

	Write-Host "Packaging Service Stack"
	sam package --template-file Generated/sam.Service.g.yaml `
	--output-template-file sam.Service.packaged.yaml `
	--s3-bucket $ArtifactsBucket `
	--s3-prefix $S3Prefix `
	--profile $Profile

	Write-Host "Uploading templates to s3"
	cd Generated
	$files = Get-ChildItem -Path . -Filter sam.*.yaml
	foreach ($file in $files) {
		$fileName = $file.Name
		aws s3 cp $file.FullName s3://$ArtifactsBucket/$S3Prefix/$fileName --profile $Profile
	}
	cd ..
}

Write-Host "Deploying the stack $StackName" 

sam deploy `
--template-file sam.Service.packaged.yaml `
--s3-bucket $ArtifactsBucket `
--stack-name $StackName `
--capabilities CAPABILITY_IAM CAPABILITY_AUTO_EXPAND `
--parameter-overrides `
AWSPROFILE=$Profile `
SystemNameParameter=$SystemName `
EnvironmentParameter=$Environment `
ArtifactsBucketParameter=$ArtifactsBucket `
S3PrefixParameter=$S3Prefix `
SystemGuidParameter=$SystemGuid  `
--profile $Profile 

# Write out the outputs of the stack for use in the LocalWebApi
$jsonObject = aws cloudformation describe-stacks `
--stack-name $StackName `
--query "Stacks[0].Outputs" `
--output json `
--profile $Profile `
| Out-String | ConvertFrom-Json
# Create a new hashtable to store the outputs
$outputObject = @{}
# Populate the hastable with OutputKey and OutputValue pairs
foreach( $item in $jsonObject) { $outputObject[$item.OutputKey] = $item.OutputValue }
#Convert the hashtable to a json string
$outputJson = $outputObject | ConvertTo-Json | Out-File -FilePath .\outputs.json

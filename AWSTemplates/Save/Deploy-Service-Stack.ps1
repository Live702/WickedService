# This script deploys a stack that defines the system service 

# Setup
# Don't turn on verbose here, do it below the imports if you need it
$VerbosePreference = "SilentlyContinue" # don't show Write-Verbose messages

. ./Functions/Get-SystemConfig.ps1	

# Set to "Continue" to debug the script
$VerbosePreference = "SilentlyContinue" # don't show Write-Verbose messages
 
Write-Verbose "Deploying system artifacts stack"  
$SystemConfig = Get-SystemConfig 
$config = $SystemConfig.Config
$Environment = $config.Environment
$Profile = $config.Profile
$S3Prefix = $config.S3Prefix
$SystemKey = $config.SystemKey
$SystemSuffix = $config.SystemSuffix

$StackName = $config.SystemKey + "---service"

$ArtifactsBucket = $config.SystemKey + "---artifacts-" + $config.SystemSuffix

# Create the Packages folder if it doesn't existing
mkdir ..\Packages -ErrorAction SilentlyContinue

# Removing existing s3 artifacts files. DON'T do this in a test or prod environment! Deployed Lambda starts would fail.
aws s3 rm s3://$ArtifactsBucket/$S3Prefix/ --recursive --profile $Profile
cd ..
Write-Verbose "Building Lambdas"
dotnet build -c Release

cd AWSTemplates

if(Test-Path "sam.Service.packages.yaml") {
		Remove-Item "sam.Service.packages.yaml"
}

# Note that sam requires we excplicitly set the --profile
sam package --template-file Generated/sam.Service.g.yaml `
--output-template-file sam.Service.packaged.yaml `
--s3-bucket $ArtifactsBucket `
--s3-prefix $S3Prefix `
--profile $Profile

Write-Verbose "Uploading templates to s3"
cd Generated
$files = Get-ChildItem -Path . -Filter sam.*.yaml
foreach ($file in $files) {
	$fileName = $file.Name
	aws s3 cp $file.FullName s3://$ArtifactsBucket/$S3Prefix/$fileName --profile $Profile
}
cd ..

Write-Verbose "Deploying the stack $StackName" 

# Note that sam requires we excplicitly set the --profile	
sam deploy `
--template-file sam.Service.packaged.yaml `
--s3-bucket $ArtifactsBucket `
--stack-name $StackName `
--capabilities CAPABILITY_IAM CAPABILITY_AUTO_EXPAND `
--parameter-overrides `
AWSPROFILE=$Profile `
SystemKeyParameter=$SystemKey `
EnvironmentParameter=$Environment `
ArtifactsBucketParameter=$ArtifactsBucket `
S3PrefixParameter=$S3Prefix `
SystemSuffixParameter=$SystemSuffix  `
--profile $Profile 


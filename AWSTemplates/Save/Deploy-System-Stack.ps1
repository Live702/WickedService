# This script deploys a stack that creates the system artifacts bucket,
# CloudFront KeyValueStore, and system DynamoDB table

# Setup
# Don't turn on verbose here, do it below the imports if you need it
$VerbosePreference = "SilentlyContinue" # don't show Write-Verbose messages

. ./Functions/Get-SystemConfig.ps1	

# Set to "Continue" to debug the script
$VerbosePreference = "SilentlyContinue" # don't show Write-Verbose messages
 
Write-Verbose "Deploying system stack"  
$SystemConfig = Get-SystemConfig 
$Region = $SystemConfig.Region
$config = $SystemConfig.Config
$Profile = $config.Profile
$SystemKey = $config.SystemKey
$SystemSuffix = $config.SystemSuffix

$StackName = $SystemKey + "---system"

# note that sam requires the --Profile be explicitly set
Write-Verbose "Deploying the stack $StackName using profile $Profile" 
sam deploy `
--template-file Templates/sam.system.yaml `
--stack-name $StackName `
--parameter-overrides SystemKeyParameter=$SystemKey SystemSuffixParameter=$SystemSuffix `
--capabilities CAPABILITY_IAM CAPABILITY_AUTO_EXPAND `
--profile $Profile

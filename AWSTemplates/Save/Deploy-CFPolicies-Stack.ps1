# This script deploys as stack that creates the policies and functions used in CloudFront

# Setup
# Don't turn on verbose here, do it below the imports if you need it
$VerbosePreference = "SilentlyContinue" # don't show Write-Verbose messages

. ./Functions/Get-SystemConfig.ps1
. ./Functions/Get-StackOutputs.ps1  

# Set to "Continue" to debug the script
$VerbosePreference = "SilentlyContinue" # don't show Write-Verbose messages
 
Write-Verbose "Deploying CloudFront Policies and Functions stack"  
$SystemConfig = Get-SystemConfig 
$config = $SystemConfig.Config
$Profile = $config.Profile
$SystemKey = $config.SystemKey
$Environment = $config.Environment

$StackName = $SystemKey + "---cfpolicies" 

# Get system stack outputs
$systemStack = $config.SystemKey + "---system"
$systemStackOutputDict = Get-StackOutputs $systemStack
$KeyValueStoreArn = $systemStackOutputDict["KeyValueStoreArn"]

Write-Verbose "Deploying the stack $StackName" 

# Note that sam requires we excplicitly set the --profile	
sam deploy `
--template-file Templates/sam.cfpolicies.yaml `
--stack-name $StackName `
--parameter-overrides SystemKey=$SystemKey  EnvironmentParameter=$Environment KeyValueStoreArnParameter=$KeyValueStoreArn `
--capabilities CAPABILITY_IAM CAPABILITY_AUTO_EXPAND `
--profile $Profile 

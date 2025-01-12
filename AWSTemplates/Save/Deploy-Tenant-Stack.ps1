# This script deploys a stack defining a tenancy
# The tenantKey argument must match a tenant defined in the SystemConfig.yaml file.
# https://docs.aws.amazon.com/powershell/latest/reference/
param( 
    [Parameter(Mandatory=$true)]
    [string]$TenantKey
)

. ./Functions/Get-SystemConfig.ps1
. ./Functions/Get-StackOutputs.ps1
. ./Functions/Display-OutputDictionary.ps1
. ./Functions/ConvertTo-ParameterOverrides.ps1   

# Set to "Continue" to debug the script
$VerbosePreference = "Continue" # don't show Write-Verbose messages

Write-Verbose "Deploying tenant stack"  
$SystemConfig = Get-SystemConfig 
$Region = $SystemConfig.Region
$config = $SystemConfig.Config
$Environment = $config.Environment
$Profile = $config.Profile
$SystemKey = $config.SystemKey
$SystemSuffix = $config.SystemSuffix

$StackName = $config.SystemKey + "-" + $TenantKey + "--tenant" 
$ArtifactsBucket = $config.SystemKey + "---artifacts-" + $config.SystemSuffix

$tenant = $config.Tenants[$TenantKey]

$requiredProps = @('RootDomain', 'HostedZoneId', 'AcmCertificateArn')
foreach ($prop in $requiredProps) {
    if (-not $tenant.ContainsKey($prop) -or [string]::IsNullOrWhiteSpace($tenant[$prop])) {
        Write-Host "Missing or empty property: $prop"
        return $false
    }
}

$RootDomain = $tenant.RootDomain
if([string]::IsNullOrWhiteSpace($RootDomain)) {
    Write-Host "RootDomain is missing or empty."
    return $false
}

$HostedZoneId = $tenant.HostedZoneId
$AcmCertificateArn = $tenant.AcmCertificateArn
$TenantSuffix = $SystemSuffix # default
if($tenant.ContainsKey('TenantSuffix') -and ![string]::IsNullOrWhiteSpace($tenant.TenantSuffix)) {
    $TenantSuffix = $tenant.TenantSuffix    
}

# Get service stack outputs
$targetStack = $config.SystemKey + "---service"
$ServiceStackOutputDict = Get-StackOutputs $targetStack

# Get policies stack outputs 
$targetStack = $config.SystemKey + "---cfpolicies"
$CFPolicyStackOutputDict = Get-StackOutputs $targetStack


# Create the parameters dictionary
$ParametersDict = @{
    # SystemConfigFile values
    "SystemKeyParameter" = $SystemKey
    "EnvironmentParameter" = $Environment
    "TenantKeyParameter" = $TenantKey
    "GuidParameter" = $TenantSuffix
    "RootDomainParameter" = $RootDomain
    "HostedZoneIdParameter" = $HostedZoneId
    "AcmCertificateArnParameter" = $AcmCertificateArn

    # ServiceStack values
    "ConfigFunctionArnParameter" = $ServiceStackOutputDict["ConfigFunctionArn"]

    # CFPolicyStack values
    "OriginRequestPolicyIdParameter" = $CFPolicyStackOutputDict["OriginRequestPolicyId"]
    "CachePolicyIdParameter" = $CFPolicyStackOutputDict["CachePolicyId"]
    "ApiCachePolicyIdParameter" = $CFPolicyStackOutputDict["ApiCachePolicyId"]
    "RequestFunctionArnParameter" = $CFPolicyStackOutputDict["RequestFunctionArn"]
    "ApiRequestFunctionArnParameter" = $CFPolicyStackOutputDict["ApiRequestFunctionArn"]
}

# Note that sam requires we explictly set the Profile
$parameters = ConvertTo-ParameterOverrides -parametersDict $ParametersDict
Write-Host "Deploying the stack $StackName" 
sam deploy `
--template-file Templates/sam.tenant.yaml `
--s3-bucket $ArtifactsBucket `
--stack-name $StackName `
--parameter-overrides $parameters `
--capabilities CAPABILITY_IAM CAPABILITY_AUTO_EXPAND `
--profile $Profile 

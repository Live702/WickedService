# This script deploys policies used in tenancies
Import-Module powershell-yaml
Import-Module AWSPowerShell.NetCore

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
$StackName = $config.SystemName + "-policies"
$Profile = $config.Profile
$Environment = $config.Environment

if($SystemGuid -like "yourguid")
{
	Write-Host "Please update the serviceconfig.yaml file with your system guid"
	exit
}

# Get service stack outputs
$targetStack = $config.SystemName + "-service"
$ServiceStackOutputDict = Get-StackOutputs $targetStack
$WebSocketApiIdParameter = $ServiceStackOutputDict["WebSocketApiId"]

Write-Host "Deploying the stack $StackName" 
sam deploy `
--template-file Templates/sam.policies.yaml `
--stack-name $StackName `
--parameter-overrides GuidParameter=$SystemGuid EnvironmentParameter=$Environment WebSocketApiIdParameter=$WebSocketApiIdParameter `
--capabilities CAPABILITY_IAM CAPABILITY_AUTO_EXPAND `
--profile $Profile 



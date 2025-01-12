Import-Module AWS.Tools.CloudFormation

function Get-StackOutputs {
	param (
	    [string]$SourceStackName
        )
    Write-Verbose "Getting stack outputs for $SourceStackName"
    $stack = Get-CFNStack -StackName $SourceStackName -ProfileName $Profile
    $outputDictionary = @{}
    foreach($output in $stack.Outputs) {
        $outputDictionary[$output.OutputKey] = $output.OutputValue
    }
    return $outputDictionary
}
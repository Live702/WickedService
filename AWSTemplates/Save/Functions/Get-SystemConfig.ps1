Import-Module powershell-yaml
Import-Module AWS.Tools.Common
. ./Functions/Find-File-Up.ps1

function Get-SystemConfig {
	# Find systemconfig.yaml file
	$filePath = Find-File-Up "systemconfig.yaml"

	if($filePath -eq $null) {
		Write-Host "Could not find a systemconfig.yaml file"
		exit 1
	}

	# Load configuration from YAML file
	Write-Verbose ("Getting system config for:" + ($config.SystemKey))

	if(-not (Test-Path $filePath))
	{
		Write-Host "Please create a systemconfig.yaml file above the solution folder."
		Write-Host "Copy the systemconfig.yaml.template file and update the values in the new file."
		exit
	}

	$config = Get-Content -Path $filePath | ConvertFrom-Yaml
	$SystemSuffix = $config.SystemSuffix
	if(-not $Guid.HasValue) {
		$Guid = $SystemSuffix
	}

	$ProfileName = $config.Profile

	Write-Verbose "Loaded system configuration from $filePath"

	Write-Verbose "Setting profile to $($config.Profile)"

	try {
		Set-AWSCredential -ProfileName $ProfileName -Scope Global
	} catch {
		Write-Host "Failed to set profile to $ProfileName"
		exit 1
	}
	$profile = GET-AWSCredential


	# Load System level configuration properties we process
	$value = @{
		Config = $config
		Account = $profile.accountId
		Region = $profile.region
	}

	$value
}
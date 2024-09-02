param(
    [string]$launchSettingsPath,
    [string]$outputsJsonPath
)

# Read and parse the outputs.json file
$outputs = Get-Content -Path $outputsJsonPath -Raw | ConvertFrom-Json -Depth 10
$awsProfile = $outputs.AWSPROFILE
Write-Host "AWSProfile: $awsProfile"
$awsRegion = $outputs.AWSREGION
Write-Host "AWSRegion: $awsRegion"
$userPools = $outputs.UserPools | ConvertFrom-Json -Depth 10
Write-Host "UserPools: $userPools"

# Read, parse and process the launchSettings.json file
$launchSettings = Get-Content -Path $launchSettingsPath -Raw | ConvertFrom-Json -Depth 10
$outputs = Get-Content -Path $outputsJsonPath -Raw | ConvertFrom-Json -Depth 10
foreach($profileName in $launchSettings.profiles.PSObject.Properties.Name) {
	$profile = $launchSettings.profiles.$profileName
    $userPoolName = $profile.environmentVariables.UserPoolName
    $userPoolConfig = $userPools.$userPoolName
    $profile.environmentVariables.UserPoolId = $userPoolConfig.UserPoolId
    $profile.environmentVariables.UserPoolClientId = $userPoolConfig.UserPoolClientId
    $profile.environmentVariables.IdentityPoolId = $userPoolConfig.IdentityPoolId
        
}

# Convert back to JSON and save. Adjust Depth as necessary.
$launchSettings | ConvertTo-Json -Depth 100 | Set-Content $launchSettingsPath

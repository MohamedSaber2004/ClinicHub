$ErrorActionPreference = "Stop"

$solutionDir = Split-Path -Parent $PSScriptRoot
$projectDir = Join-Path $solutionDir "ClinicHub.API"
$pubxmlUserFile = Join-Path $projectDir "Properties\PublishProfiles\IISProfile.pubxml.user"

if (-not (Test-Path $pubxmlUserFile)) {
    throw "Publish profile user file not found at $pubxmlUserFile"
}

Write-Host ">>> [1/5] Decrypting publish credentials..." -ForegroundColor Cyan
Add-Type -AssemblyName System.Security
$xml = [xml](Get-Content $pubxmlUserFile)
$enc = [Convert]::FromBase64String($xml.Project.PropertyGroup.EncryptedPassword)
$dec = [System.Security.Cryptography.ProtectedData]::Unprotect($enc, $null, [System.Security.Cryptography.DataProtectionScope]::CurrentUser)
$pwd = [System.Text.Encoding]::Unicode.GetString($dec)

$msdeploy = "C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe"
if (-not (Test-Path $msdeploy)) {
    $msdeploy = "C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe"
}
if (-not (Test-Path $msdeploy)) {
    throw "msdeploy.exe not found on machine."
}

$siteName = "doctory-001-site1"
$publishUrl = "https://win8245.site4now.net:8172/msdeploy.axd?site=$siteName"
$userName = "doctory-001"

# 1. Clean local publish folder without spaces
$publishTemp = "C:\publish_clinichub"
if (Test-Path $publishTemp) {
    Remove-Item -Recurse -Force $publishTemp
}
New-Item -ItemType Directory -Force -Path $publishTemp | Out-Null

Write-Host ">>> [2/5] Building & Publishing locally in Release mode..." -ForegroundColor Cyan
$projPath = Join-Path $projectDir "ClinicHub.API.csproj"
dotnet publish $projPath -c Release -o $publishTemp /p:UseAppHost=false

# Clean any offline html that might be in publish folder
if (Test-Path "$publishTemp\app_offline.htm") { Remove-Item -Force "$publishTemp\app_offline.htm" }
if (Test-Path "$publishTemp\App_Offline.htm") { Remove-Item -Force "$publishTemp\App_Offline.htm" }

# 2. Place app_offline.htm to put app offline
Write-Host ">>> [3/5] Setting remote site offline..." -ForegroundColor Cyan
$offlineDir = "C:\temp_offline"
New-Item -ItemType Directory -Force -Path $offlineDir | Out-Null
$offlineFile = "$offlineDir\app_offline.htm"
@"
<!DOCTYPE html>
<html>
<head><title>Updating Application</title></head>
<body>
    <h2>Application is being updated. Please retry in a few moments.</h2>
    <p>Update in progress for ClinicHub API...</p>
</body>
</html>
"@ | Set-Content -Path $offlineFile -Encoding UTF8

$destAuth = "doctory-001-site1/app_offline.htm,computerName=$publishUrl,userName=$userName,password=$pwd,authType=Basic"
& $msdeploy -verb:sync -source:contentPath=$offlineFile -dest:contentPath=$destAuth -allowUntrusted

Write-Host ">>> Waiting 5 seconds for file handles to release..." -ForegroundColor Cyan
Start-Sleep -Seconds 5

# 3. Sync all files
Write-Host ">>> [4/5] Syncing all files to remote server..." -ForegroundColor Cyan
$destSite = "doctory-001-site1,computerName=$publishUrl,userName=$userName,password=$pwd,authType=Basic"
& $msdeploy -verb:sync -source:contentPath=$publishTemp -dest:contentPath=$destSite -skip:objectName=filePath,absolutePath=".*ClinicHub\.API\.dll$" -enableRule:DoNotDeleteRule -allowUntrusted

# 4. Remove app_offline.htm
Write-Host ">>> [5/5] Bringing remote site back online..." -ForegroundColor Cyan
& $msdeploy -verb:delete -dest:contentPath=$destAuth -allowUntrusted

Write-Host ">>> Deployment completed! Probing live site..." -ForegroundColor Green
Start-Sleep -Seconds 3

$curlCmd = "curl.exe -i --max-time 15 -k https://api.doctoryconnect.online"
Write-Host ">>> Running: $curlCmd" -ForegroundColor Cyan
cmd.exe /c $curlCmd

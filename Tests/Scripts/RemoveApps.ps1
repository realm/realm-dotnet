$existingApps = (& realm-cli --profile deployment -f json -y apps list --disable-colors) | ConvertFrom-Json
$existingApps.data | ForEach-Object {
    $appId = $_.Split(" ")[0]
    & realm-cli --profile deployment -f json -y --disable-colors apps delete -a $appId
}
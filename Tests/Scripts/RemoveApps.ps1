param (
    [Parameter(Mandatory=$true)][string]$ClusterName
)

$existingApps = (& realm-cli --profile deployment -f json -y apps list --disable-colors) | ConvertFrom-Json
if ($existingApps.data) {
    $existingApps.data | ForEach-Object {
        $appId = $_.Split(" ")[0]
        if ($appId -Match $ClusterName) {
            & realm-cli --profile deployment -f json -y --disable-colors apps delete -a $appId
        }
    }
}

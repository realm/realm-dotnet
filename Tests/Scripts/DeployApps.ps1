param (
    [Parameter(Mandatory=$true)][string]$ClusterName
)

$tempPath = [IO.Path]::Combine($PSScriptRoot, "..", "temp")
Copy-Item -Path ([IO.Path]::Combine($PSScriptRoot, "..", "TestApps")) -Destination "$tempPath/" -Recurse

$apps = @{}
Get-ChildItem –Path $tempPath | Foreach-Object {
    $appName = "$ClusterName-$($_.Name)"
    Write-Output "Creating app $appName..."

    $createResponse = (& realm-cli --profile deployment -f json -y --disable-colors apps create --name $appName) | ConvertFrom-Json

    Remove-Item $createResponse.doc.filepath -Recurse

    $appId = $createResponse.doc.client_app_id
    Write-Output "Created app $appName with Id: $($appId)"

    $secrets = Get-Content ([IO.Path]::Combine($_.FullName, "secrets.json")) | ConvertFrom-Json
    $secrets.PSObject.Properties | ForEach-Object {
        if ($_.Name -ne "BackingDB_uri") {
            Write-Output "Importing secret $($_.Name)"

            & realm-cli --profile deployment -f json -y --disable-colors secrets create --app $appId --name "$($_.Name)" --value "$($_.Value)"
        }
    }

    $backingDBConfigPath = [IO.Path]::Combine($_.FullName, "services", "BackingDB", "config.json")
    $backingDBConfig = Get-Content -Path $backingDBConfigPath | ConvertFrom-Json
    $backingDBConfig.type = "mongodb-atlas"
    $backingDBConfig.config | Add-Member -MemberType NoteProperty -Name "clusterName" -Value $ClusterName -Force
    $backingDBConfig.PSObject.Properties.Remove("secret_config")

    $backingDBConfig | ConvertTo-Json -Depth 100 | Set-Content $backingDBConfigPath

    Write-Output "Updated BackingDB config with cluster $ClusterName"

    & realm-cli --profile deployment -f json -y --disable-colors push --local $_.FullName --remote $appId

    Write-Output "Imported $appName successfully"

    $apps.add($_.Name, $appId)
}

$bytes = [System.Text.Encoding]::Utf8.GetBytes(($apps | ConvertTo-Json))
$encoded = [Convert]::ToBase64String($bytes)

Write-Output "Command line string: --baasurl https://realm-dev.mongodb.com --baasapps $encoded"

Remove-Item -Path $tempPath -Force -Recurse
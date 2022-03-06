param (
    [Parameter(Mandatory=$true)][string]$ClusterName,
    [Parameter(Mandatory=$true)][string]$ApiKey,
    [Parameter(Mandatory=$true)][string]$PrivateApiKey,
    [string]$ProjectId = "615edc84cfa49877e3fb9503"
)

$data = @{
    name = $ClusterName
    providerSettings = @{
        instanceSizeName = "M5"
        providerName = "TENANT"
        regionName = "US_EAST_1"
        backingProviderName = "AWS"
    }
}

$data | ConvertTo-Json -Compress | curl --user "${ApiKey}:${PrivateApiKey}" --digest `
--header 'Content-Type: application/json' `
--include `
--request POST "https://cloud-dev.mongodb.com/api/atlas/v1.0/groups/$ProjectId/clusters" `
-d "@-"

$attempt = 0
while ($attempt++ -lt 200) {
    Start-Sleep -Seconds 5

    $clusterResponse = curl --user "${ApiKey}:${PrivateApiKey}" --digest `
    --header 'Content-Type: application/json' `
    --request GET "https://cloud-dev.mongodb.com/api/atlas/v1.0/groups/$ProjectId/clusters/$ClusterName"

    $state = ($clusterResponse | ConvertFrom-Json).stateName

    if ($state -eq "IDLE") {
        Write-Output "Cluster created and ready after $($attempt * 5) seconds"
        break
    }

    Write-Output "Cluster state is $state after $($attempt * 5) seconds. Waiting 5 seconds for IDLE"
}

Write-Output "Command line: --baasurl=https://realm-dev.mongodb.com --baascluster=$ClusterName --baasapikey=$ApiKey --baasprivateapikey=$PrivateApiKey --baasprojectid=$ProjectId"
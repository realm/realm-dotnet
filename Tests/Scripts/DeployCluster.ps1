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
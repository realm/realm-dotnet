param (
    [Parameter(Mandatory=$true)][string]$ClusterName,
    [Parameter(Mandatory=$true)][string]$ApiKey,
    [Parameter(Mandatory=$true)][string]$PrivateApiKey,
    [string]$ProjectId = "615edc84cfa49877e3fb9503"
)

curl --user "${ApiKey}:${PrivateApiKey}" --digest `
--header 'Content-Type: application/json' `
--include `
--request DELETE "https://cloud-dev.mongodb.com/api/atlas/v1.0/groups/$ProjectId/clusters/$ClusterName"
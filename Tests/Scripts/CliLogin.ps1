param (
    [Parameter(Mandatory=$true)][string]$ApiKey,
    [Parameter(Mandatory=$true)][string]$PrivateApiKey
)

& realm-cli --profile deployment -f json -y --disable-colors login --api-key $ApiKey --private-api-key $PrivateApiKey --atlas-url "https://cloud-dev.mongodb.com" --realm-url "https://realm-dev.mongodb.com"

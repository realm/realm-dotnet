Get-ChildItem -Path "$($PSScriptRoot)/pkl-workflows" -Filter *.pkl -File -Name | ForEach-Object {
    &pkl eval "$($PSScriptRoot)/pkl-workflows/$($_)" -o "$($PSScriptRoot)/workflows/$($_.Replace('pkl', 'yml'))"
}
Get-ChildItem -Path './pkl-workflows' -Filter *.pkl -File -Name | ForEach-Object {
    &pkl eval ./pkl-workflows/$_ -o "./workflows/$($_.Replace('pkl', 'yml'))"
}

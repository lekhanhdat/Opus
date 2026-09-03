param([string]$workspace='',
	  [string]$config='')

Write-Host "# Modify static code analysis config."
$content = Get-Content -Path $config
Clear-Content -Path $config
$newcontent = $content.Replace('${workspace}',$workspace)
Add-Content -Path $config -Value $newcontent -Encoding UTF8
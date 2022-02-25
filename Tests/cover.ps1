$coverageDir = "../Coverage"
$coverageXml = "$coverageDir/Data.opencover.xml"
$reportDir = ".coverage-report"

dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput="$coverageXml"
reportgenerator "-reports:$coverageXml" "-targetdir:$reportDir" -reporttypes:HTML;
Invoke-Expression "$reportDir/index.html"
dotnet test ./TimeTracker.sln /p:CollectCoverage=true /p:CoverletOutput=../coverage\ /p:CoverletOutputFormat=cobertura

dotnet %userprofile%\.nuget\packages\reportgenerator\5.0.2\tools\netcoreapp3.1\ReportGenerator.dll ^
    "-reports:coverage/coverage.cobertura.xml" ^
    "-targetdir:coverage" ^
    -reporttypes:Html

start .\coverage\index.html\

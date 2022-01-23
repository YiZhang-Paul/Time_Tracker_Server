dotnet test ./TimeTracker.sln --collect:"XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage
start .\coverage\index.html

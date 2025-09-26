# Based on: https://andrewlock.net/creating-a-source-generator-part-3-integration-testing-and-packaging/

# Read the version from the version.txt file
$version = Get-Content .\artifacts\version.txt | Select-Object -First 1

# Restore, build, and test with the version property
dotnet restore `
    .\test\LibStored.Net.NugetIntegrationTests\ `
    --packages ./packages `
    --configfile "nuget.integration-tests.config" `
    /p:LibStoredNetVersion=$version

dotnet build `
    .\test\LibStored.Net.NugetIntegrationTests\ `
    -c Release `
    --packages ./packages `
    --no-restore

dotnet test `
    .\test\LibStored.Net.NugetIntegrationTests\ `
    -c Release `
    --no-restore `
    --no-build
nuget pack -NoPackageAnalysis

Move-Item .\VVVV.EDSDK.*.nupkg ..\..\..\LocalNuGet\ -Force
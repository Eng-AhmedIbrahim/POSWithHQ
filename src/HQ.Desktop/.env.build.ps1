$env:MSBuildEnableWorkloadResolver = "false"
dotnet build E:\PointOfSaleProject_withKDS_withWearhouse_HQ\src\HQ.Desktop\HQ.Desktop.csproj -c Debug 2>&1 | Select-Object -Last 60

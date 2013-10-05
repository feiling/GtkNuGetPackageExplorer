@echo Off
set config=%1
if "%config%" == "" (
   set config=debug
)

.nuget\NuGet.exe restore GtkNuGetPackageExplorer.sln
msbuild %~dp0\GtkNuGetPackageExplorer.sln /p:Configuration="%config%" /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
set %ROOTDIR%=NetXP

set source=-Source https://api.nuget.org/v3/index.json
set pack=dotnet pack --no-build --no-restore -o Output /p:OutputPath=Output\

REM git clone https://github.com/juandrn/NetXP.git
REM git -C NetXP pull origin devel
REM git -C NetXP pull origin master 
REM git -C NetXP merge --no-ff --no-commit origin/devel 
REM git -C NetXP push origin master

cd NetXP.DependencyInjection
if exist Output del /Q /F /S Output
dotnet build -c Release -o Output
%pack%
cd Output
c:\bin\nuget push *.nupkg %source%

cd ..\..\

cd NetXP.NetStandard 
if exist Output del /Q /F /S Output
dotnet build -c Release -o Output
%pack%
cd Output
c:\bin\nuget push *.nupkg %source%

cd ..\..\

cd NetXP.Auditory.Implementations
if exist Output del /Q /F /S Output
dotnet build -c Release -o Output
%pack%
cd Output
c:\bin\nuget push *.nupkg %source%
cd ..\..\

cd NetXP.Cryptography.Implementations
if exist Output del /Q /F /S Output
dotnet build -c Release -o Output
%pack%
cd Output
c:\bin\nuget push *.nupkg %source%
cd ..\..

cd NetXP.DependencyInjection.Implementations.StructureMaps
if exist Output del /Q /F /S Output
dotnet build -c Release -o Output
%pack%
cd Output
c:\bin\nuget push *.nupkg %source%
cd ..\..\

cd NetXP.DependencyInjection.Implementations.UnityDI
 if exist Output del /Q /F /S Output
dotnet build -c Release -o Output
%pack%
cd Output
c:\bin\nuget push *.nupkg %source%
cd ..\..\

cd NetXP.Mappers.Implementations
 if exist Output del /Q /F /S Output
dotnet build -c Release -o Output
%pack%
cd Output
c:\bin\nuget push *.nupkg %source%
cd ..\..\

cd NetXP.MVVM
 if exist Output del /Q /F /S Output
dotnet build -c Release -o Output
%pack%
cd Output
c:\bin\nuget push *.nupkg %source%
cd ..\..\

cd NetXP.MVVM.XamarinForms
 if exist Output del /Q /F /S Output
dotnet build -c Release -o Output
%pack%
cd Output
c:\bin\nuget push *.nupkg %source%
cd ..\..\

cd NetXP.NetCore
 if exist Output del /Q /F /S Output
dotnet build -c Release -o Output
%pack%
cd Output
c:\bin\nuget push *.nupkg %source%
cd ..\..\


cd NetXP.Network
 if exist Output del /Q /F /S Output
dotnet build -c Release -o Output
%pack%
cd Output
c:\bin\nuget push *.nupkg %source%
cd ..\..\

cd NetXP.Network.Services.Implementations
 if exist Output del /Q /F /S Output
dotnet build -c Release -o Output
%pack%
cd Output
c:\bin\nuget push *.nupkg %source%
cd ..\..\

cd NetXP.Serialization.Implementations
 if exist Output del /Q /F /S Output
dotnet build -c Release -o Output
%pack%
cd Output
c:\bin\nuget push *.nupkg %source%
cd ..\..\


pause 

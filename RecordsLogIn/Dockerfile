FROM mcr.microsoft.com/dotnet/framework/aspnet:4.7.2-20210209-windowsservercore-ltsc2019
SHELL ["cmd", "/S", "/C"]
RUN %windir%\system32\inetsrv\appcmd.exe set AppPool DefaultAppPool -processModel.identityType:LocalSystem
COPY . /inetpub/wwwroot/
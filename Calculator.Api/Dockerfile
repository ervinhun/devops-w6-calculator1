FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Calculator.sln ./
COPY Calculator/Calculator.csproj Calculator/
COPY Calculator.Api/Calculator.Api.csproj Calculator.Api/
RUN dotnet restore Calculator.Api/Calculator.Api.csproj

COPY Calculator/ Calculator/
COPY Calculator.Api/ Calculator.Api/
RUN dotnet publish Calculator.Api/Calculator.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Calculator.Api.dll"]


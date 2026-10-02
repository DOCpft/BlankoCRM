FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/BlankoCRM.Core/BlankoCRM.Core.csproj src/BlankoCRM.Core/
COPY src/BlankoCRM.Infrastructure/BlankoCRM.Infrastructure.csproj src/BlankoCRM.Infrastructure/
COPY src/BlankoCRM.Application/BlankoCRM.Application.csproj src/BlankoCRM.Application/
COPY src/BlankoCRM.Web/BlankoCRM.Web.csproj src/BlankoCRM.Web/
RUN dotnet restore src/BlankoCRM.Web/BlankoCRM.Web.csproj

COPY src/ src/
RUN dotnet publish src/BlankoCRM.Web/BlankoCRM.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BlankoCRM.Web.dll"]

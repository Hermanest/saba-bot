FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /app
COPY SabaBot/SabaBot.csproj ./SabaBot/

WORKDIR /app/SabaBot
RUN dotnet restore

COPY SabaBot/ .
RUN dotnet publish "SabaBot.csproj" --configuration Release --runtime linux-x64 -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final

WORKDIR /app
COPY --from=build /app/publish/ .

ENTRYPOINT ["dotnet", "SabaBot.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
# Uploads liegen in /app/data/uploads. Auf Railway ein Volume auf /app/data mounten,
# sonst gehen Fotos und Logos bei jedem Deploy verloren.
RUN mkdir -p /app/data/uploads && chmod -R 777 /app/data
ENV Uploads__Path=/app/data/uploads

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AGM_API.csproj", "."]
RUN dotnet restore "./AGM_API.csproj"
COPY . .
RUN dotnet publish "./AGM_API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AGM_API.dll"]

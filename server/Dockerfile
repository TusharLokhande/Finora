
FROM  mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /src

COPY . .

RUN dotnet restore

WORKDIR /src/src/MoneyManagement.Api/

RUN ls -la
RUN dotnet publish -c Release -o /app/publish --no-restore

# =========================
# Runtime stage
# =========================
FROM  mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MoneyManagement.Api.dll"]
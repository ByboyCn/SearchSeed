FROM node:22-alpine AS frontend
WORKDIR /frontend
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci --silent
COPY frontend/ .
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY SearchSeed.Core/SearchSeed.Core.csproj SearchSeed.Core/
COPY SearchSeed.Api/SearchSeed.Api.csproj SearchSeed.Api/
RUN dotnet restore SearchSeed.Api/SearchSeed.Api.csproj
COPY SearchSeed.Core/ SearchSeed.Core/
COPY SearchSeed.Api/ SearchSeed.Api/
RUN dotnet publish SearchSeed.Api -c Release -o /app /m:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
COPY --from=frontend /frontend/dist /app/wwwroot
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SearchSeed.Api.dll"]
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["MloFinder/MloFinder.csproj", "MloFinder/"]
RUN dotnet restore "MloFinder/MloFinder.csproj"
COPY . .
WORKDIR "/src/MloFinder"
RUN dotnet build "MloFinder.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MloFinder.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MloFinder.dll"]

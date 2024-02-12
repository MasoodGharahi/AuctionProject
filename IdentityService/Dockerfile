	FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /app
EXPOSE 80

# copy all .csproj files and restore as distinct layers.   Use of the same COPY command
# for every dockerfile in the project to take advantage of docker caching
COPY AuctionProject.sln AuctionProject.sln
COPY AuctionService/AuctionService.csproj AuctionService/AuctionService.csproj
COPY SearchService/SearchService.csproj SearchService/SearchService.csproj
COPY GatewayService/GatewayService.csproj GatewayService/GatewayService.csproj
COPY Contracts/Contracts.csproj Contracts/Contracts.csproj
COPY IdentityService/IdentityService.csproj IdentityService/IdentityService.csproj
COPY Tests/AuctionProjectUnitTests/AuctionProjectUnitTests.csproj Tests/AuctionProjectUnitTests/AuctionProjectUnitTests.csproj

# Restore package deps
RUN dotnet restore AuctionProject.sln

# Copy the app folders over
COPY IdentityService IdentityService
WORKDIR /app/IdentityService
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT [ "dotnet", "IdentityService.dll" ]
# cmd=> docker compose build identity-svc
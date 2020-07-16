#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Pi.Replicate.WebUi/Pi.Replicate.WebUi.csproj", "Pi.Replicate.WebUi/"]
COPY ["Pi.Replicate.Domain/Pi.Replicate.Domain.csproj", "Pi.Replicate.Domain/"]
COPY ["Pi.Replicate.Application/Pi.Replicate.Application.csproj", "Pi.Replicate.Application/"]
COPY ["Pi.Replicate.Shared/Pi.Replicate.Shared.csproj", "Pi.Replicate.Shared/"]
COPY ["Pi.Replicate.Data/Pi.Replicate.Data.csproj", "Pi.Replicate.Data/"]
RUN dotnet restore "Pi.Replicate.WebUi/Pi.Replicate.WebUi.csproj"
COPY . .
WORKDIR "/src/Pi.Replicate.WebUi"
RUN dotnet build "Pi.Replicate.WebUi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pi.Replicate.WebUi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /app/publish/pidev.pfx /home/certs/pidev.pfx
COPY --from=publish /app/publish/rootca.crt /etc/ssl/certs/pirootca.crt
RUN update-ca-certificates
ENTRYPOINT ["dotnet", "Pi.Replicate.WebUi.dll"]
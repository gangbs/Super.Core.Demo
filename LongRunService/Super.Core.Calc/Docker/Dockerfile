#FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
#WORKDIR /app
#EXPOSE 80
#
#FROM microsoft/dotnet:2.2-sdk AS build
#WORKDIR /src
#COPY ["Super.Core.Api/Super.Core.Api.csproj", "Super.Core.Api/"]
#COPY ["Super.Core.Infrastruct/Super.Core.Infrastruct.csproj", "Super.Core.Infrastruct/"]
#RUN dotnet restore "Super.Core.Api/Super.Core.Api.csproj"
#COPY . .
#WORKDIR "/src/Super.Core.Api"
#RUN dotnet build "Super.Core.Api.csproj" -c Release -o /app
#
#FROM build AS publish
#RUN dotnet publish "Super.Core.Api.csproj" -c Release -o /app
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app .
#ENTRYPOINT ["dotnet", "Super.Core.Api.dll"]


FROM microsoft/dotnet:2.2-aspnetcore-runtime
MAINTAINER yangyangang
WORKDIR /app
COPY . .
VOLUME super.core.mvc /app/Config
EXPOSE 6060
ENTRYPOINT ["dotnet", "Super.Core.Api.dll"]
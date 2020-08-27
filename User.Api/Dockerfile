#基础运行镜像 
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 8001
#用于编译的SDK镜像
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["User.API.csproj", "/"]
RUN dotnet restore "/User.API.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "User.API.csproj" -c Release -o /app/build
#发布
FROM build AS publish
RUN dotnet publish "User.API.csproj" -c Release -o /app/publish
#运行
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "User.API.dll"]

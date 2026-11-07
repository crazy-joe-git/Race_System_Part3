FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Race_System(2).csproj", "./"]
RUN dotnet restore "Race_System(2).csproj"

COPY . .
RUN dotnet build "Race_System(2).csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Race_System(2).csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Race_System(2).dll"]
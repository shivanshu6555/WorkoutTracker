# Stage 1: Build the application (Updated to .NET 9.0)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["WorkoutTracker.csproj", "./"]
RUN dotnet restore "WorkoutTracker.csproj"

COPY . .
RUN dotnet publish "WorkoutTracker.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "WorkoutTracker.dll"]
# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["WorkoutTracker.csproj", "./"]
RUN dotnet restore "WorkoutTracker.csproj"

# Copy the rest of the code and build
COPY . .
RUN dotnet publish "WorkoutTracker.csproj" -c Release -o /app/publish

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Render expects web services to listen on a specific port. .NET 8 defaults to 8080 in containers.
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "WorkoutTracker.dll"]
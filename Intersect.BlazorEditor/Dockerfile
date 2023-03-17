FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Intersect.BlazorEditor/Intersect.BlazorEditor.csproj", "Intersect.BlazorEditor/"]
RUN dotnet restore "Intersect.BlazorEditor/Intersect.BlazorEditor.csproj"
COPY . .
WORKDIR "/src/Intersect.BlazorEditor"
RUN dotnet build "Intersect.BlazorEditor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Intersect.BlazorEditor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Intersect.BlazorEditor.dll"]

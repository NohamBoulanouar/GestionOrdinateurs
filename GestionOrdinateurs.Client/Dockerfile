# =========================
# Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copie des csproj seuls pour restaurer les packages dans une couche mise en cache
COPY GestionOrdinateurs.Client/GestionOrdinateurs.Client.csproj ./GestionOrdinateurs.Client/
COPY GestionOrdinateurs.Shared/GestionOrdinateurs.Shared.csproj ./GestionOrdinateurs.Shared/
RUN dotnet restore ./GestionOrdinateurs.Client

# Copie du reste du code source puis publication
COPY ./GestionOrdinateurs.Client/ ./GestionOrdinateurs.Client/
COPY ./GestionOrdinateurs.Shared/ ./GestionOrdinateurs.Shared/
RUN dotnet publish ./GestionOrdinateurs.Client -c Release -o out --no-restore

# =========================
# Runtime : le front est statique, nginx sert les fichiers publies
# =========================
FROM nginx:alpine AS runtime
COPY --from=build /app/out/wwwroot /usr/share/nginx/html
COPY GestionOrdinateurs.Client/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 5000

# =============================================
# Multi-stage Dockerfile para Angular 17
# =============================================

# Stage 1: Build Angular
FROM node:20-alpine AS build
WORKDIR /app

# Copiar package.json primero para cache de dependencias
COPY package.json package-lock.json ./
RUN npm ci

# Copiar el resto del código fuente
COPY . .

# Build de producción
RUN npm run build -- --configuration production

# Verificar que se generó el output
RUN ls -la /app/dist/rd/browser/ || (echo "ERROR: Angular build output not found" && exit 1)

# Stage 2: Servir con Nginx
FROM nginx:1.25-alpine

# Eliminar archivos default de nginx
RUN rm -rf /usr/share/nginx/html/*

# Copiar config de nginx (se monta via volumen en docker-compose)
# Pero dejamos una default por si acaso
COPY nginx-app.conf /etc/nginx/conf.d/default.conf

# Copiar los archivos compilados de Angular (Angular 17+ usa /browser/)
COPY --from=build /app/dist/rd/browser /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
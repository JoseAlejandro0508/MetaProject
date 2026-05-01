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

# Stage 2: Servir con Nginx
FROM nginx:1.25-alpine

# Copiar config de nginx (se monta via volumen en docker-compose)
# Pero dejamos una default por si acaso
COPY nginx-app.conf /etc/nginx/conf.d/default.conf

# Copiar los archivos compilados de Angular
COPY --from=build /app/dist/rd /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
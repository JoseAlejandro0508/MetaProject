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

# Eliminar TODOS los archivos default de nginx completamente
RUN rm -rf /usr/share/nginx/html/* \
    && rm -f /etc/nginx/conf.d/default.conf

# Copiar config de nginx (se monta via volumen en docker-compose)
COPY nginx-app.conf /etc/nginx/conf.d/default.conf

# Copiar DIRECTAMENTE los archivos de Angular al root(NO dentro de /browser)
COPY --from=build /app/dist/rd/browser/ /usr/share/nginx/html/

# Verificar que el index.html es de Angular, no de nginx
RUN grep -q '<app-root>' /usr/share/nginx/html/index.html || (echo "ERROR: Angular index.html not found or invalid" && exit 1)

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
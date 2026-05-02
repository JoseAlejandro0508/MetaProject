# Meta-xi 🚀

Aplicación full-stack: **Angular 17** (frontend) + **.NET 8** (backend) + **SQLite** (base de datos), desplegada con Docker.

---

## 📁 Estructura del Proyecto

```
MetaProject/
├── docker-compose.yml          # Orquestación de contenedores
├── nginx.conf                  # Reverse proxy (Angular + /api → backend)
├── .env                        # Variables de entorno (NO subir a git)
├── .gitignore
│
├── Meta-xi-Api-main/
│   └── Meta-xi-Api-main/       # Backend .NET 8
│       ├── Controllers/
│       ├── Model/
│       ├── Class/
│       ├── Databases/
│       ├── Migrations/          # ⚠️ Borrar y recrear con dotnet ef
│       ├── Program.cs
│       ├── Dockerfile
│       └── Meta-xi.csproj
│
├── Meta-xi-Client-main/
│   └── Meta-xi-Client-main/     # Frontend Angular 17
│       ├── src/
│       │   ├── app/
│       │   └── environments/     # Config por ambiente (dev vs prod)
│       ├── Dockerfile
│       ├── nginx-app.conf
│       └── angular.json
│
└── Data/                         # SQLite DB (se crea automáticamente)
    └── metaxi.db
```

---

## 🏗️ Arquitectura

```
Internet → VPS (puerto 80)
    └── client (nginx)
          ├── /          → Angular SPA (archivos estáticos)
          ├── /api/*     → proxy_pass http://api:5071/api/*
          └── /swagger/*  → proxy_pass http://api:5071/swagger/*
    
    └── api (.NET 8)
          └── SQLite ( /app/Data/metaxi.db )
```

---

## ⚡ Comandos Docker

### Levantar el proyecto

```bash
# Buildear y levantar todos los servicios (primera vez o después de cambios)
docker compose up -d --build

# Levantar sin rebuild (si no hubo cambios en el código)
docker compose up -d
```

### Detener el proyecto

```bash
# Detener todos los contenedores
docker compose down

# Detener y ELIMINAR volúmenes (⚠️ BORRA la base de datos!)
docker compose down -v
```

### Ver estado y logs

```bash
# Ver qué contenedores están corriendo
docker compose ps

# Ver logs de todos los servicios
docker compose logs

# Ver logs en tiempo real
docker compose logs -f

# Ver logs solo del backend
docker compose logs -f api

# Ver logs solo del frontend
docker compose logs -f client

# Ver las últimas 100 líneas de logs
docker compose logs --tail 100 api
```

### Reconstruir después de cambios

```bash
# Reconstruir todo
docker compose up -d --build

# Reconstruir solo el backend
docker compose up -d --build api

# Reconstruir solo el frontend
docker compose up -d --build client
```

### Reiniciar un servicio

```bash
# Reiniciar el backend
docker compose restart api

# Reiniciar el frontend
docker compose restart client
```

### Acceder al contenedor

```bash
# Entrar al backend (shell)
docker compose exec api /bin/bash

# Entrar al frontend (shell)
docker compose exec client /bin/sh

# Ver la base de datos SQLite
docker compose exec api /bin/bash
# Luego dentro del contenedor:
sqlite3 /app/Data/metaxi.db ".tables"
```

### Limpiar todo y empezar de cero

```bash
# ⚠️ Elimina contenedores, imágenes, volúmenes y redes
docker compose down -v --rmi all

# También elimina imágenes huérfanas
docker system prune -a
```

---

## 🔧 Desarrollo Local

### Backend (.NET 8)

```bash
cd Meta-xi-Api-main/Meta-xi-Api-main

# Restaurar paquetes
dotnet restore

# Crear migración inicial (solo la primera vez)
dotnet ef migrations add InitialCreate

# Correr en modo desarrollo
dotnet run

# El backend corre en http://localhost:5071
# Swagger: http://localhost:5071/swagger
```

### Frontend (Angular 17)

```bash
cd Meta-xi-Client-main/Meta-xi-Client-main

# Instalar dependencias
npm install

# Correr en modo desarrollo
npm start

# El frontend corre en http://localhost:4200
```

> En modo desarrollo, el frontend apunta a `https://meta-api-production-3abd.up.railway.app/api` (ver `environment.ts`). En producción Docker, apunta a `/api` relativo (via nginx proxy).

---

## 🔐 Seguridad

### ⚠️ ANTES de desplegar en producción:

1. **Cambiar el JWT Secret** — Genera uno nuevo en `.env`:
   ```bash
   # Generar un secreto aleatorio de 64 caracteres
   openssl rand -base64 64
   ```
   Pegar el resultado en `.env` → `JWT_SECRET=tu-secreto-aqui`

2. **Rotar credenciales expuestas** — Si este repo fue público, rotar:
   - Contraseña de la base de datos anterior (Railway)
   - Token del bot de Telegram (estaba en el frontend)
   - JWT Secret anterior

3. **HTTPS** — Instalar Certbot:
   ```bash
   apt install certbot python3-certbot-nginx
   certbot --nginx -d tu-dominio.com
   ```

4. **Swagger en producción** — Deshabilitar removiendo `|| app.Environment.IsProduction()` en `Program.cs`

---

## 🗄️ Base de Datos (SQLite)

- La DB se crea automáticamente en `/app/Data/metaxi.db` dentro del contenedor
- Se persiste vía Docker volume (`sqlite-data`)
- Al primer arranque, `dbContext.Database.Migrate()` crea las tablas automáticamente
- No requiere configuración externa ni servidor de base de datos

### Backup de la base de datos

```bash
# Copiar la DB del contenedor al host
docker compose exec api /bin/bash -c "sqlite3 /app/Data/metaxi.db '.backup /app/Data/backup.db'"
docker compose cp api:/app/Data/backup.db ./backup-$(date +%Y%m%d).db
```

### Restaurar backup

```bash
# Copiar backup al contenedor
docker compose cp ./backup-20240101.db api:/app/Data/backup.db

# Restaurar (dentro del contenedor)
docker compose exec api /bin/bash -c "cp /app/Data/backup.db /app/Data/metaxi.db && dotnet /app/Meta-xi.dll"
```

---

## 📝 Variables de Entorno (.env)

| Variable | Descripción | Default |
|----------|-------------|---------|
| `JWT_SECRET` | Secreto para firmar tokens JWT | *(cambiar en producción!)* |
| `ASPNETCORE_ENVIRONMENT` | Ambiente .NET | `Production` |
| `ASPNETCORE_URLS` | URL del backend | `http://+:5071` |

---

## 🌐 URLs

| Servicio | URL Local | URL Producción |
|----------|-----------|---------------|
| Frontend | `http://localhost:80` | `http://tu-dominio.com` |
| Backend API | `http://localhost:5071/api` | `http://tu-dominio.com/api` |
| Swagger | `http://localhost:5071/swagger` | `http://tu-dominio.com/swagger` |

---

## 🛠️ Troubleshooting

### El frontend no conecta con el backend
```bash
# Verificar que el api está corriendo
docker compose logs api

# Verificar la configuración de nginx
docker compose exec client cat /etc/nginx/conf.d/default.conf
```

### La base de datos no se crea
```bash
# Verificar que el volumen existe
docker volume ls | grep sqlite

# Eliminar y recrear (⚠️ pierde datos)
docker compose down -v
docker compose up -d --build
```

### Ver el contenido de Angular servido
```bash
docker compose exec client ls /usr/share/nginx/html
```

### Error de CORS
El backend ya tiene CORS configurado para permitir cualquier origen (`AllowAnyOrigin`). Si hay problemas, verificar los logs del backend:
```bash
docker compose logs api | grep -i cors
```

---

## 📜 Stack Tecnológico

- **Frontend**: Angular 17, TailwindCSS, ngx-toastr
- **Backend**: .NET 8, Entity Framework Core, SQLite
- **Autenticación**: JWT Bearer
- **Contenedores**: Docker, docker-compose, nginx (reverse proxy)
- **Base de datos**: SQLite (persistida en Docker volume)
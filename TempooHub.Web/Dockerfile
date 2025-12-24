# Etapa 1: Compilación (Node.js)
FROM node:20-alpine as build
WORKDIR /app

# Copiar package.json y restaurar dependencias
COPY package*.json ./
RUN npm install

# Copiar el código fuente
COPY . .

# Compilar la app (asegúrate de que el script 'build' exista en package.json)
RUN npm run build

# Etapa 2: Servidor Web (Nginx)
FROM nginx:alpine

# 1. Copiar los archivos compilados de Angular
COPY --from=build /app/dist/TempooHub /usr/share/nginx/html

# 2. COPIAR LA PLANTILLA A LA CARPETA MÁGICA DE NGINX
# Nginx buscará archivos .template aquí, sustituirá las variables 
# y pondrá el resultado final en /etc/nginx/conf.d/default.conf automáticamente.
COPY default.conf.template /etc/nginx/templates/default.conf.template

# Definir variables por defecto (por si se te olvida ponerlas en el docker-compose)
ENV PORT=80
ENV BACKEND_HOST=gateway
ENV BACKEND_PORT=8080

EXPOSE 80
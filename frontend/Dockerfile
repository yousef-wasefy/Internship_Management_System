# Multi-stage build: Node only exists to run `npm run build`; the final image is nginx
# serving the resulting static files (index.html + JS/CSS bundle) - there's no Node
# runtime, and no source code, in the image that actually runs.

# ---- Build stage ----
FROM node:24-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

# ---- Runtime stage ----
FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

# The actual API URL is injected at container *startup*, not baked in above - see
# docs/DECISIONS.md D22. env-config.template.js has the placeholder;
# 40-generate-env-config.sh (run automatically by the base image's own entrypoint,
# which executes every script under /docker-entrypoint.d/) turns it into the
# env-config.js the browser loads, using whatever API_BASE_URL this container starts
# with - the same image works in Docker Compose, on Render, or anywhere else.
COPY docker/env-config.template.js /usr/share/nginx/html/env-config.template.js
COPY docker/40-generate-env-config.sh /docker-entrypoint.d/40-generate-env-config.sh
RUN chmod +x /docker-entrypoint.d/40-generate-env-config.sh
ENV API_BASE_URL=/api

EXPOSE 80

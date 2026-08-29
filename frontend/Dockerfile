# Multi-stage build: Node only exists to run `npm run build`; the final image is nginx
# serving the resulting static files (index.html + JS/CSS bundle) - there's no Node
# runtime, and no source code, in the image that actually runs.

# ---- Build stage ----
FROM node:24-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .

# Vite bakes VITE_* variables into the built JS at build time, not read at container
# startup - so this has to be a build ARG, not a runtime environment variable. "/api" is
# a same-origin relative path: nginx.conf below reverse-proxies it to the backend
# container, so the browser only ever talks to one origin and needs no CORS here (unlike
# the separate-origin `npm run dev` workflow in .env.development, which still needs it -
# see docs/DECISIONS.md D21).
ARG VITE_API_BASE_URL=/api
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

# ---- Runtime stage ----
FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80

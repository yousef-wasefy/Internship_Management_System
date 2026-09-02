// Default for `npm run dev` and as a build-time placeholder in dist/ - a container
// deployment (Docker Compose, Render) overwrites this exact file at startup with a real
// API_BASE_URL (see docker-entrypoint.d/40-generate-env-config.sh). Left empty here so
// client.ts's fallback to the Vite build-time variable (.env.development) takes over
// whenever this hasn't been overwritten - see docs/DECISIONS.md D22.
window.__ENV__ = {};

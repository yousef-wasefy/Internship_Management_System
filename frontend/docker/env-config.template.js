// Template for the runtime env-config pattern (see docs/DECISIONS.md D22). Not served
// directly and not copied into the Vite build (lives outside src/ and public/) -
// docker-entrypoint.d/40-generate-env-config.sh runs `envsubst` over this at container
// *startup*, writing the result to /usr/share/nginx/html/env-config.js, overwriting the
// empty build-time default from public/env-config.js.
window.__ENV__ = {
  API_BASE_URL: "${API_BASE_URL}"
};

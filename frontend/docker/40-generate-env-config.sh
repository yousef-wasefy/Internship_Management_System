#!/bin/sh
# The official nginx image runs every executable script in /docker-entrypoint.d/ (in
# filename order) before starting nginx - this one exists purely to turn
# env-config.template.js into the env-config.js the browser actually loads, using
# whatever API_BASE_URL this specific container was started with. See
# docs/DECISIONS.md D22 for why this happens at container startup, not image build time.
set -eu

envsubst '${API_BASE_URL}' \
  < /usr/share/nginx/html/env-config.template.js \
  > /usr/share/nginx/html/env-config.js

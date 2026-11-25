#!/bin/sh
set -e

chown -R app:app /data 2>/dev/null || true

cd /app
exec runuser -u app -- dotnet CustomerFeedback.Api.dll
#!/usr/bin/env bash
set -euo pipefail

dotnet build src/VitaCare.sln -m:1 -v:minimal
dotnet test src/VitaCare.sln -m:1 --no-build -v:minimal

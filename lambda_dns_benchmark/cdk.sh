#!/bin/sh
set -e

# Run the cdk in the local node_modules dir.

cd `dirname $0`
exec npm run cdk -- "$@"

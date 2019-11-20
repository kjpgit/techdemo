#!/bin/sh
set -e
set -u
cd `dirname $0`

# Usage: deploy.sh (Typically Jenkins will call this script to build/deploy)
#
# Prereq: GITHUB_ACCESS_TOKEN must be set, to download (read-only) your
# company-private dependencies from github
#
# Prereq: This should run on an EC2 machine with an IAM instance role, so
# the CDK (running inside docker) can get AWS creds to call cloudformation

# NB: Ensure jenkins does not do parallel builds
STACK_NAME=acme-prod-DemoLambda
IMAGE_NAME=acme-demolambda-deploy
CONFIG_FILE=../source/config.txt

set_config_entry()
{
    echo "$1" >> $CONFIG_FILE
}

# I like having a config file for my lambdas, since you can exhaust Lambda env var limits
# Jenkins should give us a clean workspace dir, but be safe
#rm -f $CONFIG_FILE
#set_config_entry "ACME_DB_WRITER_HOST=$ACME_DB_WRITER_HOST"
#set_config_entry "ACME_DB_READER_HOST=$ACME_DB_READER_HOST"
#set_config_entry "ACME_DB_USER=$ACME_DB_USER"

docker build -f Dockerfile \
    --build-arg GITHUB_ACCESS_TOKEN=$GITHUB_ACCESS_TOKEN \
    -t $IMAGE_NAME ../

# Don't allocate a tty; it adds windows newlines(!)
docker run --rm $IMAGE_NAME \
    cdk deploy $STACK_NAME --require-approval never

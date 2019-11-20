# Builds the application code, and runs the CDK to deploy it
#
FROM ubuntu:18.04

ARG GITHUB_ACCESS_TOKEN

# Bootstrap image (cacheable)
# TODO: Switch to Amazon Linux, especially if you are building python C-extensions for lambda
RUN apt-get update && \
    apt-get -y --no-install-recommends -q install python3.6 python3-pip python3-venv python3-setuptools \
        git curl xz-utils && \
    rm -rf /var/lib/apt/lists/*

# CDK and dependencies
WORKDIR /deploy
RUN curl https://nodejs.org/dist/v10.16.3/node-v10.16.3-linux-x64.tar.xz | tar -xJf -
ENV PATH="/deploy/node-v10.16.3-linux-x64/bin:${PATH}"
RUN npm install -g aws-cdk
COPY deploy/requirements.txt ./
RUN cat requirements.txt | pip3 --no-cache install -r /dev/stdin

# Application dependencies
# This supports your company's private github repos
# GITHUB_ACCESS_TOKEN should have read-only access to the code.
WORKDIR /app
COPY source/requirements.txt ./
RUN sed "s|git+ssh://git@github.com|git+https://$GITHUB_ACCESS_TOKEN@github.com|" requirements.txt \
    | pip3 --no-cache install -r /dev/stdin --no-deps --target /app/layer/python --system

# All the other code/scripts.  These change often, do them last
COPY source/. /app/code/
COPY deploy/. /deploy/

# Python unicode breakage without this
ENV LANG=C.UTF-8

WORKDIR /deploy
CMD [ "/bin/bash" ]


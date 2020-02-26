FROM mcr.microsoft.com/dotnet/core/sdk:2.1

RUN apt-get update -yq && apt-get upgrade -yq && \
apt-get install -yq curl git nano jq

RUN curl -sL https://deb.nodesource.com/setup_12.x | bash - && \
apt-get install -yq nodejs

RUN npm install -g npm

RUN cd /tmp && npm install mongodb-stitch-cli

ENV PATH "${PATH}:/tmp/node_modules/mongodb-stitch-cli"
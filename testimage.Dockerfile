FROM mcr.microsoft.com/dotnet/core/sdk:2.1

RUN apt-get update -yq && apt-get upgrade -yq && \
apt-get install -yq curl git nano

RUN curl -sL https://deb.nodesource.com/setup | bash - && \
apt-get install -yq nodejs

RUN npm install -g npm stitch-cli
FROM mcr.microsoft.com/dotnet/core/sdk:2.1

apt-get update -yq && apt-get upgrade -yq && \
apt-get install -yq curl git nano

curl -sL https://deb.nodesource.com/setup | sudo bash - && \
apt-get install -yq nodejs

npm install -g npm stitch-cli
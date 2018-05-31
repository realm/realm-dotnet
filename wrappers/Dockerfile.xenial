FROM ubuntu:xenial

ARG PACKAGECLOUD_URL
ARG REALM_CORE_VERSION
ARG REALM_SYNC_VERSION

RUN apt-get update && \
    apt-get install -y \
            curl \
            cmake \
            build-essential \
            cpp \
            libssl-dev

RUN curl -s ${PACKAGECLOUD_URL}/script.deb.sh | bash
RUN apt-get install -y \
            librealm=${REALM_CORE_VERSION}-* \
            librealm-dev=${REALM_CORE_VERSION}-* \
            librealm-node-dev=${REALM_CORE_VERSION}-* \
            librealm-sync=${REALM_SYNC_VERSION}-* \
            librealm-sync-dev=${REALM_SYNC_VERSION}-* \
            librealm-sync-node-dev=${REALM_SYNC_VERSION}-*

VOLUME /source
CMD ["/source/build.sh"]

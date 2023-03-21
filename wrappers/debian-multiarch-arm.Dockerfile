FROM debian:11

RUN dpkg --add-architecture armhf && apt-get update
RUN dpkg --add-architecture arm64 && apt-get update
RUN apt-get install -y \
        build-essential \
        curl \
        crossbuild-essential-armhf \
        crossbuild-essential-arm64 \
        libprocps-dev:armhf \
        libprocps-dev:arm64 \
        libssl-dev:armhf \
        libssl-dev:arm64 \
        libz-dev:armhf \
        libz-dev:arm64 \
        libasio-dev \
        ninja-build \
        qemu-user

RUN mkdir -p /opt/cmake \
    && curl https://cmake.org/files/v3.18/cmake-3.18.2-Linux-x86_64.sh -o /cmake.sh \
    && sh /cmake.sh --prefix=/opt/cmake --skip-license \
    && rm /cmake.sh

ENV PATH "/opt/cmake/bin:$PATH"

VOLUME /source

ENTRYPOINT ["/source/build-linux.sh"]
FROM ubuntu:20.04

# Keep the packages in alphabetical order to make it easy to avoid duplication
RUN DEBIAN_FRONTEND=noninteractive apt-get update -qq \
    && apt-get install -y \
               curl \
               git \
               make \
               unzip \
    && apt-get clean

# Install CMake
RUN mkdir -p /opt/cmake \
 && curl https://cmake.org/files/v3.18/cmake-3.18.2-Linux-x86_64.sh -o /cmake.sh \
 && sh /cmake.sh --prefix=/opt/cmake --skip-license \
 && rm /cmake.sh
ENV PATH "/opt/cmake/bin:$PATH"

# Install the NDK
RUN cd /opt \
 && curl -OJ https://dl.google.com/android/repository/android-ndk-r21-linux-x86_64.zip \
 && unzip android-ndk-r21-linux-x86_64.zip \
 && rm -f android-ndk-r21-linux-x86_64.zip

ENV ANDROID_NDK_ROOT "/opt/android-ndk-r21"
ENV ANDROID_NDK_HOME "/opt/android-ndk-r21"
ENV ANDROID_NDK "/opt/android-ndk-r21"

VOLUME /source
CMD ["/source/build-android.sh"]

FROM centos:7

# Install EPEL & devtoolset
RUN yum install -y \
        epel-release \
        centos-release-scl-rh \
    && yum-config-manager --enable rhel-server-rhscl-7-rpms

RUN yum install -y \
        which \
        curl \
        make \
        devtoolset-6-gcc \
        devtoolset-6-gcc-c++ \
        devtoolset-6-binutils \
        git \
        procps-devel \
        openssh-clients \
        openssl-static \
        zlib-devel

RUN mkdir -p /opt/cmake && \
    curl -s https://cmake.org/files/v3.14/cmake-3.14.1-Linux-x86_64.sh -o /cmake.sh && \
    sh /cmake.sh --prefix=/opt/cmake --skip-license && \
    rm /cmake.sh
ENV PATH="/opt/rh/devtoolset-6/root/usr/bin:/opt/cmake/bin:${PATH}"

RUN mkdir -p /etc/ssh && \
    echo "Host github.com\n\tStrictHostKeyChecking no\n" >> /etc/ssh/ssh_config && \
    ssh-keyscan github.com >> /etc/ssh/ssh_known_hosts

VOLUME /source
CMD ["/source/build.sh"]

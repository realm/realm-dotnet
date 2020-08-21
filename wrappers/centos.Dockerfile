FROM centos:7

# Install EPEL & devtoolset
RUN yum install -y \
        epel-release \
        centos-release-scl-rh \
 && yum-config-manager --enable rhel-server-rhscl-7-rpms

RUN yum install -y \
        chrpath \
        devtoolset-9 \
        jq \
        libconfig-devel \
        openssh-clients \
        rh-git218 \
        zlib-devel \
 && yum clean all

ENV PATH /opt/cmake/bin:/opt/rh/rh-git218/root/usr/bin:/opt/rh/devtoolset-9/root/usr/bin:$PATH
ENV LD_LIBRARY_PATH /opt/rh/devtoolset-9/root/usr/lib64:/opt/rh/devtoolset-9/root/usr/lib:/opt/rh/devtoolset-9/root/usr/lib64/dyninst:/opt/rh/devtoolset-9/root/usr/lib/dyninst:/opt/rh/devtoolset-9/root/usr/lib64:/opt/rh/devtoolset-9/root/usr/lib

RUN mkdir -p /opt/cmake \
 && curl https://cmake.org/files/v3.18/cmake-3.18.2-Linux-x86_64.sh -o /cmake.sh \
 && sh /cmake.sh --prefix=/opt/cmake --skip-license \
 && rm /cmake.sh

RUN mkdir -p /etc/ssh && \
    echo "Host github.com\n\tStrictHostKeyChecking no\n" >> /etc/ssh/ssh_config && \
    ssh-keyscan github.com >> /etc/ssh/ssh_known_hosts

VOLUME /source
CMD ["/source/build.sh"]

FROM centos:7

ARG CMAKE_VERSION=3.21.3

# Add the Oracle Linux Software Collections repository
RUN echo $' \n\
[ol7_software_collections] \n\
name=Software Collection packages for Oracle Linux 7 (\$basearch) \n\
baseurl=http://yum.oracle.com/repo/OracleLinux/OL7/SoftwareCollections/\$basearch/ \n\
gpgkey=https://yum.oracle.com/RPM-GPG-KEY-oracle-ol7 \n\
gpgcheck=1 \n\
enabled=1 \n\
' > /etc/yum.repos.d/OracleLinux-Software-Collections.repo

# Add the EPEL repository
RUN yum -y install \
      epel-release

RUN yum install -y \
        chrpath \
        devtoolset-10 \
        jq \
        libconfig-devel \
        openssh-clients \
        rh-git218 \
        zlib-devel \
        ccache \
 && yum clean all

ENV PATH /opt/cmake/bin:/opt/rh/rh-git218/root/usr/bin:/opt/rh/devtoolset-9/root/usr/bin:$PATH
ENV LD_LIBRARY_PATH /opt/rh/devtoolset-9/root/usr/lib64:/opt/rh/devtoolset-9/root/usr/lib:/opt/rh/devtoolset-9/root/usr/lib64/dyninst:/opt/rh/devtoolset-9/root/usr/lib/dyninst:/opt/rh/devtoolset-9/root/usr/lib64:/opt/rh/devtoolset-9/root/usr/lib

RUN mkdir -p /opt/cmake \
 && curl https://github.com/Kitware/CMake/releases/download/v$CMAKE_VERSION/cmake-$CMAKE_VERSION-linux-x86_64.sh -o /cmake.sh \
 && sh /cmake.sh --prefix=/opt/cmake --skip-license \
 && rm /cmake.sh

RUN ccache --set-config=cache_dir=/work/.ccache \
 && ccache --set-config=compression=true

RUN mkdir -p /etc/ssh && \
    echo "Host github.com\n\tStrictHostKeyChecking no\n" >> /etc/ssh/ssh_config && \
    ssh-keyscan github.com >> /etc/ssh/ssh_known_hosts

VOLUME /source
CMD ["/source/build.sh"]

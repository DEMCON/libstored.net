# Add ARG for libstored version
ARG LIBSTORED_VERSION=1.8.0

FROM ubuntu:22.04 as builder

# Use the ARG after FROM
ARG LIBSTORED_VERSION

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        build-essential \
        git-core \
        cmake \
        pkg-config \
        python3 \
        python3-pip \
        python3-setuptools \
        doxygen \
        plantuml \
        python3-venv \
        python3-dev \
        lsb-release \
        libgl1 \
        libegl1 \
        libxkbcommon0 \
        ninja-build \
        libzmq3-dev \
        libcairo2 \
        sudo \
        wget && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /tmp

RUN wget https://github.com/DEMCON/libstored/archive/refs/tags/v${LIBSTORED_VERSION}.tar.gz && \
     tar -xzf v${LIBSTORED_VERSION}.tar.gz --transform="s/libstored-${LIBSTORED_VERSION}/libstored/" && \
     rm v${LIBSTORED_VERSION}.tar.gz && \
     cd libstored && \
     dist/ubuntu/bootstrap.sh && \
     dist/ubuntu/build.sh Debug examples zmq make notest

# Create production image
FROM ubuntu:22.04

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        libzmq5 && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Copy the examples
COPY --from=builder /tmp/libstored/dist/ubuntu/build/deploy/bin/ /app/

WORKDIR /app

CMD ["./8_sync", "-i", "container", "-d", "tcp://*:5555", "-p", "5556"]

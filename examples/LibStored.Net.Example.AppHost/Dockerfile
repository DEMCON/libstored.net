FROM ubuntu:22.04 as builder

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

# RUN wget https://github.com/DEMCON/libstored/archive/refs/tags/v1.7.1.tar.gz && \
#     tar -xzf v1.7.1.tar.gz --transform='s/libstored-1.7.1/libstored/' && \
#     rm v1.7.1.tar.gz && \
#     cd libstored && \
#     dist/ubuntu/bootstrap.sh && \
#     dist/ubuntu/build.sh Debug examples zmq make notest

# The commit that add the Dealer to SyncZmqLayer layer
RUN wget https://github.com/DEMCON/libstored/archive/a8a5b6613876511ed886733660b515c1f67f57db.tar.gz && \
    tar -xzf a8a5b6613876511ed886733660b515c1f67f57db.tar.gz --transform='s/libstored-a8a5b6613876511ed886733660b515c1f67f57db/libstored/' && \
    rm a8a5b6613876511ed886733660b515c1f67f57db.tar.gz && \
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
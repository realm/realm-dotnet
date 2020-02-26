# Docker image for the Mongo Stitch command

FROM golang:alpine AS build

# Do a system update
# No need to put this into separate steps.
RUN apk update && apk add git curl

# Declare base dir
WORKDIR $GOPATH/src/github.com/10gen/stitch-cli

# Fetch the dependencies
RUN curl https://raw.githubusercontent.com/golang/dep/master/install.sh | sh

# Master sometimes breaks, so use a release
#
# 1.5 - OK
# 1.6 - OK
# 1.7 - OK
# 1.8 - ?
# 1.9 - OK
RUN git clone https://github.com/10gen/stitch-cli.git .
RUN git checkout v1.9.0
# `dep init` required 1.8+
RUN dep init

# Remove the old dependencies
RUN rm -rf vendor

# We build in case all depencies are pulled in corrctly.
RUN dep ensure && go build

# Second stage build to just expose the command
FROM alpine:3.9

# Set new default folder
WORKDIR /project

# Do a system update
# We can run it in one step, as it is more likely that an update
# is available than that ca-certificates have changed
RUN apk update && apk add ca-certificates jq curl

COPY --from=build /go/src/github.com/10gen/stitch-cli/stitch-cli /usr/bin/

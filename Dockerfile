FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0.306@sha256:ca77338a19f87a7d24494a3656cb7d878a040c158621337b9cd3ab811c5eb057 AS build
ARG TARGETARCH

LABEL org.opencontainers.image.description="Update Hue lights to follow along with Eurovision"

COPY . /source
WORKDIR /source

SHELL ["/bin/bash", "-o", "pipefail", "-c"]

RUN apt-get update \
    && apt-get install gpg --yes \
    && rm --recursive --force /var/lib/apt/lists/* \
    && curl --silent --show-error --location --retry 5 https://dot.net/v1/dotnet-install.asc --output dotnet-install.asc \
    && gpg --import dotnet-install.asc \
    && rm dotnet-install.asc

RUN curl --silent --show-error --location --retry 5 https://dot.net/v1/dotnet-install.sh --output dotnet-install.sh \
    && curl --silent --show-error --location --retry 5 https://dot.net/v1/dotnet-install.sig --output dotnet-install.sig \
    && gpg --verify dotnet-install.sig dotnet-install.sh \
    && chmod +x ./dotnet-install.sh \
    && ./dotnet-install.sh --jsonfile ./global.json --install-dir /usr/share/dotnet \
    && rm dotnet-install.sh \
    && rm dotnet-install.sig

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish ./src/EurovisionHue --arch "${TARGETARCH}" --output /app --self-contained

FROM mcr.microsoft.com/dotnet/runtime-deps:9.0.10-noble@sha256:72f1a83c7de58476caa427af1cebb20418a4cad112fe761ba0802a5a95df5860 AS final

WORKDIR /app
COPY --from=build /app .

RUN ./EurovisionHue --install-deps

ENTRYPOINT ["./EurovisionHue"]

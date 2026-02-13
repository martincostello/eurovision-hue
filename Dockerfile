FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0.103@sha256:0a506ab0c8aa077361af42f82569d364ab1b8741e967955d883e3f23683d473a AS build
ARG TARGETARCH

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

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0.3-noble@sha256:52c58de54993994d6b0f161c46cb37b69e4cb4d29db2d943d8bfd6e1d266043d AS final

WORKDIR /app
COPY --from=build /app .

RUN ./EurovisionHue --install-deps

ENTRYPOINT ["./EurovisionHue"]

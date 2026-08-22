FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:11.0.100-preview.7@sha256:fd96d62893be03527912ce16b32e4331a64e6cf93393129f783a59b11e651790 AS build
ARG TARGETARCH
ARG SOURCE_DATE_EPOCH

COPY . /source
WORKDIR /source

SHELL ["/bin/bash", "-o", "pipefail", "-c"]

RUN apt-get update \
    && apt-get install gpg --yes \
    && rm --recursive --force /var/lib/apt/lists/*

RUN curl --silent --show-error --location --retry 5 https://dot.net/v1/dotnet-install.asc --output dotnet-install.asc \
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
    SOURCE_DATE_EPOCH="${SOURCE_DATE_EPOCH:-$(date +%s)}" \
    dotnet publish ./src/EurovisionHue --arch "${TARGETARCH}" --output /app --self-contained

FROM mcr.microsoft.com/dotnet/runtime-deps:11.0.0-preview.7-resolute@sha256:68068d0e7d51812ffbaaa3c4c45d8a17965ba3898c2d3bdbf1fd49d16bc9745c AS final

WORKDIR /app
COPY --from=build /app .

RUN ./EurovisionHue --install-deps

ENTRYPOINT ["./EurovisionHue"]

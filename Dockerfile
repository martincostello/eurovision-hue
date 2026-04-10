FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0.201@sha256:127d7d4d601ae26b8e04c54efb37e9ce8766931bded0ee59fcd799afd21d6850 AS build
ARG TARGETARCH

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
    dotnet publish ./src/EurovisionHue --arch "${TARGETARCH}" --output /app --self-contained

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0.5-noble@sha256:90bb23be7c17d7fce2381508d14d5716b36948534c5841e4804944bc9d941de7 AS final

WORKDIR /app
COPY --from=build /app .

RUN ./EurovisionHue --install-deps

ENTRYPOINT ["./EurovisionHue"]

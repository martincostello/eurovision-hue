FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0.301@sha256:548d93f8a18a1acbe6cc127bc4f47281430d34a9e35c18afa80a8d6741c2adc3 AS build
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

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0.9-noble@sha256:c7ee658e8fb276eb545cc67e2c37085dd7b902c0055d4aa35af213d1c18c6e6b AS final

WORKDIR /app
COPY --from=build /app .

RUN ./EurovisionHue --install-deps

ENTRYPOINT ["./EurovisionHue"]

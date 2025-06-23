FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0.301@sha256:faa2daf2b72cbe787ee1882d9651fa4ef3e938ee56792b8324516f5a448f3abe AS build
ARG TARGETARCH

COPY . /source
WORKDIR /source

SHELL ["/bin/bash", "-o", "pipefail", "-c"]

RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --jsonfile ./global.json --install-dir /usr/share/dotnet

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish ./src/EurovisionHue --arch "${TARGETARCH}" --output /app --self-contained

WORKDIR /app
COPY . ./
RUN dotnet publish ./src/EurovisionHue

FROM mcr.microsoft.com/dotnet/runtime:9.0.6-noble@sha256:bbf6472d8b38879c7c179783c8e5645c14c1ff8eee0d3045dc52dc3c53ec6b92 AS final
WORKDIR /app
COPY --from=build /app .

RUN dotnet EurovisionHue.dll --install-deps

ENTRYPOINT ["dotnet", "EurovisionHue.dll"]

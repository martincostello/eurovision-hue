FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0.301@sha256:faa2daf2b72cbe787ee1882d9651fa4ef3e938ee56792b8324516f5a448f3abe AS build
ARG TARGETARCH

COPY . /source
WORKDIR /source

SHELL ["/bin/bash", "-o", "pipefail", "-c"]

RUN curl -sSL --retry 5 https://dot.net/v1/dotnet-install.sh --output dotnet-install.sh \
    && echo "SHA512: $(sha512sum dotnet-install.sh)" \
    && echo "f8c59166ed912d6861e93c3efc2840be31ec32897679678a72f781423ebf061348d3b92b16c9541f5b312a34160f452826bb3021efb1414d76bd7e237e4c0e9a  dotnet-install.sh" | sha512sum -c \
    && chmod +x ./dotnet-install.sh \
    && ./dotnet-install.sh --jsonfile ./global.json --install-dir /usr/share/dotnet \
    && rm dotnet-install.sh

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish ./src/EurovisionHue --arch "${TARGETARCH}" --output /app

FROM mcr.microsoft.com/dotnet/runtime:9.0.6-noble@sha256:bbf6472d8b38879c7c179783c8e5645c14c1ff8eee0d3045dc52dc3c53ec6b92 AS final

WORKDIR /app
COPY --from=build /app .

RUN dotnet EurovisionHue.dll --install-deps

ENTRYPOINT ["dotnet", "EurovisionHue.dll"]

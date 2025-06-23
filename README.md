# Eurovision Hue 💡

[![Build status][build-badge]][build-status]
[![codecov][coverage-badge]][coverage-report]
[![OpenSSF Scorecard][scorecard-badge]][scorecard-report]

## Introduction

Updates your Hue lights to follow along with the Eurovision Song Contest.

This project was inspired by the [cnorthwood/eurovisionhue][inspiration] project.

## Building and Testing

Compiling the application yourself requires Git and the [.NET SDK][dotnet-sdk] to be installed.

To build and test the application locally from a terminal/command-line, run the
following set of commands:

```powershell
git clone https://github.com/martincostello/eurovision-hue.git
cd eurovision-hue
./build.ps1
```

## Running with Docker

To run the application using Docker, you will need to have [Docker][docker] installed.

First, build the Docker image for the application by running the following command in a terminal:

```terminal
docker build --tag eurovision-hue .
```

Finally, run the application using the following command:

```terminal
docker run --interactive --tty --rm --mount type=volume,src=eurovision-hue,dst=/app/MartinCostello/EurovisionHue eurovision-hue
```

The `eurovision-hue` volume is used to persist the configuration of the application.

## Feedback

Any feedback or issues can be added to the issues for this project in [GitHub][issues].

## Repository

The repository is hosted in [GitHub][repo]: <https://github.com/martincostello/eurovision-hue.git>

## License

This project is licensed under the [Apache 2.0][license] license.

[build-badge]: https://github.com/martincostello/eurovision-hue/actions/workflows/build.yml/badge.svg?branch=main&event=push
[build-status]: https://github.com/martincostello/eurovision-hue/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush "Continuous Integration for this project"
[coverage-badge]: https://codecov.io/gh/martincostello/eurovision-hue/branch/main/graph/badge.svg
[coverage-report]: https://codecov.io/gh/martincostello/eurovision-hue "Code coverage report for this project"
[docker]: https://www.docker.com/get-started/ "Get started with Docker"
[dotnet-sdk]: https://dotnet.microsoft.com/download "Download the .NET SDK"
[inspiration]: https://github.com/cnorthwood/eurovisionhue "Eurovision Hue by cnorthwood on GitHub"
[issues]: https://github.com/martincostello/eurovision-hue/issues "Issues for this project on GitHub.com"
[license]: https://www.apache.org/licenses/LICENSE-2.0.txt "The Apache 2.0 license"
[repo]: https://github.com/martincostello/eurovision-hue "This project on GitHub.com"
[scorecard-badge]: https://api.securityscorecards.dev/projects/github.com/martincostello/eurovision-hue/badge
[scorecard-report]: https://securityscorecards.dev/viewer/?uri=github.com/martincostello/eurovision-hue "OpenSSF Scorecard for this project"

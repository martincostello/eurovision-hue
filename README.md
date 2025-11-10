# Eurovision Hue 💡🇪🇺🎶

[![Build status][build-badge]][build-status]
[![codecov][coverage-badge]][coverage-report]
[![OpenSSF Scorecard][scorecard-badge]][scorecard-report]

## Introduction

Updates your Hue lights to follow along with the Eurovision Song Contest.

This project was inspired by the [cnorthwood/eurovisionhue][inspiration] project.

## How it Works

The application monitors a webpage for a live feed of the [Eurovision Song Contest][eurovision]
and updates your [Philips Hue][philips-hue] lights to match the colours of the flag of the current
participant in the contest that is on stage.

The current participant is determined by finding the most recent occurrence of the name
of a country in the live feed being monitored, so it may not always be accurate.

The Hue bridge for your lights must be discoverable on the same local network running
the application. The first time the application runs, it will prompt you to press the
button on your Hue bridge to allow it to connect to the bridge. You will then be able to
select which lights you want to synchronise with the live feed.

### Changing the Feed

If the built-in feed does not work for you, you can change the feed URL by changing the
following settings in [`appsettings.json`][appsettings]:

```json
{
  "FeedUrl": "https://www.bbc.co.uk/news/live/c74n9n5l1nxt",
  "ArticleSelector": "article[data-testid='content-post']"
}
```

- `FeedUrl` is the URL of the feed to monitor, for example the BBC News live blog.
- `ArticleSelector` is a [CSS selector][css-selector] that matches the HTML element(s) in
  the feed that contain the name of the country currently on stage. If multiple elements
  match the selector, the first one will be used.

These values can also be set using environment variables of the same names, for example:

```sh
export FeedUrl="https://www.bbc.co.uk/news/live/c74n9n5l1nxt"
export ArticleSelector="article[data-testid='content-post']"
```

> [!NOTE]
> The feed can also be reconfigured by the owner of this project via an edit to
> [this GitHub Gist](https://gist.github.com/martincostello/5ac0a49cd687c073ec09f4172c37185f)
> during the Eurovision Song Contest if you have not manually changed the settings.
>
> This is useful if the feed URL changes during an event without users needing to reconfigure
> or rebuild their local copy of the application after it has been started. If the feed configuration
> is changed remotely, the application should automatically observe the change within 5 minutes.

## Running with Docker

To run the application using Docker, you will need to have [Docker][docker] installed.

First, pull the latest image for the application to your local machine:

```terminal
docker pull ghcr.io/martincostello/eurovision-hue:edge
```

Finally, run the application using the following command:

```terminal
docker run --interactive --tty --rm --mount type=volume,src=eurovision-hue,dst=/app/MartinCostello/EurovisionHue ghcr.io/martincostello/eurovision-hue:main
```

The `eurovision-hue` volume is used to persist the configuration of the application between runs.

### Demo

> [!NOTE]
> To run the demo, you will need to either clone this repository locally, or download the
> [demo HTML file](https://raw.githubusercontent.com/martincostello/eurovision-hue/main/demo.html).

To run a demo of the application using Docker, you can use the following command:

```terminal
docker run --env ArticleSelector="td:nth-child(2)" --env FeedUrl="file:///app/demo.html" --interactive --tty --rm --mount type=volume,src=eurovision-hue,dst=/app/MartinCostello/EurovisionHue --mount type=bind,source=./demo.html,target=/app/demo.html ghcr.io/martincostello/eurovision-hue:main
```

This command will run the application in a mode where your selected Hue lights will randomly change
to the colours of the flag of a Eurovision participant approximately every 10 seconds.

## Building and Testing

Compiling the application yourself without Docker requires Git and the [.NET SDK][dotnet-sdk] to be installed.

To build and test the application locally from a terminal/command-line, run the
following set of commands:

```powershell
git clone https://github.com/martincostello/eurovision-hue.git
cd eurovision-hue
./build.ps1
```

## Verifying Container Image Signatures

The container images published for this project are signed using [cosign][cosign]. You
can verify the signatures using the following command:

```sh
IMAGE="ghcr.io/martincostello/eurovision-hue:edge"
IDENTITY="https://github.com/martincostello/eurovision-hue/.github/workflows/build.yml@refs/heads/main"
OIDC_ISSUER="https://token.actions.githubusercontent.com"

cosign verify $IMAGE --certificate-identity $IDENTITY --certificate-oidc-issuer $OIDC_ISSUER
```

## Feedback

Any feedback or issues can be added to the issues for this project in [GitHub][issues].

## Repository

The repository is hosted in [GitHub][repo]: <https://github.com/martincostello/eurovision-hue.git>

## License

This project is licensed under the [Apache 2.0][license] license.

[appsettings]: https://github.com/martincostello/eurovision-hue/blob/f64127b29cb633f1cab383f9f620724efdc97ccc/src/EurovisionHue/appsettings.json
[build-badge]: https://github.com/martincostello/eurovision-hue/actions/workflows/build.yml/badge.svg?branch=main&event=push
[build-status]: https://github.com/martincostello/eurovision-hue/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush "Continuous Integration for this project"
[cosign]: https://github.com/sigstore/cosign "Cosign on GitHub"
[coverage-badge]: https://codecov.io/gh/martincostello/eurovision-hue/branch/main/graph/badge.svg
[coverage-report]: https://codecov.io/gh/martincostello/eurovision-hue "Code coverage report for this project"
[css-selector]: https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_selectors "CSS Selectors on MDN"
[docker]: https://www.docker.com/get-started/ "Get started with Docker"
[dotnet-sdk]: https://dotnet.microsoft.com/download "Download the .NET SDK"
[eurovision]: https://eurovision.tv/ "Eurovision Song Contest"
[inspiration]: https://github.com/cnorthwood/eurovisionhue "Eurovision Hue by cnorthwood on GitHub"
[issues]: https://github.com/martincostello/eurovision-hue/issues "Issues for this project on GitHub.com"
[license]: https://www.apache.org/licenses/LICENSE-2.0.txt "The Apache 2.0 license"
[philips-hue]: https://www.philips-hue.com/ "Philips Hue"
[repo]: https://github.com/martincostello/eurovision-hue "This project on GitHub.com"
[scorecard-badge]: https://api.securityscorecards.dev/projects/github.com/martincostello/eurovision-hue/badge
[scorecard-report]: https://securityscorecards.dev/viewer/?uri=github.com/martincostello/eurovision-hue "OpenSSF Scorecard for this project"

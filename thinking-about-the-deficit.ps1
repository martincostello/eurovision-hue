
#docker build --tag eurovision-hue . && `
docker run --env ArticleSelector="td:nth-child(2)" `
           --env FeedUrl="file:///app/demo.html" `
           --interactive `
           --tty `
           --rm `
           --mount type=volume,src=eurovision-hue,dst=/app/MartinCostello/EurovisionHue `
           --mount type=bind,source=./demo.html,target=/app/demo.html `
           eurovision-hue

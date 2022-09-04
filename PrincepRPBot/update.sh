docker stop discord-bot

docker image rm princep/discord-bot

git pull

docker build -t princep/discord-bot .

docker run -d --name discord-bot princep/discord-bot
if [ $(docker ps -a --format {{.Names}} | grep user-api) ]
then
    docker rm -f user-api
    docker rmi user-api
fi
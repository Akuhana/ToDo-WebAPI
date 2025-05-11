docker stop webapi_prod
docker rm   webapi_prod
docker build -t webapitest:prod .
docker run -d \
  --name webapi_prod \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -p 32774:8080 \
  -p 32775:8081 \
  webapitest:prod
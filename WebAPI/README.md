
# Building the WebAPI
docker build -t todo-webapi:latest .

# Running the WebAPI
docker run -d --name todo-webapi -p 5333:8080 -p 5334:8081 todo-webapi:latest
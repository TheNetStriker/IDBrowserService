docker build -f DockerfileTest --tag idbrowserservicecore-test .
docker run --rm --name idbrowserservicecore-test -v Z:\:/storage01/Multimedia idbrowserservicecore-test
docker image rm idbrowserservicecore-test
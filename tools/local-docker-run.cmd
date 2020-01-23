@@echo off
set image="trail365/app-dev:latest"
cd %~dp0
echo Start with docker image %image% ...
docker run --rm -p 8888:80 %image% 
pause

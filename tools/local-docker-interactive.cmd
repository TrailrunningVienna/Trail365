@@echo off
set image="trail365/app-dev:latest"
cd %~dp0
echo Interactive start with docker 
echo   image=%image%
echo   interpreter=bash
echo.
docker run -it --rm --entrypoint bash %image% 
pause


#!/bin/sh
#REQUIREMENTS: pwd is located on repo root!
set -e
echo "Trail365 Build Script v1.0.0"
#$1....Docker Image:Tag
#$2....Docker Context
#echo "BUILD_NUMBER=${BUILD_NUMBER}"
echo ""
tag=$1

#Context (=current folder) must be the parent folder of src)
if ! [ -z "$2" ]
then
  cd $2
fi

mainFolderPath=$(pwd)
echo "Start docker build (BUILD_NUMBER=${BUILD_NUMBER}) for docker tag '${tag}' using Context '${mainFolderPath}'"
echo ""
outFolderPath=${mainFolderPath}/build-out
docker build --build-arg BUILD_NUMBER=${BUILD_NUMBER} -f ./src/Trail365.Web/Dockerfile -t ${tag} .

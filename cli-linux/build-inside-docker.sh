#!/bin/sh
set -e
echo ""
#echo "Docker Build CLI for Trail365"

mainFolderPath=$(pwd)
outFolderPath=/app
solutionFile=${mainFolderPath}/Trail365.sln

verbosity=minimal
configuration=Release

if ! [ -z "$1" ]
then
  verbosity=$1
fi

if ! [ -z "$2" ]
then
  configuration=$2
fi

echo ""

dotnet restore ${solutionFile} -v ${verbosity}
dotnet build ${solutionFile} -c ${configuration} -o ${outFolderPath} -v ${verbosity} --no-restore
dotnet publish -c ${configuration} -o ${outFolderPath} -v ${verbosity} --no-restore
echo ""
FULL_VERSION=$(dotnet ${outFolderPath}/Trail365.Web.dll --version)
MINOR_VERSION=$(dotnet ${outFolderPath}/Trail365.Web.dll --minor)
echo "::set-env name=FULL_VERSION::${FULL_VERSION}"
echo "::set-env name=MINOR_VERSION::${MINOR_VERSION}"
echo ""

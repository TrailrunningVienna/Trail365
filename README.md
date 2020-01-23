# Trail 365 

## Issues
Issues currently are managed in this [Github repository](https://github.com/TrailrunningVienna/Trail365-Home)

## Books, Ideas, Sources

### General
- [Solution Architecture](https://github.com/ardalis/CleanArchitecture)
- [eShopWeb](https://github.com/dotnet-architecture/eShopOnWeb)

### Icons
- [Material Design](https://github.com/Templarian/MaterialDesign)
- [icon8](https://icons8.com/icons/set)

## Progress
Date|Status
---|---
21.08.2019|start deployment (GitFlow Rules) from branch 'master' to app-service 'trail365' on D1WindowsProduction 
22.08.2019|start deployment from branch 'develop' to app-service 'trail365-qa' on D1WindowsStaging
04.09.2019|v0.0.210 released to master/prod
05.09.2019|System.Drawing.Common (windows) replaced with another X-Plat Drawing Engine, refactoring to MSS.Core.Graphics
04.11.2019|v0.2.0 event sync with FB
08.11.2019|v0.3.0 backend and API improvements for Tracks (track database online) 
05.01.2020|v0.9.0 migrate to asp net core 3.1

## TODO 

## Special Routes/Urls

Route|Result
---|---
auth/diag|authentication diagnostics
auth/signout|Logout
backend|Backend UI
health|health UI

## Known Issues

## Planning/Issues

### Build Implementation 
Core code for build is implemented in sh (linux shell) and located inside "cli-linux" on repository root.
Our Dockerfile is located inside the Web Project

### Executing local docker build on a windows developer machine

#### Requirements
- DotnetCore 2.2 SDK must be installed (Part of VS) and available via PATH (dotnet --info)
- docker for Windows (in Linux Mode) must be installed and docker commandline available via PATH (docker --version)
- sh.exe (Linux Shell file Interpreter) must be installed (part of Git for Windows, Sourcetree etc) and available via PATH (sh --version)

Tip: if sh.exe is not available via PATH please check with "dir c:\sh.exe" if it is just installed but not added to PATH!

#### build docker container
- execute "docker-build.cmd" located on repository root!
    - a docker image should be buildet on the local machine (some warning regarding windows can be ignored)
- execute "docker-run.cmd" located on repository root
   - this starts a container using our image. The container starts with defaults, that means
       - authentication is off
       - migrations are not started / database is not available  

### Version/Build numbers
Basically we use [Semver 2.0](https://semver.org/spec/v2.0.0.html)
There is ONE place (=file) where the version number can be set for all assemblies => "version.props" on repo root.
There are 3 Version parts that can be defined by the development team (following SemVer 2.0).
- MajorVersion
- MinorVersion
- PatchVersion
- PreReleaseLabel

We use and support pre-release versions.
For pre-releases set the property "PreReleaseLabel" to something different then empty (suggested: "preview" or "prev").
On pre-releases we automatically add a build-number.


# we use ARGS here because they can be overriden for 'docker build' 

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as dotnet-build
WORKDIR /src

FROM dotnet-build as publish
WORKDIR /src
COPY . .

ARG BUILD_VERBOSITY=quiet
ARG BUILD_CONFIGURATION=Release
ARG BUILD_NUMBER=0001

RUN chmod +x ./cli-linux/build-inside-docker.sh
RUN ./cli-linux/build-inside-docker.sh $BUILD_VERBOSITY $BUILD_CONFIGURATION

FROM base AS final

ENV CUSTOMCONNSTR_TrailDB="Data Source=/home/trail365_dockerdefault.sqlite"
ENV CUSTOMCONNSTR_IdentityDB="Data Source=/home/identity365_dockerdefault.sqlite"
ENV CUSTOMCONNSTR_TaskDB="Data Source=/home/task365_dockerdefault.sqlite"
ENV PuppeteerEnabled=false
ENV RunMigrationsAtStartup=true
ENV SeedOnCreation=false

WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Trail365.Web.dll"]

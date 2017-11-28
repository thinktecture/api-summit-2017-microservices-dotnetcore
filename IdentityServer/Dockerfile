FROM microsoft/dotnet:1.1.1-sdk

RUN apt-get update
RUN apt-get -qq update

ADD ./ /app/
WORKDIR /app/
RUN dotnet restore

EXPOSE 5000
ENTRYPOINT ["dotnet", "run"]
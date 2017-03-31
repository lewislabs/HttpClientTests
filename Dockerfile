FROM microsoft/dotnet:1-sdk
RUN mkdir /app
COPY . /app
WORKDIR /app
RUN dotnet restore
RUN dotnet build -c Release
ENTRYPOINT ["dotnet", "run", "-c", "Release"]



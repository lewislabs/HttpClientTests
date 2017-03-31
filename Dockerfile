FROM microsoft/dotnet:1-sdk
RUN mkdir /app
COPY . /app
WORKDIR /app
RUN dotnet restore -r netcoreapp1.1
RUN dotnet build -c Release -r netcoreapp1.1
ENTRYPOINT ["dotnet", "run", "-c", "Release", "-r", "netcoreapp1.1"]



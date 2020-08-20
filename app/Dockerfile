#FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
#WORKDIR /app

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
#COPY --from=build-env /app/out .
ADD https://raw.githubusercontent.com/zuk38/webrtc-server/master/app.tar /tmp/app.tar
RUN ls -Rlh /tmp/ && tar xvf /tmp/app.tar -C /app/
ENTRYPOINT ["dotnet", "ST.WebApi.dll"]
EXPOSE 80
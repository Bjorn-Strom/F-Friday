# Build
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine as build

WORKDIR /app

RUN apk add --update nodejs npm netcat-openbsd

COPY .config/. .config/.

COPY Server/. Server/.
COPY Shared/. Shared/.
COPY Client/. Client/.

RUN dotnet tool restore
WORKDIR /app/Client
RUN npm ci
RUN npm run build
WORKDIR /app/Server
RUN dotnet publish -c release -o out

# Run
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
WORKDIR /app/Server
COPY --from=build /app/Server/out .
RUN mkdir wwwroot
COPY --from=build /app/Client/dist wwwroot/.
ENV PORT=80
ENV ASPNETCORE_URLS=http://+:80
ENV DATABASE_URL=$DATABASE_URL
EXPOSE 80
CMD dotnet Server.dll
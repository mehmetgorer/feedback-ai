FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CustomerFeedback.slnx .
COPY CustomerFeedback.Api/CustomerFeedback.Api.csproj CustomerFeedback.Api/
RUN dotnet restore CustomerFeedback.Api/CustomerFeedback.Api.csproj

COPY CustomerFeedback.Api/ CustomerFeedback.Api/
RUN dotnet publish CustomerFeedback.Api/CustomerFeedback.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

RUN mkdir -p /data

COPY --from=build /app/publish .
COPY docker-entrypoint.sh /app/docker-entrypoint.sh
RUN sed -i 's/\r$//' /app/docker-entrypoint.sh \
    && chmod +x /app/docker-entrypoint.sh

ENTRYPOINT ["/app/docker-entrypoint.sh"]
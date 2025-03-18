FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
# config listen port
ENV ASPNETCORE_URLS=http://+:8083 

ARG ConnectionStrings__dbConnectionTransactions
ARG APISettings__JwtOptions__Secret
ARG APISettings__JwtOptions__Issuer
ARG APISettings__JwtOptions__Audience
ARG RabbitMQ__Host
ARG RabbitMQ__Port
ARG RabbitMQ__Username
ARG RabbitMQ__Password
ARG Stripe__Secret
ARG Redis__ConnectionString

ENV ConnectionStrings__dbConnectionTransactions=${ConnectionStrings__dbConnectionTransactions}
ENV APISettings__JwtOptions__Secret=${APISettings__JwtOptions__Secret}
ENV APISettings__JwtOptions__Issuer=${APISettings__JwtOptions__Issuer}
ENV APISettings__JwtOptions__Audience=${APISettings__JwtOptions__Audience}
ENV RabbitMQ__Host=${RabbitMQ__Host}
ENV RabbitMQ__Port=${RabbitMQ__Port}
ENV RabbitMQ__Username=${RabbitMQ__Username}
ENV RabbitMQ__Password=${RabbitMQ__Password}
ENV Stripe__Secret=${Stripe__Secret}
ENV Redis__ConnectionString=${Redis__ConnectionString}


RUN echo "Variables cargadas: $RabbitMQ__Host"

EXPOSE 8083
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "PaymentsMS.dll"]
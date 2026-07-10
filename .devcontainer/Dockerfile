FROM mcr.microsoft.com/devcontainers/dotnet:2-10.0-noble

ENV DEBIAN_FRONTEND=noninteractive
ENV PATH="$PATH:/opt/mssql-tools18/bin"

RUN apt-get update && apt-get install -y \
    curl \
    gnupg2 \
    apt-transport-https \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

RUN curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg \
    && curl -fsSL https://packages.microsoft.com/config/ubuntu/24.04/mssql-server-2025.list | tee /etc/apt/sources.list.d/mssql-server-2025.list \
    && curl -sSL -O https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb
    
RUN apt-get update && apt-get install -y mssql-server \
    && ACCEPT_EULA=Y apt-get -y install mssql-tools18 unixodbc-dev mssql-tools18 \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 1433

# SpendSmart - ASP.NET MVC Expense Tracker

[![Jenkins Pipeline](https://img.shields.io/badge/Jenkins-Pipeline-blue)](https://jenkins.io)
[![Docker Container](https://img.shields.io/badge/Docker-Container-informational)](https://www.docker.com)
[![ASP.NET Core 8](https://img.shields.io/badge/ASP.NET%20Core-8.0-purple)](https://dotnet.microsoft.com)
[![Webhook Enabled](https://img.shields.io/badge/Webhook-Enabled-brightgreen)](https://docs.github.com/en/webhooks)
![Tests](https://img.shields.io/badge/Unit%20Tests-Passing-brightgreen)

SpendSmart is a containerized ASP.NET MVC 8 application with cookie-based authentication, SQL Server backend, and EF Core migrations. The application exposes a secured Web API for external consumption and integrates live currency conversion using a third-party API. Managed via Docker and Jenkins CI/CD.

Live on Azure: [Try the app here](spendsmart-app-daagcrd6g0effgf4.uksouth-01.azurewebsites.net)
![SpendSmart Login](SpendSmart/Assets/Login.png)
![SpendSmart User_Landing](SpendSmart/Assets/User_Landing.png)
![SpendSmart User_DashBoard](SpendSmart/Assets/User_DashBoard.png)
##  Key Technologies
- **ASP.NET Core MVC 8** with Cookie Authentication
- **Entity Framework Core** (Code-first migrations)
- **SQL Server Database**
- **Docker** (Multi-container environment)
- **Jenkins** (CI/CD pipeline)
- **Swagger (REST API documentation and testing)** for external integration
- **Currency Conversion via open.er-api.com**

##  Authentication System
The application implements **cookie-based authentication** with:
- User registration/login functionality
- Session management via cookies
- Password hashing
- ASP.NET Identity framework
- Anti-forgery token validation
- Secure cookie settings (HTTPOnly, SameSite=Lax)

## External Currency Conversion (Live)

SpendSmart integrates live currency conversion using `https://open.er-api.com/v6/latest/USD`.  
This enables users to view their expenses in real-time in multiple currencies.

- **Base Currency**: USD (fixed)
- **Frontend Integration**: Drop-down to choose target currency
- **Conversion Logic**: Handled in service layer and controller
- **Security**: External API call cached per session

---

## Exposing Your Data via Web API

SpendSmart exposes secure API endpoints to allow external systems to access user expense data. Authentication is enforced via API key.

## API Endpoint: Get User Expenses
SpendSmart exposes a secure REST API to fetch user expenses:

GET /api/ExternalApi/expenses/{userId}
- Headers: X-API-KEY: MySuperSecretKey123
- Response: JSON list of user's expense
- Authentication: Requires a valid API key

## Swagger Integration

SpendSmart includes Swagger UI for testing and exploring REST APIs.

### How to Enable Swagger in Program.cs
```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

app.UseSwagger();
app.UseSwaggerUI();
```
### Access Swagger UI

https://localhost:9003/Swagger


##  Cookie Security Implementation
SpendSmart uses ASP.NET Identity's cookie authentication with these security settings:

### Security Features
| Setting | Implementation | Notes |
|---------|----------------|-------|
| **HttpOnly** | Enabled (default) | Prevents client-side access to cookies |
| **SameSite** | Lax (default) | Balanced CSRF protection |
| **SecurePolicy** | Environment-aware | `None` in development, `Always` in production |
| **Expiration** | 1 minute sliding | Aggressive security posture |
| **Token Validation** | Built-in | Automatic token validation |

### Code Implementation
Security settings come from ASP.NET Identity defaults configured in `Program.cs`:
```csharp
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    // Password policy configured here
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<SpendSmartDBContext>();

builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(1);
    options.SlidingExpiration = true;
});
```
### Security Verification
| Environment | Cookie Security | Verification |
|-------------|-----------------|-------------|
| Development | HTTP allowed | No `Secure` flag |
| Production | HTTPS required | `Secure` flag present |
| Docker | Config-driven | Set via `ASPNETCORE_ENVIRONMENT` |

**Browser Check:** After login, verify in DevTools > Cookies:
- HttpOnly flag
- SameSite=Lax
- Secure flag (in production)

##  Why Docker?
| Benefit | Description |
|---------|-------------|
| **Consistent Environments** | Identical behavior across all systems |
| **Simplified Setup** | No manual dependency installation |
| **Isolated Services** | Web app + DB in separate containers |
| **Portability** | Runs anywhere Docker is supported |
| **DevOps Friendly** | Seamless CI/CD pipeline integration |
| **Secure Execution** | Isolated from host OS |

##  System Requirements
1. Docker Desktop ([Windows/Mac](https://www.docker.com/products/docker-desktop) | [Linux](https://docs.docker.com/engine/install/))
2. Git
3. Jenkins (for pipeline execution)

##  One-Command Setup

## 1. Clone and Launch
```bash
git clone https://github.com/nsg26/SpendSmart.git
cd SpendSmart
```
## Build and start containers:
```
docker-compose up --build -d
```
## Access application:
1. Open https://localhost:9003

2. Register new account or use default(Admin) credentials:

    - Email: admin@spendsmart.com

    - Password: Admin@123

3. Start managing expenses!

## To stop:
```
docker-compose down
```
## To stop and remove the database volume (i.e., wipe data):
```
docker-compose down --volumes
```
---

##  Run via Jenkins Pipeline

This repo contains two Jenkinsfiles for CI/CD automation:

**1. jenkinsfile**
- Clones the repo

- Builds Docker images

- Starts the app using Docker Compose

How to use:

- Open Jenkins and create a new Pipeline job.

- Choose Pipeline script from SCM.

- Set repository URL to this repo.

- Set Script Path to jenkinsfile.

- Run the job to build and start the application.

![SpendSmart Start_Pipeline](SpendSmart/Assets/Start_Pipeline.png)
![SpendSmart Start_Pipeline_Output](SpendSmart/Assets/Start_Pipeline_Output.png)

**2. jenkinsfile-stop**
- Stops and removes the containers and volumes safely.

How to use:

- Create a new Pipeline job in Jenkins.

- Choose Pipeline script from SCM.

- Set repository URL to the same repo.

- Set Script Path to jenkinsfile-stop.

- Run the job to stop the application and clean up Docker resources.

![SpendSmart Stop_Pipeline](SpendSmart/Assets/Stop_Pipeline.png)

---
##  GitHub Webhook Integration (Auto-Trigger CI/CD)

SpendSmart integrates with **GitHub webhooks** to automatically trigger the Jenkins CI/CD pipeline whenever code is pushed to the repository.

### How It Works

- A **GitHub webhook** is configured to notify Jenkins of any new push events.
- Jenkins runs locally in a Docker container and is exposed to the internet using **ngrok**, which creates a secure HTTPS tunnel.
- GitHub sends a webhook payload to Jenkins, which then triggers the CI/CD pipeline automatically.

### Exposing Jenkins with ngrok

To allow GitHub to reach your local Jenkins instance:

```bash
ngrok http 9090
```
### This generates a public HTTPS URL like:

```bash
https://abc123.ngrok-free.app/github-webhook/
```
### Use this URL as the Payload URL when configuring the webhook in GitHub.

**GitHub Webhook Setup Steps**
1. Run ngrok to expose Jenkins:

```bash
ngrok http 9090
```
2. Copy the generated public URL.
3. In your GitHub repository, go to:
   
   Settings > Webhooks > Add webhook

4. Configure the webhook as follows:
   
    - Payload URL: https://your-ngrok-subdomain/github-webhook/
    - Content type: application/json
    - Secret: (optional but recommended for verification)
    - Event type: Just the push event
  
6. In Jenkins, open your pipeline job and enable:
   
   Build Triggers > GitHub hook trigger for GITScm polling
   
### Result
Every push to GitHub triggers a Jenkins build automatically

---

### Unit Testing (xUnit)

SpendSmart uses **xUnit** for unit testing, integrated into Jenkins CI pipeline.

Test coverage includes:
- CurrencyService – success + error handling
- Automated via `dotnet test` in Jenkins

---

##  Accessing the SQL Server

| Setting  | Value            |
| -------- | ---------------- |
| Server   | `localhost,1433` |
| Username | `sa`             |
| Password | `Docker@1234`    |

![SpendSmart SQL_Server_Database](SpendSmart/Assets/SQL_Server_Database.png)
---

##  Project Structure
<pre>
SpendSmart/
├── SpendSmart/              # ASP.NET MVC Source
│   └── SpendSmart.csproj
├── docker-compose.yml       # Docker Compose file
├── jenkinsfile              # Jenkinsfile to build and run
├── jenkinsfile-stop         # Jenkinsfile to stop and clean
└── README.md                # This file
</pre>
---

##  Notes

- Application uses cookie-based authentication

- Database migrations run automatically on startup

- Port Configuration

    | Service        | Host Port | Container Port | Protocol | Description                                 |
    |----------------|-----------|----------------|----------|---------------------------------------------|
    | Jenkins        | `8080`    | `8080`         | HTTP     | Jenkins CI/CD interface (Docker container)  |
    | SpendSmart App | `9003`    | `443`          | HTTPS    | Access the app securely over HTTPS (Docker) |
    | SQL Server     | `1433`    | `1433`         | TCP      | SQL Server database port                    |


- SQL Server password follows complexity rules

- Use docker volume ls and docker volume rm for manual volume management

##  License
MIT © 2025 SpendSmart Contributors



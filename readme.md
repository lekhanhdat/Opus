# Opus

**Pipeline Status:**

[![Latest Opus project pipeline status](https://git.avepoint.net/cloud/records/reco/badges/master/pipeline.svg)](https://git.avepoint.net/cloud/records/reco/-/commits/master)

---

## 📖 Table of Contents

- [Project Background](#project-background)
- [Technology Stack](#technology-stack)
- [Repository Layout](#repository-layout)
- [Getting Started](#getting-started)
- [Environment Setup Guides](#environment-setup-guides)
- [FAQ](#faq)

---

## Project Background

### Overview
AvePoint Opus is an online information lifecycle solution that supports organizations in managing their content stored in various repositories such as Microsoft 365, Azure File System, on-premises content - Windows File System content and SharePoint On-Premises content (SharePoint 2013, 2016, 2019, and Subscription Edition), and physical records. Now with added support for Box and Google Drive (Preview) as content sources, AvePoint Opus extends its capabilities even further. Additionally, the connector framework allows integration with other business systems, ensuring a seamless and efficient content management experience. Organizations can achieve cost savings with storage optimization or can run full lifecycle programs across content, wherever that content lives.

### Supported Content Sources
Current (some may be in Preview) connectors / repositories surfaced through the platform:
- Microsoft 365 (SharePoint Online, Exchange Online, Teams, OneDrive, related workloads)
- Azure File System
- Windows File System (on-premises via Hybrid / Agent)
- SharePoint On-Premises (2013 / 2016 / 2019 / Subscription Edition)
- Physical Records (inventory / metadata representation of physical assets)
- Box
- Google Drive (Preview)

### Integrations & Extensions
| Integration | Purpose |
|-------------|---------|
| AVA (Virtual Assistant) | Conversational assistance & automation entry points |
| MyHub | Workspace lifecycle & provisioning synergy |
| Connector Framework | Extensible model to onboard additional business systems / repositories |
| AI (Maestro / Internal Models) | Enrichment, classification recommendations, extraction workflows |
| Recovery / ReCenter Portal | End-user recovery & self-service access pathways |
| Security / Encryption | Data encryption methods (refer to internal encryption guide & Infra Cipher doc) |

---

## Technology Stack

| Area | Technology / Notes |
|------|--------------------|
| Language | .NET 8 (C#), some legacy .NET Standard 2.0 components |
| Web/API | ASP.NET Core, RESTful APIs |
| Frontend (if applicable) | `RAWeb.UI` (internal UI framework / Razor Pages) |
| Database | Azure SQL / Azure Table / CosmosDB |
| Data storage | Azure Storage / Google Cloud Storage / Amazon S3(-Compatible Storage) / FTP / SFTP / Dropbox / storage abstraction (`Storage.ModernApi`) |
| Caching | Redis |
| Security | Infra Cipher, custom encryption components, audit logging |
| Main solutions | `RAOnline.DEV.sln`, `RAAgent.sln`, etc. |
| CI/CD | GitLab |
| Quality | ESLint / SonarQube / static analysis (Fortify) / dependency governance |
| Main modules | Discovery and Analysis / Information Lifecycle / Storage Optimization |
| Search / Restore center (full text search) | Global search (`RAGlobalSearch`) |
| AI/ML | Maestro AI / Integration with AVA |

---

## Repository Layout

Selected key directories (not exhaustive):

```
RAWeb*/                   Opus UI
RATimerWorkerRole/        Timer Service
RAApi.Web/                Core internal API (REST)
RAApi.Web.Public/         Public-facing API endpoints
AIExtractWorker/          AI document extraction & processing worker
RAApi.Services/           Domain service implementations
RAApi.Contract/           API contracts / DTOs
RACommon*/                Shared utilities & foundational libraries
RAReportCenter/           Reporting & audit center
RAScheduleJob/            Scheduled jobs & timer-based execution
RAArtificialIntelligence/ AI related logic
RAGlobalSearch/           Global search functionality
RAGoogle/, RAExchange/, RASharePoint*, RAAzureFile/ Multi-source connectors (Google, Exchange, SharePoint, Azure File, etc.)
RecordsAPIContract/       Records API contract package
Hybrid* / HybirdFramework Hybrid agent & connector support
Upgrade / Migration / Maintenance Upgrade, migration & maintenance artifacts
UnitTest / *.Tests/       Unit & integration test projects
```

---

## Getting Started

### Prerequisites

Required:
```
.NET SDK 8.x
Node.js >= 18.18
Git >= 2.41
Visual Studio 2022 (with .NET & Web workloads) or VS Code
SQL Server
Redis
Cosmos DB
``` 

### Clone & Restore
```powershell
git clone git@git.avepoint.net:cloud/records/reco.git opus
cd opus
dotnet restore RAOnline.DEV.sln
```

---

### Configuration

Modify the following configuration files:
- `build\AppConfig\appsettings.json`
- `HybirdFramework\HybridServer\appsettings.json`

Refer to [env-config-en](https://git.avepoint.net/devops/wiki/-/blob/main/Opus/project_config_guide/env-config-en.md?ref_type=heads)

Never commit secrets: use **User Secret / Environment Variables / Vault**. See [Development Guide For Infra Cipher](https://git.avepoint.net/cloud_architect/cloud-wiki/-/blob/main/Development-Guide-For-Infra-Cipher.md).

---

### Build
```powershell
dotnet build RAOnline.DEV.sln -c Debug

cd RAWeb.UI
npm install
npm run build
```
> Other solutions: `RAAgent.sln`, `Tool.Build.sln` as needed.


### Run
```powershell
dotnet run --no-build --project RAWeb\RAWeb.csproj
dotnet run --no-build --project RATimerWorkerRole\RATimerWorkerRole.csproj
dotnet run --no-build --project RAApi.Web/RAApi.Web.csproj
dotnet run --no-build --project RAApi.Web.Public\RAApi.Web.Public.csproj
dotnet run --no-build --project HybirdFramework\HybridServer\HybridServer.csproj
```
Or set as startup project in Visual Studio.

## Environment Setup Guides

> Centralized references to environment setup documents.

- Git Operation Flow (CN) — [doc](https://avepointcrm.sharepoint.com/:w:/r/sites/Records_RD/Shared%20Documents/General/Records/Dev/Git%E6%93%8D%E4%BD%9C%E6%B5%81%E7%A8%8B.docx?d=wd083d790ab8c41ce96533309af6fee82&csf=1&web=1&e=S8Tmcs)
- Dev Environment (CN) — [env-config-cn](https://git.avepoint.net/devops/wiki/-/blob/main/Opus/project_config_guide/env-config-cn.docx?ref_type=heads)
- Dev Environment (EN) — [env-config-en](https://git.avepoint.net/devops/wiki/-/blob/main/Opus/project_config_guide/env-config-en.md?ref_type=heads)
- DAO Environment (CN) — [dao-env-config-cn](https://git.avepoint.net/devops/wiki/-/blob/main/Opus/project_config_guide/dao-env-config-cn.docx?ref_type=heads)
- DAO Environment (EN) — [dao-env-config-en](https://git.avepoint.net/devops/wiki/-/blob/main/Opus/project_config_guide/dao-env-config-en.docx?ref_type=heads)
- Agent Environment (CN) — [agent-env-config-cn](https://git.avepoint.net/devops/wiki/-/blob/main/Opus/project_config_guide/agent-env-config-cn.docx?ref_type=heads)
- Agent Environment (EN) — [agent-env-config-en](https://git.avepoint.net/devops/wiki/-/blob/main/Opus/project_config_guide/agent-env-config-en.docx?ref_type=heads)
- .NET6 Upgrade Note — [NET6 Upgrade](https://git.avepoint.net/devops/wiki/-/blob/main/Opus/upgreade_note/net6upgrade-note.md?ref_type=heads)
- Feature Standard — [Feature standard](https://avepointcrm.sharepoint.com/:w:/r/sites/Records_RD/Shared%20Documents/General/Records/Release/Records%20Feature%20%E6%A0%87%E5%87%86.docx?d=wdb5d8fca0cdc4c178454de84f335b258&csf=1&web=1&e=CjALQF)

---

## FAQ

### 1. Can not browse tree on Content Source page
Check points:
- Whether the Cosmos DB service is running
- Whether the internal Opus API site is running

### 2. An 503 error occurred while accessing the Opus
Check points:
- Whether the appsettings.json is configured correctly.
- Whether the tenant status of account is 0, query the RMTenantInfoes table in the Control DB.
- Check the 'C:\logs\RecordsWeb.log' for more information.

---



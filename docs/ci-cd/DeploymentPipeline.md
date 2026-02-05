# PoTicTac - Deployment Pipeline Documentation

## Overview

PoTicTac uses GitHub Actions with Azure Developer CLI (azd) for automated CI/CD deployment to Azure Container Apps. The pipeline uses OIDC (OpenID Connect) for secure, credential-less authentication.

---

## Pipeline Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   GitHub Repo   │────▶│  GitHub Actions │────▶│  Azure Cloud    │
│   (main branch) │     │   (ubuntu-latest)│     │  (Container Apps)│
└─────────────────┘     └─────────────────┘     └─────────────────┘
                               │
                               ▼
                        ┌─────────────────┐
                        │  Azure Services │
                        │  • ACR (images) │
                        │  • CAE (hosting)│
                        │  • Table Storage│
                        └─────────────────┘
```

---

## Workflow File

**Location**: `.github/workflows/azure-deploy.yml`

### Triggers

```yaml
on:
  push:
    branches:
      - main           # Deploy on every push to main
  workflow_dispatch:   # Manual trigger from GitHub UI
```

### Required Permissions

```yaml
permissions:
  id-token: write     # Required for OIDC token exchange
  contents: read      # Read repository code
```

---

## Environment Variables & Secrets

### GitHub Variables (Repository Settings → Variables)

| Variable | Description | Example |
|----------|-------------|---------|
| `AZURE_CLIENT_ID` | Service Principal Client ID | `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` |
| `AZURE_TENANT_ID` | Azure AD Tenant ID | `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` |
| `AZURE_SUBSCRIPTION_ID` | Target subscription | `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` |
| `AZURE_ENV_NAME` | azd environment name | `potictac-prod` |
| `AZURE_LOCATION` | Azure region | `eastus` |

### Azure Resources Created

| Resource | Name | Resource Group |
|----------|------|----------------|
| Container Registry | `acrposhared` | PoShared |
| Container Apps Environment | `cae-poshared` | PoShared |
| Log Analytics Workspace | `law-poshared` | PoShared |
| Key Vault | `kv-poshared` | PoShared |
| Table Storage | `stpotictac` | PoTicTac |
| Managed Identity | `id-potictac` | PoTicTac |

---

## Pipeline Stages

### Stage 1: Setup

```yaml
- name: Checkout repository
  uses: actions/checkout@v4

- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '10.0.x'

- name: Install Azure Developer CLI
  uses: Azure/setup-azd@v2
```

### Stage 2: Azure Authentication (OIDC)

```yaml
- name: Azure Login (OIDC)
  uses: azure/login@v2
  with:
    client-id: ${{ env.AZURE_CLIENT_ID }}
    tenant-id: ${{ env.AZURE_TENANT_ID }}
    subscription-id: ${{ env.AZURE_SUBSCRIPTION_ID }}
```

**Key Points**:
- No secrets stored in GitHub
- Uses federated credentials
- Token auto-expires

### Stage 3: Build & Test

```yaml
- name: Build solution
  run: dotnet build PoTicTac.sln --configuration Release

- name: Run unit tests
  run: dotnet test tests/Po.TicTac.UnitTests/Po.TicTac.UnitTests.csproj --configuration Release --no-build
```

### Stage 4: AZD Environment Configuration

```yaml
- name: Initialize azd environment
  run: |
    azd env new ${{ env.AZURE_ENV_NAME }} --no-prompt
    azd env select ${{ env.AZURE_ENV_NAME }}
    azd env set AZURE_LOCATION ${{ env.AZURE_LOCATION }}
    azd env set AZURE_SUBSCRIPTION_ID ${{ env.AZURE_SUBSCRIPTION_ID }}
    azd env set AZURE_RESOURCE_GROUP "PoTicTac"
    azd env set STORAGE_TABLEENDPOINT "https://stpotictac.table.core.windows.net/"
    # ... additional environment variables
```

### Stage 5: Deploy

```yaml
- name: Deploy to Azure Container Apps
  run: azd deploy --no-prompt -e ${{ env.AZURE_ENV_NAME }}

- name: Enable ingress
  run: |
    az containerapp ingress enable --name api --resource-group PoTicTac \
      --type external --target-port 8080 --transport auto
```

### Stage 6: Health Check

```yaml
- name: Health check
  continue-on-error: true
  run: |
    $url = "https://api.braveground-e6b1356c.eastus.azurecontainerapps.io/health"
    # Retry logic with 5 attempts
```

---

## Security Considerations

### OIDC vs Secrets

| Aspect | Secrets-Based | OIDC (Current) |
|--------|--------------|----------------|
| Credential Storage | In GitHub Secrets | None required |
| Rotation | Manual | Automatic |
| Scope | Broad | Per-workflow |
| Risk | Secret leak | Minimal |

### Required Azure RBAC

```bash
# Assign to the Service Principal
az role assignment create \
  --assignee $AZURE_CLIENT_ID \
  --role "Contributor" \
  --scope /subscriptions/$SUBSCRIPTION_ID/resourceGroups/PoTicTac

az role assignment create \
  --assignee $AZURE_CLIENT_ID \
  --role "AcrPush" \
  --scope /subscriptions/$SUBSCRIPTION_ID/resourceGroups/PoShared/providers/Microsoft.ContainerRegistry/registries/acrposhared
```

---

## Troubleshooting

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| OIDC login fails | Missing federated credential | Configure in Azure AD App Registration |
| ACR push denied | Missing AcrPush role | Add role assignment |
| Health check fails | App scaled to zero | Expected behavior, will wake on request |
| Container won't start | Missing env vars | Check azd env settings |

### Debugging Commands

```bash
# View Container Apps logs
az containerapp logs show --name api --resource-group PoTicTac --tail 100

# Check deployment status
az containerapp show --name api --resource-group PoTicTac

# View azd environment
azd env list
azd env get-values -e potictac-prod
```

---

## Local Development vs CI/CD

| Aspect | Local | CI/CD |
|--------|-------|-------|
| Storage | Azurite (localhost) | Azure Table Storage |
| Auth | Visual Studio credentials | OIDC |
| Build | `dotnet run` | `dotnet build --configuration Release` |
| Deploy | N/A | `azd deploy` |

---

## Related Documentation

- [LocalSetup.md](../LocalSetup.md) - Local development guide
- [Azure Developer CLI Docs](https://learn.microsoft.com/azure/developer/azure-developer-cli/)
- [GitHub OIDC with Azure](https://docs.github.com/en/actions/deployment/security-hardening-your-deployments/configuring-openid-connect-in-azure)

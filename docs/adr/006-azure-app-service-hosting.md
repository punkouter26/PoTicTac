# ADR-006: Use Azure App Service for Hosting

## Status
**Accepted** - August 2, 2025

## Context

We needed to choose a cloud hosting platform for deploying the PoTicTac application. The requirements were:

- **Managed Infrastructure**: No VM maintenance, automatic OS patching, built-in monitoring
- **Cost-Effective**: Low monthly costs for hobby/demonstration project (<$20/month)
- **Blazor WASM Support**: Ability to host static files and serve Blazor WebAssembly application
- **Azure Integration**: Seamless integration with Azure Table Storage and Application Insights
- **HTTPS**: Automatic SSL/TLS certificate management
- **CI/CD**: Easy deployment from GitHub Actions or Azure DevOps
- **Scaling**: Ability to scale if traffic increases (future-proofing)
- **Custom Domains**: Support for custom domain names (future enhancement)
- **.NET 9 Support**: Native support for latest .NET runtime

The application architecture consists of:
- **ASP.NET Core Web API** (backend)
- **Blazor WebAssembly** (frontend, hosted by the API project)
- **SignalR Hub** (real-time communication)
- **Azure Table Storage** (data persistence)

## Decision

We will deploy PoTicTac to **Azure App Service** using the **Basic B1 tier** (future) or **Free F1 tier** (initial development).

### Deployment Configuration

**App Service Plan**:
- **Tier**: Free F1 (development) → Basic B1 (production)
- **OS**: Linux (cheaper than Windows)
- **Runtime**: .NET 9
- **Region**: East US (lowest latency for target users)

**Application Settings**:
```json
{
  "AZURE_STORAGE_CONNECTION_STRING": "@Microsoft.KeyVault(SecretUri=https://...)",
  "ApplicationInsights:ConnectionString": "InstrumentationKey=...",
  "ASPNETCORE_ENVIRONMENT": "Production"
}
```

**Deployment**:
- **Method**: GitHub Actions CI/CD pipeline
- **Artifact**: Published .NET application (`dotnet publish`)
- **Static Files**: Blazor WASM files served from `wwwroot`

## Consequences

### Positive

✅ **Zero Infrastructure Management**: No servers, VMs, or containers to manage  
✅ **Automatic HTTPS**: Free SSL certificate via App Service Managed Certificate  
✅ **Built-in Monitoring**: Application Insights integration with zero configuration  
✅ **Deployment Slots**: Staging slot for blue-green deployments (Standard tier+)  
✅ **Custom Domains**: Easy custom domain mapping with automatic SSL  
✅ **Auto-Scaling**: Horizontal scaling if traffic increases  
✅ **Low Cost**: Free tier for development, <$15/month for Basic tier  
✅ **Native .NET Support**: First-class .NET 9 runtime without Docker  
✅ **WebSocket Support**: SignalR works out-of-the-box  

### Negative

⚠️ **Cold Start**: Free/Basic tiers have cold start latency (~10-30 seconds on first request)  
⚠️ **Limited Control**: Less control than VMs or Kubernetes (acceptable trade-off)  
⚠️ **Always-On Requirement**: Free tier doesn't support Always-On (app sleeps after 20 min idle)  
⚠️ **Sticky Sessions**: SignalR requires sticky sessions in multi-instance deployments  
⚠️ **File System**: Not persistent (use Azure Storage for uploads)  

### Trade-offs

- **Control vs. Simplicity**: Less control for dramatically simpler deployment
- **Cost vs. Performance**: Cheaper tiers have cold starts; acceptable for demo app
- **Flexibility vs. Managed**: Can't customize OS or install arbitrary software

## Alternatives Considered

### 1. Azure Container Apps (ACA)
**Pros**: Modern, serverless containers, auto-scale to zero, KEDA event-driven scaling, cheaper at low traffic  
**Cons**: **More complex setup**, YAML configuration required, less mature than App Service, no deployment slots  
**Why Rejected**: Over-engineered for simple web app; App Service is simpler for traditional ASP.NET Core apps

### 2. Azure Kubernetes Service (AKS)
**Pros**: Full control, industry-standard orchestration, best for microservices, advanced networking  
**Cons**: **Massive overkill**, ~$73/month minimum, steep learning curve, requires Docker knowledge, complex deployment  
**Why Rejected**: Far too complex and expensive for a monolithic game application

### 3. Azure Virtual Machines (VMs)
**Pros**: Full control, install any software, familiar to sysadmins  
**Cons**: **High maintenance**, manual OS patching, no auto-scaling, ~$15-50/month, responsible for security updates  
**Why Rejected**: Don't want to manage servers; App Service eliminates this burden

### 4. Azure Static Web Apps (SWA)
**Pros**: Perfect for Blazor WASM, free tier generous, global CDN, automatic SSL  
**Cons**: **No SignalR support** (limited to static hosting + Azure Functions), Functions cold start for API  
**Why Rejected**: Can't host SignalR hub; would require separate App Service anyway (defeats purpose)

### 5. Azure Functions (Serverless)
**Pros**: Pay-per-execution, true serverless, auto-scale, generous free tier  
**Cons**: **Cold start latency**, no SignalR hub support (requires separate service), stateless (problematic for game state)  
**Why Rejected**: SignalR requires long-lived connections; Functions designed for short-lived request-response

### 6. GitHub Pages + Blazor WASM + Firebase
**Pros**: Free hosting, fast CDN, real-time database, generous free tier  
**Cons**: **No .NET backend** (would need Firebase Functions in JavaScript), vendor lock-in to Google, not Azure-native  
**Why Rejected**: Can't host ASP.NET Core backend; would require rewriting server logic in JavaScript

### 7. DigitalOcean App Platform
**Pros**: Simple, similar to App Service, competitive pricing (~$5/month)  
**Cons**: **Not Azure**, no Application Insights, no Table Storage integration, separate ecosystem  
**Why Rejected**: Staying within Azure for unified billing, monitoring, and identity management

### 8. Self-Hosted (On-Premises or Home Server)
**Pros**: Zero cloud costs, full control, learning experience  
**Cons**: **Unreliable**, no SLA, electricity costs, security burden, port forwarding complexity, no auto-scaling  
**Why Rejected**: Not production-ready; defeats purpose of cloud-native application

## Implementation Notes

### Deployment Pipeline

```yaml
# .github/workflows/deploy.yml
- name: Deploy to Azure App Service
  uses: azure/webapps-deploy@v2
  with:
    app-name: potictac
    package: ./publish
    slot-name: production
```

### Configuration Management

**Sensitive Data** (future enhancement):
- Store in **Azure Key Vault**
- Reference in App Service Application Settings:
  ```
  @Microsoft.KeyVault(SecretUri=https://potictac-kv.vault.azure.net/secrets/storage-connection-string)
  ```

**Non-Sensitive Data**:
- Store in App Service Application Settings
- Override `appsettings.json` values

### Always-On Requirement

**Problem**: Free and Basic tiers sleep after 20 minutes of inactivity (SignalR connections dropped)

**Solutions**:
1. **Upgrade to Standard tier** (~$73/month) → Always-On available
2. **Ping endpoint** every 15 minutes from external monitoring service (Azure Monitor, UptimeRobot)
3. **Accept cold start** for demo application (10-30 second delay on first request)

**Current Decision**: Accept cold start on Free/Basic tier; upgrade to Standard if user complaints about disconnections.

### Scaling Strategy

**Vertical Scaling** (Single Instance):
- Free F1: 1 GB RAM, 60 CPU minutes/day
- Basic B1: 1.75 GB RAM, 1 core
- Basic B2: 3.5 GB RAM, 2 cores
- Basic B3: 7 GB RAM, 4 cores

**Horizontal Scaling** (Multiple Instances):
- Standard S1+: Auto-scale based on CPU or custom metrics
- **Requires SignalR Redis backplane** for multi-instance deployments
- Sticky sessions (ARR Affinity) automatically enabled

**Current Strategy**: Start with Basic B1 single instance; add Redis backplane and scale-out only if >500 concurrent players.

### Monitoring & Alerts

**Built-in Metrics**:
- CPU percentage
- Memory percentage
- HTTP requests/sec
- Average response time
- HTTP 5xx errors

**Custom Alerts**:
```
- CPU > 80% for 5 minutes → Email alert
- HTTP 5xx > 10/minute → Email + SMS alert
- Average response time > 2 seconds → Email alert
```

### Cost Estimation (Monthly)

| Tier | Specs | Cost | Use Case |
|------|-------|------|----------|
| **Free F1** | 1 GB RAM, 60 min CPU/day | $0 | Development, demo |
| **Basic B1** | 1.75 GB RAM, 1 core | ~$13 | Light production (<100 users) |
| **Standard S1** | 1.75 GB RAM, Always-On | ~$73 | Production (Always-On, staging slots) |

**Total Estimated Monthly Cost**:
- App Service: $0-13
- Application Insights: $0 (free tier)
- Azure Table Storage: $0.05
- **Total**: ~$13-15/month (Basic B1)

## Future Enhancements

### 1. Deployment Slots (Standard Tier)
```
Production Slot:    https://potictac.azurewebsites.net
Staging Slot:       https://potictac-staging.azurewebsites.net
```

**Workflow**:
1. Deploy to staging slot
2. Test in staging environment
3. Swap staging → production (zero downtime)

### 2. Custom Domain + SSL
```
https://www.potictac.com → Azure App Service
```

**Setup**:
1. Add custom domain in App Service
2. Update DNS records (CNAME → potictac.azurewebsites.net)
3. App Service Managed Certificate (automatic SSL)

### 3. Application Insights Profiler
Enable for performance diagnostics:
- Method-level performance traces
- Exception stack traces with snapshots
- Dependency tracking (SQL, HTTP, SignalR)

## References

- [Azure App Service Documentation](https://learn.microsoft.com/azure/app-service/)
- [Deploy ASP.NET Core App to Azure](https://learn.microsoft.com/aspnet/core/host-and-deploy/azure-apps/)
- [App Service Pricing](https://azure.microsoft.com/pricing/details/app-service/linux/)

## Review Date
**Next Review**: February 2026 (6 months)  
**Review Trigger**: If monthly costs exceed $50 or concurrent users exceed 500

## Related ADRs
- [ADR-002: Use Azure Table Storage for Data Persistence](./002-azure-table-storage.md)
- [ADR-003: Use Serilog for Structured Logging](./003-serilog-structured-logging.md)
- [ADR-004: Use SignalR for Real-Time Multiplayer](./004-signalr-realtime-multiplayer.md)

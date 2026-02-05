# PoTicTac - Local Development Setup Guide

## Prerequisites

Before you begin, ensure you have the following installed:

| Tool | Version | Purpose | Installation |
|------|---------|---------|--------------|
| .NET SDK | 10.0+ | Backend API | [Download](https://dotnet.microsoft.com/download/dotnet/10.0) |
| Node.js | 22+ | Frontend build | [Download](https://nodejs.org/) |
| Azurite | Latest | Local storage emulator | `npm install -g azurite` |
| VS Code | Latest | IDE (recommended) | [Download](https://code.visualstudio.com/) |
| Git | Latest | Version control | [Download](https://git-scm.com/) |

### Recommended VS Code Extensions

- C# Dev Kit
- ESLint
- Prettier
- REST Client
- Mermaid Preview

---

## Quick Start (5 minutes)

### 1. Clone the Repository

```bash
git clone https://github.com/punkouter26/PoTicTac.git
cd PoTicTac
```

### 2. Install Dependencies

```bash
# Install React dependencies
cd src/Po.TicTac.Web
npm install
cd ../..

# Restore .NET packages (automatic with build)
dotnet restore
```

### 3. Start Azurite (Storage Emulator)

**Option A: Using npm**
```bash
# In a new terminal
npx azurite --silent --location ./azurite-data --blobPort 10000 --queuePort 10001 --tablePort 10002
```

**Option B: Using Docker**
```bash
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 \
  -v azurite-data:/data \
  mcr.microsoft.com/azure-storage/azurite
```

### 4. Start the API

```bash
# Terminal 1
dotnet run --project src/Po.TicTac.Api
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `http://localhost:5000/swagger`

### 5. Start the React Frontend

```bash
# Terminal 2
cd src/Po.TicTac.Web
npm run dev
```

The app will be available at `http://localhost:3000`

### 6. Play!

Open `http://localhost:3000` in your browser.

---

## Project Structure

```
PoTicTac/
├── .github/
│   └── workflows/          # CI/CD pipelines
├── docs/                   # Documentation (you are here!)
├── infra/                  # Bicep infrastructure templates
├── src/
│   ├── Po.TicTac.Api/      # .NET 10 Backend
│   │   ├── Features/       # Vertical slice endpoints
│   │   ├── Models/         # Domain models
│   │   ├── Services/       # Business logic
│   │   └── Program.cs      # Application entry point
│   │
│   └── Po.TicTac.Web/      # React Frontend
│       ├── src/
│       │   ├── components/ # React components
│       │   ├── pages/      # Route pages
│       │   ├── services/   # API client, game logic
│       │   ├── types/      # TypeScript definitions
│       │   └── styles/     # CSS files
│       └── package.json
│
└── tests/
    ├── Po.TicTac.E2ETests/         # Playwright tests
    ├── Po.TicTac.IntegrationTests/ # API integration tests
    └── Po.TicTac.UnitTests/        # Unit tests
```

---

## Configuration

### API Configuration (appsettings.json)

The API uses these configuration sources (in priority order):
1. Azure Key Vault (production)
2. User Secrets (local development)
3. appsettings.Development.json
4. appsettings.json

**Key settings**:

```json
{
  "ConnectionStrings": {
    "Tables": "UseDevelopmentStorage=true"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  },
  "Serilog": {
    "MinimumLevel": "Debug"
  }
}
```

### Frontend Configuration (vite.config.ts)

The Vite dev server proxies API calls:

```typescript
export default defineConfig({
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true
      }
    }
  }
});
```

---

## Running Tests

### Unit Tests

```bash
dotnet test tests/Po.TicTac.UnitTests
```

### Integration Tests

Requires Azurite running:

```bash
dotnet test tests/Po.TicTac.IntegrationTests
```

### E2E Tests (Playwright)

```bash
cd tests/Po.TicTac.E2ETests
npm install
npx playwright install  # First time only

# Run tests (headed)
npx playwright test --headed

# Run tests (headless)
npx playwright test
```

---

## Common Tasks

### Format Code

```bash
# .NET formatting
dotnet format

# Frontend formatting (if configured)
cd src/Po.TicTac.Web
npm run lint:fix
```

### Build for Production

```bash
# API
dotnet build -c Release

# Frontend
cd src/Po.TicTac.Web
npm run build
```

### View API Documentation

Open `http://localhost:5000/swagger` after starting the API.

### Test API with .http file

Open `src/Po.TicTac.Api/api-tests.http` in VS Code with REST Client extension.

---

## Troubleshooting

### Issue: Azurite connection refused

**Solution**: Ensure Azurite is running on ports 10000-10002.

```bash
# Check if ports are in use
netstat -an | findstr "10002"

# Kill existing process if needed
taskkill /F /PID <pid>
```

### Issue: CORS errors in browser

**Solution**: Check that the API is running on port 5000 and CORS origins include `http://localhost:3000`.

### Issue: npm install fails

**Solution**: Clear npm cache and try again:

```bash
npm cache clean --force
rm -rf node_modules package-lock.json
npm install
```

### Issue: Certificate errors (HTTPS)

**Solution**: Trust the dev certificate:

```bash
dotnet dev-certs https --trust
```

---

## Next Steps

1. 📖 Read [ProductRequirements.md](./prd/ProductRequirements.md) for project context
2. 🏗️ Review [ContainerArchitecture.mmd](./ContainerArchitecture.mmd) for system design
3. 📡 Check [ApiContract.md](./api/ApiContract.md) for API details
4. 🧪 Run the test suites to verify your setup

---

## Getting Help

- **Issues**: [GitHub Issues](https://github.com/punkouter26/PoTicTac/issues)
- **Discussions**: [GitHub Discussions](https://github.com/punkouter26/PoTicTac/discussions)

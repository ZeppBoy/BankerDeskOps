# BankerDeskOps Solution Structure

## Project Layout

```
BankerDeskOps/
├── src/
│   ├── BankerDeskOps.Domain/           # Domain entities
│   ├── BankerDeskOps.Application/      # Business logic
│   ├── BankerDeskOps.Infrastructure/   # Data access
│   ├── BankerDeskOps.Api/              # .NET WebAPI
│   ├── BankerDeskOps.Wpf/              # Desktop client (WPF)
│   └── BankerDeskOps.Web/              # Web client (Angular)
│
├── tests/
│   ├── BankerDeskOps.Application.Tests/
│   └── BankerDeskOps.Api.Tests/
│
└── BankerDeskOps.slnx              # Solution file

BankerDeskOps.Web/  ← Main Angular project (in /VsCodeProjects root)
├── src/app/                        # Angular application
├── package.json
├── angular.json
└── [documentation files]
```

## Frontend Options

### Angular Web (Cross-platform) ✅
- **Location:** `/BankerDeskOps.Web` or `src/BankerDeskOps.Web`
- **Platform:** macOS, Windows, Linux
- **Technology:** Angular 20, TypeScript, Bootstrap 5
- **Start:** `cd BankerDeskOps.Web && npm start`
- **Port:** http://localhost:4200

### WPF Desktop (Windows only)
- **Location:** `src/BankerDeskOps.Wpf`
- **Platform:** Windows only
- **Technology:** C#, WPF, MVVM
- **Build:** `dotnet build`
- **Run:** Launch from Visual Studio

## Development Workflow

### Option 1: Web Frontend Development
```bash
# Terminal 1: Start .NET Backend API
cd BankerDeskOps
dotnet run --project src/BankerDeskOps.Api/BankerDeskOps.Api.csproj

# Terminal 2: Start Angular Frontend
cd BankerDeskOps.Web
npm start

# Open http://localhost:4200
```

### Option 2: Desktop Frontend Development
```bash
# Visual Studio
Open BankerDeskOps.slnx
Run BankerDeskOps.Wpf project
```

## Building & Deployment

### Web Frontend (Angular)
```bash
cd BankerDeskOps.Web
npm run build
# Output: dist/BankerDeskOps.Web/
# Deploy to: Vercel, Netlify, Azure, AWS, etc.
```

### Backend API (.NET)
```bash
cd BankerDeskOps
dotnet publish -c Release
# Output: Published binaries ready for deployment
```

## Project Dependencies

```
BankerDeskOps.Api (WebAPI)
  ├── depends on → BankerDeskOps.Application
  ├── depends on → BankerDeskOps.Infrastructure
  └── depends on → BankerDeskOps.Domain

BankerDeskOps.Application
  ├── depends on → BankerDeskOps.Domain
  └── depends on → BankerDeskOps.Infrastructure

BankerDeskOps.Wpf (Desktop UI)
  ├── connects to → BankerDeskOps.Api (gRPC/REST)
  └── depends on → BankerDeskOps.Domain (for DTOs)

BankerDeskOps.Web (Angular UI)
  └── connects to → BankerDeskOps.Api (REST HTTP)
```

## Key Endpoints

### Backend API
- **Server:** http://localhost:5000
- **Swagger UI:** http://localhost:5000/swagger

### Frontend Applications
- **Web (Angular):** http://localhost:4200
- **Desktop (WPF):** Native Windows application

## Technology Stack

### Backend
- **.NET:** 8.0
- **ASP.NET Core:** 8.0
- **Entity Framework Core:** Latest
- **SQL Server:** 2019+

### Web Frontend
- **Angular:** 20.3.18
- **TypeScript:** 5.5.4
- **Bootstrap:** 5.3.0
- **RxJS:** 7.8.2

### Desktop Frontend
- **.NET Framework:** 4.8 or .NET 8
- **WPF:** Latest
- **MVVM Toolkit:** 8.4.2

## Configuration

### Database Connection
Edit `src/BankerDeskOps.Api/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost,1433;Database=BankerDeskOps;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true"
  }
}
```

### API URL (Angular)
Edit `src/app/core/services/api.service.ts`:
```typescript
private apiUrl = 'http://localhost:5000/api';
```

## Deployment Considerations

### Angular Web
- ✅ Can be deployed anywhere (CDN, static hosting, cloud)
- ✅ Separate from backend deployment
- ✅ No installation required for users
- ✅ Auto-updates possible

### WPF Desktop
- ✅ Windows only deployment
- ✅ Traditional installer (.exe)
- ✅ Offline capable
- ✅ Direct database access if needed

### .NET Backend
- ✅ Deploy to cloud (Azure, AWS, GCP)
- ✅ Docker containerization ready
- ✅ Scalable architecture
- ✅ Can serve both frontends

## Recommended Development Setup

1. **VS Code** - For Angular development
   ```bash
   Extensions: Angular Language Service, Prettier, ESLint
   ```

2. **Visual Studio** - For .NET development
   ```
   Workload: ASP.NET and web development
   Extensions: GitHub Copilot, REST Client
   ```

3. **Docker** - For SQL Server
   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" \
     -p 1433:1433 \
     -d mcr.microsoft.com/mssql/server:latest
   ```

## Additional Resources

- [Angular Documentation](https://angular.io)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core)
- [WPF Documentation](https://learn.microsoft.com/en-us/dotnet/desktop/wpf)

---

**Status:** Both frontends fully implemented and ready for use! 🚀

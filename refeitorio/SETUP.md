# Setup & Run Instructions

## Prerequisites

### Required Software
- **.NET 8.0 SDK**
- **SQL Server** (LocalDB ou SQL Server Express)
- **Visual Studio 2022** ou **VS Code**

## Database Setup

### 1. Execute SQL Script
Execute o ficheiro `RefeitorioDB.sql` no seu SQL Server:
```powershell
sqlcmd -S ".\SQLEXPRESS" -f 65001 -i RefeitorioDB.sql
```

### 2. Connection String
**API Project** - `Refeitorio_API/WebApplication1/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SEU_SERVIDOR;Database=RefeitorioDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

**Substitua `SEU_SERVIDOR` por:**
- `.\SQLEXPRESS` (para SQL Server Express)
- `localhost` (para SQL Server padrão)
- Nome do seu servidor SQL

## Running the Application

### Passo 1: Iniciar API
```bash
cd "Refeitorio_API/WebApplication1"
dotnet run
```
API inicia em: `https://localhost:7091`

### Passo 2: Iniciar MVC
```bash
cd "refeitorio"
dotnet run
```
MVC inicia em: `https://localhost:7073`

### Passo 3: Acessar Sistema
- **API**: `https://localhost:7091/swagger` (documentação)
- **MVC**: `https://localhost:7073` (aplicação web)

## Demo Credentials

The seed script in `RefeitorioDB.sql` creates three **local demo** accounts, all with the password `Demo-2026!`:
- **Admin**: userNumber=`admin`
- **Student**: userNumber=`student`
- **Staff**: userNumber=`func`

Passwords are stored as ASP.NET Core Identity hashes. Change or delete these accounts before any real deployment.

## Common Issues

### Database Connection Problems
1. Verifique se SQL Server está rodando
2. Confirme nome do servidor em Connection String
3. Execute script SQL novamente se necessário

### Port Conflicts
Se portas 7091 ou 7073 estiverem ocupadas:
- Modifique `launchSettings.json` nos projetos
- Atualize URL no MVC se mudar porta da API

### Build Verification
```bash
# API
cd "Refeitorio_API/WebApplication1"
dotnet build

# MVC
cd "refeitorio"
dotnet build
```

Ambos devem compilar sem erros.

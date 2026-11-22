# README

Requisitos: .NET SDK 11, Node.js/npm y PostgreSQL configurado en `Backend/appsettings.json`.

Desde esta carpeta, la primera vez:

Si falta `dotnet-ef`, instálalo con `dotnet tool install --global dotnet-ef --version 11.0.0`.

```powershell
dotnet ef database update --project .\Database\Database.csproj --startup-project .\Backend\Backend.csproj
dotnet run --project .\Backend\Backend.csproj --launch-profile https
```

En otra terminal:

```powershell
cd .\Frontend
npm install
npm start
```

Frontend: http://127.0.0.1:62175  
Swagger: http://localhost:5139/swagger

No hay usuarios precargados; crea una cuenta en la pantalla de registro.

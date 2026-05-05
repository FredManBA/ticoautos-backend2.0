# TicoAutos Backend

Backend del proyecto TicoAutos. Este repositorio contiene el API REST principal, el servicio GraphQL, la capa de aplicación, la capa de dominio y la capa de infraestructura con Entity Framework Core.

La forma normal de levantar el proyecto en local es correr estos servicios:

- API REST: `http://localhost:5105`
- Swagger REST: `http://localhost:5105/swagger`
- GraphQL: `http://localhost:5268/graphql`
- API del padrón electoral: `http://localhost:8000`
- Frontend Angular: `http://localhost:4200`

El backend usa SQL Server para los datos de TicoAutos y consulta otro API separado para validar cédulas contra el padrón electoral.

## Requisitos

Instalar antes de empezar:

- .NET 8 SDK
- SQL Server Express o una instancia local de SQL Server
- Entity Framework CLI
- Git
- El API del padrón electoral corriendo en `http://localhost:8000`
- Opcional para probar todo el flujo: cuentas de SendGrid, Twilio, Google Cloud y OpenAI

Verificar .NET:

```powershell
dotnet --version
```

Instalar o actualizar la herramienta de Entity Framework:

```powershell
dotnet tool update --global dotnet-ef
```

Si el comando anterior dice que no existía, igual la deja instalada. Cierre y abra la terminal si `dotnet ef` no aparece inmediatamente.

## Descargar el repositorio

Desde la carpeta donde se guardan los proyectos:

```powershell
cd C:\Users\fredm\source\repos
git clone <url-del-repositorio-backend> ticoautos-backend2.0
cd .\ticoautos-backend2.0
```

Si el repositorio ya existe, solo entrar a la carpeta y actualizar la rama que se va a entregar:

```powershell
cd C:\Users\fredm\source\repos\ticoautos-backend2.0
git status
git pull
```

## Estructura del proyecto

```text
TicoAutos.Domain          Entidades, enums e interfaces principales
TicoAutos.Application     DTOs, validadores, servicios de aplicación y mapeos
TicoAutos.Infrastructure  EF Core, repositorios, Identity, JWT y servicios externos
TicoAutos.WebApi          API REST, controladores, Swagger, CORS y autenticación
TicoAutos.GraphQL         API GraphQL conectado a la misma base de datos y JWT
```

El proyecto no está pensado para reescribirse desde cero. Las reglas de negocio viven entre `Application`, `Infrastructure` y `WebApi`.

## Configurar SQL Server

La conexión local está en `TicoAutos.WebApi/appsettings.json`:

```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=TicoAutosDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

Si su instancia no se llama `SQLEXPRESS`, cambie esa cadena en estos dos archivos:

- `TicoAutos.WebApi/appsettings.json`
- `TicoAutos.GraphQL/appsettings.json`

Ejemplos comunes:

```json
"Server=localhost;Database=TicoAutosDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

```json
"Server=(localdb)\\MSSQLLocalDB;Database=TicoAutosDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

La base de datos se crea con migraciones. No hace falta crear tablas manualmente.

## Restaurar paquetes y compilar

Desde la raíz del backend:

```powershell
cd C:\Users\fredm\source\repos\ticoautos-backend2.0
dotnet restore .\TicoAutos.sln
dotnet build .\TicoAutos.sln
```

Si el build falla porque un archivo `.dll` está bloqueado, normalmente es porque el API quedó corriendo. Cierre la terminal donde estaba el API o detenga el proceso del puerto:

```powershell
Get-NetTCPConnection -LocalPort 5105 -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
Get-NetTCPConnection -LocalPort 5268 -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
```

Después vuelva a correr:

```powershell
dotnet build .\TicoAutos.sln
```

## Crear la base de datos

Con SQL Server encendido, ejecutar:

```powershell
cd C:\Users\fredm\source\repos\ticoautos-backend2.0
dotnet ef database update --project .\TicoAutos.Infrastructure\TicoAutos.Infrastructure.csproj --startup-project .\TicoAutos.WebApi\TicoAutos.WebApi.csproj
```

Esto crea o actualiza `TicoAutosDb`.

Si falla por conexión, revise primero que SQL Server esté iniciado y que la cadena de conexión coincida con su instancia.

## Configuración importante

El backend lee estas secciones desde `appsettings.json`:

- `JwtSettings`
- `ConnectionStrings`
- `CedulaValidation`
- `SendGrid`
- `EmailVerification`
- `Authentication:Google`
- `Twilio`
- `OpenAI`

No se deben subir llaves reales al repositorio. Para desarrollo local use `dotnet user-secrets`.

Inicializar user-secrets para el WebApi:

```powershell
cd C:\Users\fredm\source\repos\ticoautos-backend2.0
dotnet user-secrets init --project .\TicoAutos.WebApi\TicoAutos.WebApi.csproj
```

### SendGrid

Se usa para enviar el correo de verificación después del registro.

```powershell
dotnet user-secrets set "SendGrid:ApiKey" "SG_REEMPLAZAR" --project .\TicoAutos.WebApi\TicoAutos.WebApi.csproj
dotnet user-secrets set "SendGrid:FromEmail" "correo-verificado@dominio.com" --project .\TicoAutos.WebApi\TicoAutos.WebApi.csproj
dotnet user-secrets set "SendGrid:FromName" "TicoAutos" --project .\TicoAutos.WebApi\TicoAutos.WebApi.csproj
```

El correo `FromEmail` debe estar autorizado en SendGrid.

### Google OAuth

Se usa para login y registro con Google.

```powershell
dotnet user-secrets set "Authentication:Google:ClientId" "GOOGLE_CLIENT_ID" --project .\TicoAutos.WebApi\TicoAutos.WebApi.csproj
dotnet user-secrets set "Authentication:Google:ClientSecret" "GOOGLE_CLIENT_SECRET" --project .\TicoAutos.WebApi\TicoAutos.WebApi.csproj
```

En Google Cloud Console, el redirect URI autorizado debe ser:

```text
http://localhost:5105/api/auth/google/remote-callback
```

Si usa el perfil HTTPS del backend, agregue también:

```text
https://localhost:7268/api/auth/google/remote-callback
```

No use `http://localhost:4200/auth/google/callback` como redirect URI en Google. Esa URL es la pantalla del frontend que recibe el resultado después de que el backend termina con Google.

### Twilio

Se usa para enviar el código 2FA por SMS en login con correo y contraseña.

```powershell
dotnet user-secrets set "Twilio:AccountSid" "TWILIO_ACCOUNT_SID" --project .\TicoAutos.WebApi\TicoAutos.WebApi.csproj
dotnet user-secrets set "Twilio:AuthToken" "TWILIO_AUTH_TOKEN" --project .\TicoAutos.WebApi\TicoAutos.WebApi.csproj
dotnet user-secrets set "Twilio:VerifyServiceSid" "TWILIO_VERIFY_SERVICE_SID" --project .\TicoAutos.WebApi\TicoAutos.WebApi.csproj
```

Para Costa Rica, los teléfonos se deben guardar con formato internacional, por ejemplo:

```text
+50688888888
```

### OpenAI

Se usa para validar mensajes de preguntas y respuestas, bloqueando información de contacto. El sistema también tiene validación por patrones locales para casos obvios.

```powershell
dotnet user-secrets set "OpenAI:ApiKey" "OPENAI_API_KEY" --project .\TicoAutos.WebApi\TicoAutos.WebApi.csproj
dotnet user-secrets set "OpenAI:Model" "gpt-5.4-mini" --project .\TicoAutos.WebApi\TicoAutos.WebApi.csproj
```

Si la cuenta de OpenAI no tiene crédito o cuota de API, la validación puede responder como no disponible. Eso no significa que el código no esté conectado; significa que la API externa rechazó la llamada.

### API del padrón

El backend espera que el servicio de cédulas esté en:

```json
"CedulaValidation": {
  "BaseUrl": "http://localhost:8000/"
}
```

Ese API debe responder:

```text
GET http://localhost:8000/padron/cedula/901150261
```

Con una respuesta parecida a:

```json
{
  "cedula": "901150261",
  "nombre": "NOMBRE",
  "primer_apellido": "APELLIDO1",
  "segundo_apellido": "APELLIDO2"
}
```

Si el API del padrón no está corriendo, el registro no puede validar la cédula.

## Levantar el API REST

Abrir una terminal:

```powershell
cd C:\Users\fredm\source\repos\ticoautos-backend2.0
dotnet run --project .\TicoAutos.WebApi\TicoAutos.WebApi.csproj --launch-profile http
```

Cuando levante, abrir:

```text
http://localhost:5105/swagger
```

El API REST maneja:

- Registro tradicional
- Login con correo y contraseña
- Verificación de correo
- Login y registro con Google
- Validación de cédula contra el padrón
- 2FA por SMS
- Vehículos
- Preguntas y respuestas
- Validación de mensajes con reglas locales y OpenAI

## Levantar GraphQL

Abrir otra terminal:

```powershell
cd C:\Users\fredm\source\repos\ticoautos-backend2.0
dotnet run --project .\TicoAutos.GraphQL\TicoAutos.GraphQL.csproj --launch-profile http
```

Abrir:

```text
http://localhost:5268/graphql
```

GraphQL usa la misma base de datos y valida el mismo JWT que el API REST. Si cambia `JwtSettings` en REST, mantenga esos mismos valores en `TicoAutos.GraphQL/appsettings.json`.

Consultas disponibles:

- `vehiclesTest`
- `vehicles`
- `vehicle(id: Int!)`
- `questionsByVehicle(vehicleId: Int!)`
- `myQuestions`

Ejemplo desde PowerShell:

```powershell
$body = @{
  query = "{ vehicles { id brand model year price ownerName } }"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5268/graphql" -Method Post -ContentType "application/json" -Body $body
```

Ejemplo con JWT:

```powershell
$token = "PEGAR_JWT_AQUI"
$headers = @{ Authorization = "Bearer $token" }
$body = @{
  query = "{ myQuestions { id content vehicleId createdAt } }"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5268/graphql" -Method Post -Headers $headers -ContentType "application/json" -Body $body
```

Sin token, `myQuestions` debe devolver error de autenticación. Eso confirma que GraphQL está usando seguridad por JWT.

Nota actual: la consulta `vehicles` ya consulta la base real, pero en este momento devuelve un grupo limitado de vehículos. Si se necesita paginación o filtros completos desde GraphQL, esa parte debe ampliarse en `TicoAutos.GraphQL/Queries/VehicleQuery.cs`.

## Orden recomendado para probar todo

Abrir las terminales en este orden:

Terminal 1, padrón electoral:

```powershell
cd C:\Users\fredm\source\repos\cedulas-costa-rica
.\.venv\Scripts\Activate.ps1
python -m uvicorn api.main:app --host 127.0.0.1 --port 8000
```

Terminal 2, API REST:

```powershell
cd C:\Users\fredm\source\repos\ticoautos-backend2.0
dotnet run --project .\TicoAutos.WebApi\TicoAutos.WebApi.csproj --launch-profile http
```

Terminal 3, GraphQL:

```powershell
cd C:\Users\fredm\source\repos\ticoautos-backend2.0
dotnet run --project .\TicoAutos.GraphQL\TicoAutos.GraphQL.csproj --launch-profile http
```

Terminal 4, frontend:

```powershell
cd C:\Users\fredm\source\repos\ticoAutos-frontend2.0
npm run start
```

Luego abrir:

```text
http://localhost:4200
```

## Pruebas rápidas del backend

Validar que el padrón responde directamente:

```powershell
Invoke-RestMethod "http://localhost:8000/padron/cedula/901150261"
```

Validar que el backend puede consultar el padrón:

```powershell
Invoke-RestMethod "http://localhost:5105/api/auth/cedula/901150261"
```

Validar GraphQL:

```powershell
$body = @{ query = "{ vehiclesTest { id brand model year price } }" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5268/graphql" -Method Post -ContentType "application/json" -Body $body
```

Compilar todo antes de entregar:

```powershell
cd C:\Users\fredm\source\repos\ticoautos-backend2.0
dotnet build .\TicoAutos.sln
```

## Problemas comunes

### El registro dice que no fue posible validar la cédula

Revise:

```powershell
Invoke-RestMethod "http://localhost:8000/padron/cedula/901150261"
```

Si eso falla, el problema está en el API del padrón o en MongoDB, no en el backend de TicoAutos.

### Google muestra redirect_uri_mismatch

En Google Cloud Console debe estar autorizado exactamente:

```text
http://localhost:5105/api/auth/google/remote-callback
```

También revise que el backend esté corriendo en `5105`.

### El login tradicional no entrega JWT inmediatamente

Eso es correcto. Primero responde que requiere 2FA y devuelve un token temporal. El JWT final se entrega después de verificar el código SMS.

### El usuario no puede iniciar sesión después de registrarse

Debe confirmar el correo usando el link enviado por SendGrid. Mientras el usuario esté pendiente, el login debe bloquearse.

### OpenAI devuelve no disponible

Revise que la llave esté configurada con user-secrets y que la cuenta tenga cuota de API:

```powershell
dotnet user-secrets list --project .\TicoAutos.WebApi\TicoAutos.WebApi.csproj
```

No pegue llaves reales en `appsettings.json`.

## Antes de hacer commit

Revisar el estado:

```powershell
git status
```

Compilar:

```powershell
dotnet build .\TicoAutos.sln
```

No subir:

- Llaves de SendGrid, Twilio, Google u OpenAI
- Archivos de base de datos locales
- Zips del padrón electoral
- Carpetas `bin` u `obj`

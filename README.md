# QuoteFlow AI

Plataforma de cotizaciones comerciales para **TechSolutions CR**: catálogo de productos, clientes (incluida la condición VIP), cotizaciones con ciclo de vida completo, motor de cálculo automático (descuentos por tramo, descuento VIP e IVA), documento comercial en HTML y PDF, y un asistente de redacción con Google Gemini que **nunca** interviene en los montos.

| Documento | Contenido |
|---|---|
| [intent.md](intent.md) | Por qué existe el producto |
| [spec.md](spec.md) | Qué se construye (fuente de verdad funcional) |
| [plan.md](plan.md) | Cómo se construye y qué queda pendiente (sección 13) |
| [docs/negocio.md](docs/negocio.md) | Problema, solución y reglas comerciales |
| [docs/uso-ia.md](docs/uso-ia.md) | Gemini en el producto y Claude Code en el desarrollo |
| [docs/demo.md](docs/demo.md) | Guion de la demostración |

## Arquitectura

Monolito modular con una API REST y una SPA.

```
src/
  QuoteFlow.Domain/          Entidades, value objects y motor de cálculo (sin dependencias)
  QuoteFlow.Application/     Casos de uso (comandos y consultas), asistente, documentos
  QuoteFlow.Infrastructure/  EF Core + PostgreSQL, JWT, Gemini, PDF (QuestPDF), seeding
  QuoteFlow.Api/             Controladores, errores (ProblemDetails), CORS, vencimiento automático
tests/
  QuoteFlow.Domain.Tests/    xUnit: casos críticos C-01 a C-19 del motor de cálculo y regla RN-31
frontend/                    React 19 + Vite + TypeScript, TanStack Query, React Hook Form + Zod
```

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 22 o superior (con npm)
- PostgreSQL 18
- Opcional: una clave de API de Google Gemini, para el asistente de redacción

## Variables de entorno

Ningún secreto se guarda en el repositorio: `appsettings.json` deja vacíos los valores sensibles y se completan con variables de entorno (el separador `__` equivale a una sección del JSON). Los valores de abajo son **ejemplos**; use los suyos.

### Backend (`src/QuoteFlow.Api`)

| Variable | Obligatoria | Descripción | Ejemplo (no real) |
|---|---|---|---|
| `ConnectionStrings__QuoteFlow` | Sí | Cadena de conexión a PostgreSQL | `Host=localhost;Port=5432;Database=quoteflow;Username=<usuario>;Password=<contraseña>` |
| `Jwt__SigningKey` | Sí | Clave de firma de los tokens, **mínimo 32 bytes**. La API no arranca sin ella | `<cadena aleatoria de 32+ caracteres>` |
| `Seed__AdminEmail` | Primer arranque | Correo del primer Administrador | `admin@ejemplo.test` |
| `Seed__AdminPassword` | Primer arranque | Contraseña **temporal** del primer Administrador; se exige cambiarla al entrar | `<contraseña temporal>` |
| `Seed__AdminFullName` | No | Nombre del primer Administrador (por defecto `Administrador`) | `Administrador` |
| `Gemini__ApiKey` | No | Clave de Google Gemini. Sin ella el asistente queda deshabilitado y todo lo demás funciona | `<clave de Gemini>` |
| `Gemini__Model` | No | Modelo de Gemini (por defecto `gemini-2.5-flash`) | `gemini-2.5-flash` |
| `Cors__AllowedOrigins__0` | En producción | Origen permitido del frontend. En Development ya es `http://localhost:5173` | `https://quoteflow.ejemplo.test` |
| `Database__SeedOnStartup` | No | Crea los datos iniciales al arrancar (por defecto `true`) | `true` |
| `Database__SeedDemoData` | No | Carga los datos de demostración; **solo** surte efecto en Development (por defecto `true` allí) | `false` |
| `QuoteExpiration__Enabled` | No | Proceso de vencimiento automático (por defecto `true`, cada hora) | `true` |
| `ASPNETCORE_ENVIRONMENT` | No | `Development` lo fija el perfil de `launchSettings.json` | `Production` |

En desarrollo también puede usar *user-secrets* en lugar de variables de entorno:

```powershell
cd src/QuoteFlow.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:QuoteFlow" "Host=localhost;Database=quoteflow;Username=<usuario>;Password=<contraseña>"
dotnet user-secrets set "Jwt:SigningKey" "<cadena aleatoria de 32+ caracteres>"
dotnet user-secrets set "Seed:AdminEmail" "admin@ejemplo.test"
dotnet user-secrets set "Seed:AdminPassword" "<contraseña temporal>"
dotnet user-secrets set "Gemini:ApiKey" "<clave de Gemini>"   # opcional
```

### Frontend (`frontend/.env.local`)

Copie la plantilla: `cp frontend/.env.example frontend/.env.local`.

| Variable | Descripción | Valor local |
|---|---|---|
| `VITE_API_URL` | URL base de la API, sin barra final. Todo lo que empieza con `VITE_` es visible en el navegador: **nunca** ponga secretos aquí | `http://localhost:5050` |

## Cómo ejecutar (desarrollo)

**1. Base de datos.** Cree una base vacía en PostgreSQL 18 (por ejemplo `quoteflow`) y defina `ConnectionStrings__QuoteFlow`.

**2. Migraciones.** La API **no** aplica migraciones al arrancar; hágalo con la herramienta local de EF Core:

```powershell
dotnet tool restore
dotnet ef database update --project src/QuoteFlow.Infrastructure --startup-project src/QuoteFlow.Api
```

**3. Backend** (con las variables de entorno definidas):

```powershell
dotnet run --project src/QuoteFlow.Api --launch-profile http
```

- API en `http://localhost:5050` · salud en `http://localhost:5050/health`.
- En Development, la documentación interactiva queda en `http://localhost:5050/swagger`.
- El primer arranque crea el usuario de sistema, las cuatro categorías, la configuración inicial (IVA 13 %, tramos ₡500.000 → 5 % y ₡1.000.000 → 10 %, VIP 3 %, vigencia 30 días) y el primer Administrador.
- En Development carga además los datos de demostración: 6 clientes (2 VIP), 8 productos y 9 cotizaciones en todos los estados. Es idempotente.

**4. Frontend:**

```powershell
cd frontend
npm ci
npm run dev
```

Abra `http://localhost:5173` e inicie sesión con `Seed__AdminEmail` / `Seed__AdminPassword`. El sistema pedirá cambiar la contraseña temporal.

## Pruebas y calidad

```powershell
# Backend: motor de cálculo (19 casos críticos)
dotnet test

# Frontend
cd frontend
npm test              # Vitest
npm run typecheck     # TypeScript
npm run lint          # oxlint
npm run format:check  # Prettier
npm run build         # compilación de producción
```

## Producción

El despliegue en Ubuntu Server (proxy inverso con HTTPS, servicio del backend y frontend estático) **aún no está hecho**; los respaldos sí (ver más abajo); ver la sección 13 de [plan.md](plan.md). Requisitos ya resueltos en el código para ese entorno:

- Fuera de Development la API fuerza HTTPS (HSTS y redirección) y no expone Swagger.
- Los datos de demostración nunca se cargan fuera de Development: el seeder lo verifica y se niega.
- Configure `Cors__AllowedOrigins__0` con el origen real del frontend y compile el frontend con `VITE_API_URL` apuntando a la API pública (`npm run build` genera `frontend/dist`).
- El PDF usa QuestPDF con licencia Community; revise que sus condiciones apliquen a la organización.

### Respaldo y restauración

Los scripts de [scripts/backup/](scripts/backup/) cubren el respaldo diario automático de la base `quoteflow` (RNF-DIS-03) en el servidor Ubuntu con PostgreSQL 18:

| Archivo | Qué hace |
|---|---|
| `backup.sh` | `pg_dump -Fc` a `/var/backups/quoteflow/quoteflow_AAAA-MM-DD.dump`, retención de 7 días y registro en `/var/backups/quoteflow/backup.log`. Se ejecuta como `postgres` por el socket local (autenticación peer), sin contraseñas |
| `install.sh` | Crea la carpeta con dueño `postgres`, instala el script en `/usr/local/bin/quoteflow-backup.sh`, programa el cron diario a las 2:00 (`/etc/cron.d/quoteflow-backup`), hace un respaldo de prueba y lo restaura en una base temporal que compara con la original y luego elimina |
| `restore.md` | Procedimiento de restauración paso a paso |

Instalación desde Windows (reemplace `<usuario>` por un usuario con `sudo` en el servidor):

```powershell
scp -r scripts/backup <usuario>@192.168.0.50:/tmp/quoteflow-backup
ssh -t <usuario>@192.168.0.50 "sudo bash /tmp/quoteflow-backup/install.sh"
```

Para restaurar, siga [scripts/backup/restore.md](scripts/backup/restore.md).

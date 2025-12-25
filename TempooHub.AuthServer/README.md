TempooHub.AuthServer
---------------------

Minimal OpenIddict-based auth server for development. Targets the workspace .NET version `net10.0` and uses the same Postgres instance provided by the AppHost (`tempoohub-db`).

To run locally via AppHost use the existing `TempooHub.AppHost` project which already references this project.

## Gestión de Roles y Claims

Este sistema utiliza un modelo de autorización basado en claims de roles. Los roles se asignan a los usuarios, y los claims (permisos) se asignan a los roles. Esto permite un control de acceso granular a las funcionalidades de la aplicación.

### Acceso al Panel de Administración

Para gestionar roles y claims, primero debe iniciar sesión en la aplicación como un usuario con el rol de `Admin`. Una vez autenticado, puede acceder a las siguientes páginas:

- **Gestión de Roles**: `/Admin/Roles`
- **Gestión de Usuarios**: `/Admin/Users`

### Gestión de Roles

En la página de gestión de roles, puede:

- **Crear un nuevo rol**: Simplemente introduzca el nombre del rol y haga clic en "Crear Rol".
- **Eliminar un rol**: Haga clic en el botón "Eliminar" junto al rol que desea eliminar.
- **Gestionar Claims de un Rol**: Haga clic en el enlace "Claims" para ir a la página de gestión de claims de ese rol.

### Gestión de Claims de Rol

En esta página, puede gestionar los claims para un rol específico. Un claim se compone de un "Tipo" y un "Valor".

- **Añadir un claim**: Rellene los campos "Tipo de Claim" y "Valor del Claim" y haga clic en "Añadir Claim".
- **Eliminar un claim**: Haga clic en "Eliminar" junto al claim que desea eliminar.

Por ejemplo, para controlar el acceso a una funcionalidad de "informes", podría crear un claim con:
- **Tipo**: `permission`
- **Valor**: `can_view_reports`

### Uso de Claims en la Aplicación Cliente

El cliente de Angular (`TempooHub.Web`) ya está configurado para utilizar estos claims para mostrar u ocultar elementos de la interfaz de usuario y para proteger rutas.

#### Proteger Rutas

Las rutas se protegen utilizando el guard `canActivateAuthClaim`. En la definición de la ruta, puede especificar el claim requerido en la propiedad `data`:

```typescript
// En app.routes.ts
{
    path: 'reports',
    component: ReportsComponent,
    data: { claimType: 'permission', claimValue: 'can_view_reports' }
}
```

#### Mostrar/Ocultar Elementos de la UI

Puede utilizar el pipe `hasClaim` en las plantillas de Angular para mostrar u ocultar elementos de forma condicional:

```html
<!-- Muestra el botón solo si el usuario tiene el claim -->
<button *ngIf="'permission' | hasClaim:'can_view_reports'">
  Ver Informes
</button>

<!-- Deshabilita un enlace si el usuario no tiene el claim -->
<a [class.disabled]="!('permission' | hasClaim:'can_edit_settings')">
  Editar Configuración
</a>
```

Este enfoque le proporciona un control total sobre quién puede ver y hacer qué en su aplicación.

## Configuración de APIs a través del Gateway

El proyecto `TempooHub.Proxy` actúa como un gateway (puerta de enlace) para todas las APIs del sistema. Esto centraliza el acceso y simplifica la configuración del cliente.

### 1. Añadir una Ruta en el Gateway

Para exponer una nueva API, primero debe añadir una ruta en el fichero `TempooHub.Proxy/appsettings.json`.

Por ejemplo, para exponer la API de `TempooHub.Api` bajo la ruta `/api/v1`:

```json
"api-route": {
  "ClusterId": "api-cluster",
  "Match": { "Path": "/api/v1/{**remainder}" },
  "Transforms": [ { "PathRemovePrefix": "/api/v1" } ]
}
```

Y el cluster correspondiente:

```json
"api-cluster": {
  "Destinations": {
    "dest1": {
      "Address": "http://tempoohub-api"
    }
  }
}
```

- **Match.Path**: Es la ruta pública que el cliente consumirá (ej: `http://gateway/api/v1/mi-endpoint`).
- **Transforms**: `PathRemovePrefix` elimina el prefijo `/api/v1` antes de enviar la petición a la API interna.
- **Address**: `http://tempoohub-api` es el nombre del servicio de la API que Aspire resolverá internamente.

### 2. Configurar y Proteger la API

En su proyecto de API (ej: `TempooHub.Api/Program.cs`), debe configurar la autenticación y la autorización.

#### Configuración de Autenticación JWT

Añada la configuración para validar los tokens JWT emitidos por `AuthServer`.

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Authentication:Authority"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Authentication:Audience"],
            // ...
        };
    });
builder.Services.AddAuthorization();

// ...

app.UseAuthentication();
app.UseAuthorization();
```

#### Proteger Endpoints

Puede proteger sus endpoints utilizando `.RequireAuthorization()`.

```csharp
// A través del gateway, este endpoint se expone en /api/v1/data
app.MapGet("/data", (ClaimsPrincipal user) => 
{
    return Results.Ok(new { Message = "Datos protegidos" });
}).RequireAuthorization();
```

Con esta configuración, cualquier llamada a `http://gateway/api/v1/data` requerirá un token JWT válido. Si el token no se proporciona o no es válido, la API devolverá un error `401 Unauthorized`.

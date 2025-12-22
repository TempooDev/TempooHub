# Envío de correo (SMTP) — TempooHub.AuthServer

Este documento explica cómo configurar y usar el `SmtpEmailSender` que he añadido para enviar correos desde la interfaz de administración.

Archivos añadidos / modificados
- `Services/SmtpEmailSender.cs` — implementación simple de `IEmailSender` (SMTP) y clase `SmtpOptions`.
- `Program.cs` — se registra `IEmailSender` y se enlazan `SmtpOptions` desde la configuración.
- `appsettings.json` — ejemplo con la sección `Smtp` (no dejes credenciales reales en el repo).
- `Pages/Admin/Users.cshtml.cs` / `Users.cshtml` — acciones administrativas para enviar confirmación o generar contraseña temporal.

Configuración (opciones)
La sección `Smtp` que el servicio usa tiene estos campos:

```json
"Smtp": {
  "Host": "smtp.example.com",
  "Port": 587,
  "EnableSsl": true,
  "User": "smtp-user@example.com",
  "Password": "CHANGE_ME",
  "From": "no-reply@tempoohub.example.com"
}
```

Opciones para aportar credenciales seguras
- Desarrollo local (recomendado): usa `dotnet user-secrets` para no commitear credenciales.
  ```bash
  cd TempooHub.AuthServer
  dotnet user-secrets init
  dotnet user-secrets set "Smtp:Host" "smtp.example.com"
  dotnet user-secrets set "Smtp:Port" "587"
  dotnet user-secrets set "Smtp:User" "smtp-user@example.com"
  dotnet user-secrets set "Smtp:Password" "YOUR_PASSWORD"
  dotnet user-secrets set "Smtp:From" "no-reply@tempoohub.example.com"
  ```

- Producción: usa variables de entorno. Las claves usan `:` en configuración, en variables de entorno para .NET se usan `__`.
  - `Smtp__Host`, `Smtp__Port`, `Smtp__User`, `Smtp__Password`, `Smtp__From`.

Probar localmente
- Si no quieres usar un SMTP real, instala `smtp4dev` o usa Mailtrap para capturar los correos de prueba.
- Construir y ejecutar:
  ```bash
  cd TempooHub.AuthServer
  dotnet build
  dotnet run
  ```
- Accede a la UI de administración (ej. `/Admin/Users`) y usa los botones:
  - "Recordar confirmar email": genera token de confirmación. Si SMTP está configurado, el servidor enviará el enlace completo; si no, verás en pantalla (TempData) los datos `userId=...&code=...` para construir manualmente la URL.
  - "Generar contraseña temporal": genera una contraseña segura y resetea la contraseña del usuario. Si SMTP está configurado, la contraseña temporal se enviará por correo; si no, la contraseña temporal aparecerá en la UI temporalmente (TempData).

Formato de enlace de confirmación
- El código que se muestra en `TempData` está codificado en Base64Url. Un ejemplo de enlace que el `SmtpEmailSender` genera automáticamente es:
  ```
  https://<tu-host>/Identity/Account/ConfirmEmail?userId=<userId>&code=<code>
  ```
- Si adaptas la ruta de confirmación, cambia la construcción del `callbackUrl` en `Pages/Admin/Users.cshtml.cs`.

Seguridad
- No incluyas credenciales en el repositorio.
- Para entornos CI/CD o producción, usa variables de entorno o el servicio de secretos de la plataforma (Azure Key Vault, AWS Secrets Manager, etc.).
- En desarrollo, `dotnet user-secrets` es la opción recomendada.

Alternativas y notas
- Puedes reemplazar `SmtpEmailSender` por un proveedor (SendGrid, Mailgun, etc.) implementando `IEmailSender` o usando los paquetes oficiales.
- Si quieres que implemente una integración con SendGrid (o similar) puedo añadirla como opción.

Contacto / seguimiento
- Si quieres, puedo:
  - Añadir instrucciones para `dotnet user-secrets` en README principal.
  - Implementar integración con SendGrid.
  - Mejorar el contenido del correo (plantillas HTML).


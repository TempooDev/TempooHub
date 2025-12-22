module.exports = {
  "/api": {
    target:
      process.env["services__tempoohub-auth__https__0"] ||
      process.env["services__tempoohub-auth__http__0"] ||
      "http://localhost:5000", // Fallback por si acaso
    secure: false, // En desarrollo suele dar menos problemas con certificados auto-firmados
    changeOrigin: true, // CLAVE para que el backend no rechace la petición por el Host header
    pathRewrite: {
      "^/api": "",
    },
    logLevel: "debug" // Útil para ver en la terminal si el proxy está redirigiendo bien
  },
};
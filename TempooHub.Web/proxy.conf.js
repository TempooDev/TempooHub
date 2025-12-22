module.exports = {
  "/api": {
    target:
      process.env["services__gateway__https__0"] ||
      process.env["services__gateway__http__0"] ||
      "http://localhost:7000", // Puerto por defecto de tu Gateway
    secure: false,
    changeOrigin: true,
    logLevel: "debug"
  },
};
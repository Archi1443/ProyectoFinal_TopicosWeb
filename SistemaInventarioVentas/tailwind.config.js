module.exports = {
  content: ["./Views/**/*.cshtml", "./wwwroot/js/*.js", "!./wwwroot/js/chart.umd.js"],
  theme: {
    extend: {
      colors: {
        marino: { 700: "#17306b", 800: "#10224f", 900: "#0b1838" },
        accion: { 500: "#2563eb", 600: "#1d4ed8", 700: "#1e40af" }
      },
      fontFamily: {
        sans: ["Segoe UI", "system-ui", "-apple-system", "Roboto", "Helvetica Neue", "Arial", "sans-serif"]
      }
    }
  },
  plugins: []
};

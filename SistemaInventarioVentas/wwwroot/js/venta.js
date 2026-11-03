document.addEventListener("DOMContentLoaded", () => {
    const formulario = document.getElementById("form-venta");
    const campoJson = document.getElementById("DetallesJson");
    const selectProducto = document.getElementById("producto");
    const campoCantidad = document.getElementById("cantidad");
    const filas = document.getElementById("carrito-filas");
    const tabla = document.getElementById("carrito");
    const vacio = document.getElementById("carrito-vacio");
    const textoTotal = document.getElementById("total");
    const errorProducto = document.getElementById("error-producto");
    const errorCarrito = document.getElementById("error-carrito");
    const errorCliente = document.getElementById("error-cliente");

    const dinero = new Intl.NumberFormat("es-MX", { style: "currency", currency: "MXN" });

    // Cada línea: { idProducto, nombre, precio, existencia, cantidad }
    let carrito = [];

    function infoProducto(id) {
        const opcion = selectProducto && selectProducto.querySelector(`option[value="${id}"]`);
        if (!opcion) return null;
        return {
            idProducto: id,
            nombre: opcion.dataset.nombre,
            precio: parseFloat(opcion.dataset.precio),
            existencia: parseInt(opcion.dataset.existencia, 10)
        };
    }

    // Si el servidor devolvió el formulario con errores, se reconstruye el carrito.
    try {
        JSON.parse(campoJson.value || "[]").forEach(linea => {
            const info = infoProducto(linea.idProducto);
            if (info) carrito.push({ ...info, cantidad: linea.cantidad });
        });
    } catch (e) {
        carrito = [];
    }

    function mostrarError(elemento, mensaje) {
        elemento.textContent = mensaje;
        elemento.classList.remove("hidden");
    }

    function dibujar() {
        filas.innerHTML = "";
        let total = 0;

        carrito.forEach((linea, indice) => {
            const subtotal = linea.precio * linea.cantidad;
            total += subtotal;

            const fila = document.createElement("tr");

            const celdaNombre = document.createElement("td");
            celdaNombre.className = "font-medium";
            celdaNombre.textContent = linea.nombre;

            const celdaPrecio = document.createElement("td");
            celdaPrecio.className = "text-right";
            celdaPrecio.textContent = dinero.format(linea.precio);

            const celdaCantidad = document.createElement("td");
            const entrada = document.createElement("input");
            entrada.type = "number";
            entrada.min = "1";
            entrada.max = String(linea.existencia);
            entrada.value = linea.cantidad;
            entrada.className = "campo py-1.5";
            entrada.setAttribute("aria-label", "Cantidad de " + linea.nombre);
            entrada.addEventListener("change", () => {
                let valor = parseInt(entrada.value, 10);
                if (isNaN(valor) || valor < 1) valor = 1;
                if (valor > linea.existencia) valor = linea.existencia;
                linea.cantidad = valor;
                dibujar();
            });
            celdaCantidad.appendChild(entrada);

            const celdaSubtotal = document.createElement("td");
            celdaSubtotal.className = "text-right font-semibold";
            celdaSubtotal.textContent = dinero.format(subtotal);

            const celdaQuitar = document.createElement("td");
            const quitar = document.createElement("button");
            quitar.type = "button";
            quitar.className = "btn-secundario btn-chico";
            quitar.textContent = "Quitar";
            quitar.setAttribute("aria-label", "Quitar " + linea.nombre);
            quitar.addEventListener("click", () => {
                carrito.splice(indice, 1);
                dibujar();
            });
            celdaQuitar.appendChild(quitar);

            fila.append(celdaNombre, celdaPrecio, celdaCantidad, celdaSubtotal, celdaQuitar);
            filas.appendChild(fila);
        });

        tabla.classList.toggle("hidden", carrito.length === 0);
        vacio.classList.toggle("hidden", carrito.length > 0);
        textoTotal.textContent = dinero.format(total);
        campoJson.value = JSON.stringify(carrito.map(l => ({ idProducto: l.idProducto, cantidad: l.cantidad })));
        if (carrito.length > 0) errorCarrito.classList.add("hidden");
    }

    document.getElementById("agregar")?.addEventListener("click", () => {
        errorProducto.classList.add("hidden");

        const id = parseInt(selectProducto.value, 10);
        const cantidad = parseInt(campoCantidad.value, 10);
        if (!id) return mostrarError(errorProducto, "Selecciona un producto.");
        if (isNaN(cantidad) || cantidad < 1) return mostrarError(errorProducto, "La cantidad debe ser mayor que cero.");

        const info = infoProducto(id);
        const existente = carrito.find(l => l.idProducto === id);
        const nuevaCantidad = (existente ? existente.cantidad : 0) + cantidad;
        if (nuevaCantidad > info.existencia) {
            return mostrarError(errorProducto, `Solo hay ${info.existencia} unidad(es) disponibles de ${info.nombre}.`);
        }

        if (existente) existente.cantidad = nuevaCantidad;
        else carrito.push({ ...info, cantidad });

        selectProducto.value = "";
        campoCantidad.value = 1;
        dibujar();
    });

    formulario.addEventListener("submit", e => {
        const faltaCliente = !formulario.elements["IdCliente"].value;
        errorCliente.classList.toggle("hidden", !faltaCliente);
        errorCarrito.classList.toggle("hidden", carrito.length > 0);
        if (faltaCliente || carrito.length === 0) e.preventDefault();
    });

    dibujar();
});

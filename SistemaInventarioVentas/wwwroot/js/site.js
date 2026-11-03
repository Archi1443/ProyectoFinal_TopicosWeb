document.addEventListener("DOMContentLoaded", () => {
    const menu = document.getElementById("menu-lateral");
    const fondo = document.getElementById("fondo-menu");
    const abrir = document.getElementById("abrir-menu");

    function alternarMenu(mostrar) {
        menu.classList.toggle("-translate-x-full", !mostrar);
        fondo.classList.toggle("hidden", !mostrar);
    }
    if (menu && abrir) {
        abrir.addEventListener("click", () => alternarMenu(true));
        fondo.addEventListener("click", () => alternarMenu(false));
        document.addEventListener("keydown", e => { if (e.key === "Escape") alternarMenu(false); });
    }

    document.querySelectorAll("[data-cerrar]").forEach(boton =>
        boton.addEventListener("click", () => boton.closest("[data-alerta]").remove()));

    document.querySelectorAll("[data-mostrar-password]").forEach(boton => {
        boton.addEventListener("click", () => {
            const campo = document.getElementById(boton.dataset.mostrarPassword);
            const visible = campo.type === "text";
            campo.type = visible ? "password" : "text";
            boton.setAttribute("aria-pressed", String(!visible));
            boton.querySelector("[data-ojo]").classList.toggle("hidden", !visible);
            boton.querySelector("[data-ojo-cerrado]").classList.toggle("hidden", visible);
            boton.setAttribute("aria-label", visible ? "Mostrar contraseña" : "Ocultar contraseña");
        });
    });

    // Confirmación para acciones destructivas: <form data-confirmar="mensaje">
    const dialogo = document.getElementById("dialogo-confirmar");
    let formularioPendiente = null;
    if (dialogo) {
        document.addEventListener("submit", e => {
            const mensaje = e.target.dataset.confirmar;
            if (!mensaje || e.defaultPrevented) return;
            e.preventDefault();
            e.stopImmediatePropagation();
            formularioPendiente = e.target;
            document.getElementById("dialogo-mensaje").textContent = mensaje;
            dialogo.showModal();
        }, true);
        document.getElementById("dialogo-cancelar").addEventListener("click", () => dialogo.close());
        document.getElementById("dialogo-aceptar").addEventListener("click", () => {
            dialogo.close();
            const formulario = formularioPendiente;
            formulario.removeAttribute("data-confirmar");
            formulario.requestSubmit();
        });
    }

    // Evita envíos dobles: al enviar un formulario válido se deshabilita el botón.
    document.addEventListener("submit", e => {
        if (e.defaultPrevented) return;
        const boton = e.target.querySelector("button[type=submit][data-cargando]");
        if (!boton) return;
        setTimeout(() => {
            boton.disabled = true;
            boton.dataset.textoOriginal = boton.innerHTML;
            boton.innerHTML = '<svg class="h-4 w-4 animate-spin" viewBox="0 0 24 24" fill="none"><circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" class="opacity-25"/><path d="M4 12a8 8 0 018-8" stroke="currentColor" stroke-width="4" class="opacity-75"/></svg> ' + (boton.dataset.cargando || "Guardando...");
        }, 0);
    });

    window.addEventListener("pageshow", e => {
        if (!e.persisted) return;
        document.querySelectorAll("button[data-texto-original]").forEach(b => {
            b.disabled = false;
            b.innerHTML = b.dataset.textoOriginal;
        });
    });
});

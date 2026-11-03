using System.Security.Claims;

namespace SistemaInventarioVentas.Services;

public static class ClaimsExtensions
{
    public static int ObtenerIdUsuario(this ClaimsPrincipal usuario)
    {
        return int.Parse(usuario.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}

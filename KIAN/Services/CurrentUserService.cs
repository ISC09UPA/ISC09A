namespace KIAN.Services;

/// <summary>
/// Modo un solo usuario: guarda el Id del usuario único de la aplicación.
/// Se inicializa al arrancar la API (ver Program.cs).
/// </summary>
public class CurrentUserService
{
    private Guid? _userId;

    public Guid UserId => _userId
        ?? throw new InvalidOperationException("El usuario por defecto no se ha inicializado.");

    public void Set(Guid id) => _userId = id;
}
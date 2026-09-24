namespace ReportU.Models;

/// <summary>Tipo de publicación según el alcance del MVP.</summary>
public enum PostType
{
    Incidencia = 0,
    Queja = 1,
    Discusion = 2
}

/// <summary>Categorías fijas del MVP. No hay administración dinámica.</summary>
public enum PostCategory
{
    Infraestructura = 0,
    Limpieza = 1,
    Seguridad = 2,
    Servicios = 3,
    Academico = 4,
    Tecnologia = 5,
    Cafeteria = 6,
    Estacionamiento = 7,
    Comunidad = 8,
    Otro = 9
}

/// <summary>Cómo se muestra el autor en una publicación concreta.</summary>
public enum IdentityMode
{
    /// <summary>Respeta la preferencia del perfil del autor.</summary>
    Default = 0,
    RealName = 1,
    Username = 2,
    Anonymous = 3
}

/// <summary>Preferencia pública del perfil del estudiante.</summary>
public enum DisplayNamePreference
{
    RealName = 0,
    Username = 1,
    Anonymous = 2
}

/// <summary>Orden del feed: Recientes (createdAt DESC) o Populares (supportCount DESC).</summary>
public enum FeedSort
{
    Recent = 0,
    Popular = 1
}

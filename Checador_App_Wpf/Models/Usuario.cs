namespace Checador_App_Wpf.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string ApellidoPaternoUsuario { get; set; }
        public string ApellidoMaternoUsuario { get; set; }
        public string CorreoUsuario { get; set; }
        public string UsuarioSistema { get; set; }
        public string ClaveUsuario { get; set; }
        public string FotoUsuarioURL { get; set; }
        public string FechaAltaUsuario { get; set; }
        public int IdArea { get; set; }
        public int IdEmpresa { get; set; }
        public string NombreArea { get; set; }

        public List<RolDtoUs> Roles { get; set; } = new();
        public List<BloqueDto> Bloques { get; set; } = new();
        public List<ModuloDtoUs> Modulos { get; set; } = new();

        public string PuestoUsuario { get; set; }
        public string PuestoSuperior { get; set; }
        public string PuestoSubordinado { get; set; }
    }
    public class RolDtoUs
    {
        public int IdRol { get; set; }
        public string NombreRol { get; set; }
    }

    public class BloqueDto
    {
        public int BloqueId { get; set; }
        public string BloqueNombre { get; set; }
    }

    public class ModuloDtoUs
    {
        public int IdBloque { get; set; }
        public int ModuloId { get; set; }
        public string ModuloNombre { get; set; }
        public string ModuloPath { get; set; }
        public string RolNombre { get; set; }
    }

}

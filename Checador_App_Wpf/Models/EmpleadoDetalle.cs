namespace Checador_App_Wpf.Models
{
    public class EmpleadoDetalle
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string ApellidoPaternoUsuario { get; set; }
        public string ApellidoMaternoUsuario { get; set; }
        public string CorreoUsuario { get; set; }
        public string UsuarioSistema { get; set; }
        public string ClaveUsuario { get; set; }
        public DateTime FechaAltaUsuario { get; set; }
        public string NombreRol { get; set; }
        public string NombreArea { get; set; }
        public string Puesto_NombrePuesto { get; set; }
        public string FotoUsuarioURL { get; set; }
        public string NombreDepartamento { get; set; }
        public List<Entrada> Entradas { get; set; }
        public List<Salida> Salidas { get; set; }
        public object Vehiculo { get; set; } // puedes definir una clase si sabes estructura
        public List<Horario> Horarios { get; set; }
        public List<Solicitud> Solicitudes { get; set; }
    }

    public class Entrada
    {
        public int IdEntrada { get; set; }
        public string TipoEntrada { get; set; }
        public DateTime? FechaSalida { get; set; }
    }

    public class Salida
    {
        // Define propiedades si tienes datos para esta clase
    }

    public class Horario
    {
        public int IdHorario { get; set; }
        public string Dia { get; set; }
    }

    public class Solicitud
    {
        public int IdSolicitud { get; set; }
        public string TipoSolicitud { get; set; }
    }
}

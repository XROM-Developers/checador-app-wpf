namespace Checador_App_Wpf.Models
{
    public class Empleado
    {
        public int idUsuario { get; set; }
        public string nombreUsuario { get; set; }
        public string correoUsuario { get; set; }
        public string puestoUsuario { get; set; }
        public string nombreRol { get; set; }
        public string nombreArea { get; set; }

        public List<HuellaDedo> huellas { get; set; } = new();
    }

    public class HuellaDedo
    {
        public string Nombre { get; set; }
        public bool Existe { get; set; }
    }
}

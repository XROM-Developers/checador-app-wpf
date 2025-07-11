using System.Collections.Generic;
using DPFP;
using Newtonsoft.Json;

namespace Checador_App_Wpf.Models
{
    public class EmpleadoHuella
    {
        public int idUsuario { get; set; }
        public string nombreCompleto { get; set; }
        public string puesto { get; set; }
        public string fotoPerfil { get; set; }
        public List<Huella> huellas { get; set; }
    }

    public class Huella
    {
        public int idUsuario { get; set; }
        public string archivoHuella { get; set; }

        [JsonIgnore] // Para evitar que esta propiedad se serialice cuando se mande al backend
        public Template? Template { get; set; }
    }
}

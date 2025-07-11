using Newtonsoft.Json;

public class Huella
{
    [JsonProperty("idUsuario")]
    public int IdUsuario { get; set; }

    [JsonProperty("idHuella")]
    public int IdHuella { get; set; }

    [JsonProperty("dedo")]
    public string Dedo { get; set; }
}

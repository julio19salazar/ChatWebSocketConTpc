using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatWebSocketConTpc.Models
{
    public class Datos
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Mensaje { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Mensaje MensajeTexto { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Mensaje> ListaMensajes { get; set; }
    }
}

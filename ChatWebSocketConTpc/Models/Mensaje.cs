using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChatWebSocketConTpc.Models
{
   public class Mensaje: INotifyPropertyChanged
    {
        private string mensaje_de_texto;

        public string MensajeDeTexto
        {
            get { return mensaje_de_texto; }
            set { mensaje_de_texto = value; OnPropertyChanged(); }
        }

        private string nombre;

        public string Nombre
        {
            get { return nombre; }
            set { nombre = value; OnPropertyChanged(); }
        }

        private string ip;

        public string IP
        {
            get { return ip; }
            set { ip = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

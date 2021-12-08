using ChatWebSocketConTpc.Models;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;


namespace ChatWebSocketConTpc.ViewModels
{
    public class ClienteViewModel : INotifyPropertyChanged
    {
        Dispatcher dispatcher;
        public string Direccion { get; }
        private string mensajedetextoaenviar;

        public string MensajeDeTextoEnviar
        {
            get { return mensajedetextoaenviar; }
            set { mensajedetextoaenviar = value; OnPropertyChanged(); }
        }
        private string error;

        public string Error
        {
            get { return error; }
            set { error = value; OnPropertyChanged(); }
        }


        private ObservableCollection<Mensaje> mensajes;

        public ObservableCollection<Mensaje> Mensajes
        {
            get { return mensajes; }
            set { mensajes = value; }
        }

        public ICommand EnviarCommand { get; set; }

        ClientWebSocket cliente = new ClientWebSocket();
        public ClienteViewModel(string direccion)
        {
            Direccion = direccion;
            mensajes = new ObservableCollection<Mensaje>();

            dispatcher = Dispatcher.CurrentDispatcher;

            EnviarCommand = new RelayCommand(EnviarMensaje);
            Conectar();
        }
        private void EnviarMensaje()
        {




            string nombre = Environment.MachineName;
            Error = "";


            //var anterior = mensajes.FirstOrDefault(x => x.MensajeDeTexto==mensajedetextoaenviar);
            Datos d;
            //if (anterior != null)
            //{
            //    anterior.Nombre = nombre;
            //    anterior.MensajeDeTexto = mensajedetextoaenviar;
            //    d = new Datos() { MensajeTexto = anterior };
            //}
            //else
            //{
            //    d = new Datos() { MensajeTexto = new Mensaje { MensajeDeTexto = MensajeDeTextoEnviar, Nombre = nombre } };
            //    Mensajes.Add(new Mensaje { MensajeDeTexto = MensajeDeTextoEnviar, Nombre = nombre });
            //}

            d = new Datos() { MensajeTexto = new Mensaje { MensajeDeTexto = MensajeDeTextoEnviar, Nombre = nombre } };
            Mensajes.Add(new Mensaje { MensajeDeTexto = MensajeDeTextoEnviar, Nombre = nombre });
            Enviar(d);


        }

        private async void Enviar(object o)
        {
            var json = JsonConvert.SerializeObject(o);
            byte[] buffer = Encoding.UTF8.GetBytes(json);

            await cliente.SendAsync(new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async void Conectar()
        {
            try
            {
                //await cliente.ConnectAsync(new Uri($"ws://{Direccion}:12500/websocket/"), CancellationToken.None);
                await cliente.ConnectAsync(new Uri($"{Direccion}"), CancellationToken.None);

                _ = Task.Run(Recibir);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        private async void Recibir()
        {
            while (cliente.State == WebSocketState.Open)
            {
                byte[] buffer = new byte[1024 * 10];
                var resultado = await cliente.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (resultado.MessageType == WebSocketMessageType.Close)
                {
                    await cliente.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
                else if (resultado.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buffer);

                    var datos = JsonConvert.DeserializeObject<Datos>(json);

                    if (datos.Mensaje != null) // Si Mensaje de Error del Servidor
                    {
                        Error = datos.Mensaje;
                    }
                    else if (datos.ListaMensajes != null) // Si es una Lista de Mensajes
                    {
                        dispatcher.Invoke(() =>
                        {
                            foreach (var l in datos.ListaMensajes)
                            {
                                Mensajes.Add(l);
                            }
                        }
                        );
                    }
                    else // Si Mensaje de Cliente
                    {
                        dispatcher.Invoke(() => Mensajes.Add(datos.MensajeTexto));
                    }

                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

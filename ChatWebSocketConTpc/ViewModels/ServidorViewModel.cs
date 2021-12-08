using ChatWebSocketConTpc.Models;
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
 public class ServidorViewModel : INotifyPropertyChanged
    {
        public bool Servidor { get; set; } = true;

        //public ObservableCollection<string> Mensajes { get; set; }
        private ObservableCollection<Mensaje> mensajes;
        HttpListener server;
        public ObservableCollection<Mensaje> Mensajes
        {
            get { return mensajes; }
            set { mensajes = value; OnPropertyChanged(); }
        }



        Dispatcher dispatcher;



       public ServidorViewModel()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            Mensajes = new ObservableCollection<Mensaje>();
            server = new HttpListener();
            server.Prefixes.Add("http://*:12500/websocket/");
            server.Start();
            Task.Run(RecibirPeticiones);
        }
        List<WebSocket> clientesconectado = new List<WebSocket>();
        Dictionary<WebSocket, string> iplist = new Dictionary<WebSocket, string>();
        private async void RecibirPeticiones()
        {
            try
            {
                while (server.IsListening)
                {
                    var context = server.GetContext();
                    if (context.Request.IsWebSocketRequest) //la petición se un websocket handshake
                    {
                        var cws = await context.AcceptWebSocketAsync(null); //acepta la conexión


                        clientesconectado.Add(cws.WebSocket);
                        iplist[cws.WebSocket] = context.Request.RemoteEndPoint.Address.ToString();


                        Enviar(cws.WebSocket, new Datos { ListaMensajes = mensajes.ToList() }); //enviar todos los mensajes al conectarse

                        _ = Task.Run(() =>
                        {
                            Recibir(cws.WebSocket);
                        });
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
            }
            catch
            {

            }
        }



        private async void Recibir(WebSocket webSocket)
        {
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    byte[] buffer = new byte[1024 * 10];

                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)//El cliente quiere cerrar la conexión
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    }
                    else
                    {
                        //Recibir un lugar desde el cliente, lo deserializo y lo proceso a la lista

                        var json = Encoding.UTF8.GetString(buffer);
                        Mensaje m = JsonConvert.DeserializeObject<Datos>(json).MensajeTexto;
                        m.IP = iplist[webSocket] ?? "";
                        
                           dispatcher.Invoke(()=> Mensajes.Add(new Mensaje { Nombre = m.Nombre, MensajeDeTexto = m.MensajeDeTexto, IP = m.IP }));
                        

                        //Enviar el mensaje a todos los demas clientes conectados
                        foreach (var ws in clientesconectado)
                        {
                            if (ws.State == WebSocketState.Open && ws != webSocket)
                            {
                                Enviar(ws, new Datos { MensajeTexto = m });
                            }
                        }


                    }

                }
            }
            catch
            {


            }
        }

        private async void Enviar(WebSocket webSocket, object o)
        {
            var json = JsonConvert.SerializeObject(o);
            byte[] buffer = Encoding.UTF8.GetBytes(json);

            await webSocket.SendAsync(new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            dispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }

    }
}

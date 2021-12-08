using ChatWebSocketConTpc.Views;
using ChatWebSocketConTpc.ViewModels;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using System.Net;
using ChatWebSocketConTpc.Models;

namespace ChatWebSocketConTpc.ViewModels
{
    public class SeleccionarViewModel : INotifyPropertyChanged
    {


        private bool rol;

        public bool Rol //ClienteoServidor
        {
            get { return rol; }
            set
            {
                rol = value;
                OnPropertyChanged();
            }
        }

        private string ip= "ws://127.0.0.1:12500/websocket";

        public string IP //IpServidorRemote
        {
            get { return ip; }
            set
            {
                ip = value;
                OnPropertyChanged();
            }
        }

        private UserControl control;

        public UserControl Control
        {
            get { return control; }
            set
            {
                control = value;
                OnPropertyChanged();
            }
        }

        private string error;

        public string Errores
        {
            get { return error; }
            set
            {
                error = value;
                OnPropertyChanged();
            }
        }


        public ICommand IniciarCommand { get; set; }
        EspacioView espacioView = new EspacioView();
        SeleccionarRolView seleccionarRol = new SeleccionarRolView();

        public SeleccionarViewModel()
        {
            IniciarCommand = new RelayCommand(Iniciar);
            seleccionarRol.DataContext = this;
            Control = seleccionarRol;
        }
        
        private void Iniciar()
        {
            Errores = "";

            if (Rol) //Rol true soy server
            {
                ServidorViewModel server = new ServidorViewModel();
                espacioView.DataContext = server;
                Control = espacioView;
            }
            else //soy cliente
            {

                // if (IPAddress.TryParse(IP, out IPAddress direccion))
                //{
                ClienteViewModel cliente = new ClienteViewModel(IP);
                espacioView.DataContext = cliente;
                Control = espacioView;
                //}
                //else
                //{
                Errores = "Debe escribir una dirección IP válida";
                //}
            
        }

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}

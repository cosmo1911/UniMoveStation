using UniMoveStation.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace UniMoveStation
{
    /// <summary>
    /// Interaction logic for ServerControl.xaml
    /// </summary>
    public partial class ServerControl : UserControl
    {
        private TCPServerNitoMove server;
        private bool listening = false;
        public List<CameraControl> cameraControls;

        public ServerControl()
        {
            InitializeComponent();

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        /// <summary>
        /// toggle server on and off
        /// </summary>
        /// <param name="enable"></param>
        private void toggleServer(bool enable)
        {
            if (enable)
            {
                if(server == null)
                {
                    server = new TCPServerNitoMove(this);
                    server.init(cameraControls);
                } 
                int port = int.Parse(textBox_port.Text);
                server.start(port);
                listening = true;
                button_toggleServer.Content = "Stop Listening";
            }
            else
            {
                if(server != null)
                {
                    server.stop();
                    server = null;
                    listening = false;
                    button_toggleServer.Content = "Start Listening";
                }
            }
        } //toggleServer

        private void button_startListening_Click(object sender, RoutedEventArgs e)
        {
            toggleServer(!listening);
        }

        private void button_disconnectClients_Click(object sender, RoutedEventArgs e)
        {
            if((bool) checkBox_abortivelyDisconnectClients.IsChecked)
            {
                server.closeAbortively();
            }
            else
            {
                server.disconnect();
            }
        }

        private void button_sendMessage_Click(object sender, RoutedEventArgs e)
        {
            //send complex message to all clients
            if((bool) checkBox_enableComplexMessage.IsChecked)
            {
                server.sendComplexMessageAll(textBox_message.Text);
            }
            //send normal message to all clients
            else
            {
                server.sendMessageAll(textBox_message.Text);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (server != null)
            {
                server.RefreshDisplay();
            }
        }

        private void Dispatcher_ShutdownStarted(object sender, System.EventArgs e)
        {
            //stop server
            if(listening)
            {
                toggleServer(false);
            }
        }
    } // ServerControl
} // namespace
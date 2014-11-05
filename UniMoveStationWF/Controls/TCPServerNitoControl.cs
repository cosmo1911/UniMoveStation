using System;
using System.Windows.Forms;

namespace UniMoveStation.IO
{
    public partial class TCPServerNitoControl : UserControl
    {
        public TCPServerNitoMove server;

        public TCPServerNitoControl()
        {
            InitializeComponent();
            server = new TCPServerNitoMove(this);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if(Visible && !Disposing)
            {
                server.RefreshDisplay();
            }
        }

        private void button_disconnectClients_Click(object sender, System.EventArgs e)
        {
            server.disconnect();
        }

        private void button_startListening_Click(object sender, System.EventArgs e)
        {
            server.start(int.Parse(textBox_port.Text));
        }

        private void button_abortivelyCloseClients_Click(object sender, System.EventArgs e)
        {
            server.closeAbortively();
        }

        private void button_stopListening_Click(object sender, System.EventArgs e)
        {
            server.stop();
        }

        private void button_sendMessage_Click(object sender, System.EventArgs e)
        {
            server.sendMessageAll(textBox_message.Text);
        }

        private void button_sendComplexMessage_Click(object sender, System.EventArgs e)
        {
            server.sendComplexMessageAll(textBox_message.Text);
        }

        private void TCPServerNitoControl_Load(object sender, EventArgs e)
        {
            server.RefreshDisplay();
        }
    }
}
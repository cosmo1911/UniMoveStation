namespace UniMoveStation.IO
{
    partial class TCPServerNitoControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox_server = new System.Windows.Forms.TextBox();
            this.button_sendComplexMessage = new System.Windows.Forms.Button();
            this.button_sendMessage = new System.Windows.Forms.Button();
            this.button_stopListening = new System.Windows.Forms.Button();
            this.button_abortivelyCloseClients = new System.Windows.Forms.Button();
            this.button_startListening = new System.Windows.Forms.Button();
            this.button_disconnectClients = new System.Windows.Forms.Button();
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_port = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBox_server
            // 
            this.textBox_server.BackColor = System.Drawing.SystemColors.Window;
            this.textBox_server.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_server.Location = new System.Drawing.Point(6, 91);
            this.textBox_server.Multiline = true;
            this.textBox_server.Name = "textBox_server";
            this.textBox_server.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_server.Size = new System.Drawing.Size(588, 86);
            this.textBox_server.TabIndex = 2;
            // 
            // button_sendComplexMessage
            // 
            this.button_sendComplexMessage.AutoSize = true;
            this.button_sendComplexMessage.Location = new System.Drawing.Point(141, 62);
            this.button_sendComplexMessage.Name = "button_sendComplexMessage";
            this.button_sendComplexMessage.Size = new System.Drawing.Size(131, 23);
            this.button_sendComplexMessage.TabIndex = 12;
            this.button_sendComplexMessage.Text = "Send Complex Message";
            this.button_sendComplexMessage.UseVisualStyleBackColor = true;
            this.button_sendComplexMessage.Click += new System.EventHandler(this.button_sendComplexMessage_Click);
            // 
            // button_sendMessage
            // 
            this.button_sendMessage.AutoSize = true;
            this.button_sendMessage.Location = new System.Drawing.Point(6, 61);
            this.button_sendMessage.Name = "button_sendMessage";
            this.button_sendMessage.Size = new System.Drawing.Size(126, 23);
            this.button_sendMessage.TabIndex = 11;
            this.button_sendMessage.Text = "Send Message";
            this.button_sendMessage.UseVisualStyleBackColor = true;
            this.button_sendMessage.Click += new System.EventHandler(this.button_sendMessage_Click);
            // 
            // button_stopListening
            // 
            this.button_stopListening.AutoSize = true;
            this.button_stopListening.Location = new System.Drawing.Point(278, 4);
            this.button_stopListening.Name = "button_stopListening";
            this.button_stopListening.Size = new System.Drawing.Size(131, 23);
            this.button_stopListening.TabIndex = 7;
            this.button_stopListening.Text = "Stop Listening";
            this.button_stopListening.UseVisualStyleBackColor = true;
            this.button_stopListening.Click += new System.EventHandler(this.button_stopListening_Click);
            // 
            // button_abortivelyCloseClients
            // 
            this.button_abortivelyCloseClients.AutoSize = true;
            this.button_abortivelyCloseClients.Location = new System.Drawing.Point(141, 33);
            this.button_abortivelyCloseClients.Name = "button_abortivelyCloseClients";
            this.button_abortivelyCloseClients.Size = new System.Drawing.Size(131, 23);
            this.button_abortivelyCloseClients.TabIndex = 10;
            this.button_abortivelyCloseClients.Text = "Abortively Close Clients";
            this.button_abortivelyCloseClients.UseVisualStyleBackColor = true;
            this.button_abortivelyCloseClients.Click += new System.EventHandler(this.button_abortivelyCloseClients_Click);
            // 
            // button_startListening
            // 
            this.button_startListening.AutoSize = true;
            this.button_startListening.Location = new System.Drawing.Point(141, 4);
            this.button_startListening.Name = "button_startListening";
            this.button_startListening.Size = new System.Drawing.Size(131, 23);
            this.button_startListening.TabIndex = 4;
            this.button_startListening.Text = "Start Listening";
            this.button_startListening.UseVisualStyleBackColor = true;
            this.button_startListening.Click += new System.EventHandler(this.button_startListening_Click);
            // 
            // button_disconnectClients
            // 
            this.button_disconnectClients.AutoSize = true;
            this.button_disconnectClients.Location = new System.Drawing.Point(6, 32);
            this.button_disconnectClients.Name = "button_disconnectClients";
            this.button_disconnectClients.Size = new System.Drawing.Size(126, 23);
            this.button_disconnectClients.TabIndex = 9;
            this.button_disconnectClients.Text = "Disconnect Clients";
            this.button_disconnectClients.UseVisualStyleBackColor = true;
            this.button_disconnectClients.Click += new System.EventHandler(this.button_disconnectClients_Click);
            // 
            // textBox_message
            // 
            this.textBox_message.Location = new System.Drawing.Point(278, 65);
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.Size = new System.Drawing.Size(131, 20);
            this.textBox_message.TabIndex = 8;
            this.textBox_message.Text = "test";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Port";
            // 
            // textBox_port
            // 
            this.textBox_port.Location = new System.Drawing.Point(35, 6);
            this.textBox_port.Name = "textBox_port";
            this.textBox_port.Size = new System.Drawing.Size(97, 20);
            this.textBox_port.TabIndex = 0;
            this.textBox_port.Text = "3000";
            // 
            // TCPServerNitoControl
            // 
            this.Controls.Add(this.textBox_server);
            this.Controls.Add(this.button_sendComplexMessage);
            this.Controls.Add(this.button_sendMessage);
            this.Controls.Add(this.button_stopListening);
            this.Controls.Add(this.button_abortivelyCloseClients);
            this.Controls.Add(this.button_startListening);
            this.Controls.Add(this.button_disconnectClients);
            this.Controls.Add(this.textBox_message);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_port);
            this.Location = new System.Drawing.Point(173, 116);
            this.Name = "TCPServerNitoControl";
            this.Size = new System.Drawing.Size(597, 180);
            this.Load += new System.EventHandler(this.TCPServerNitoControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBox_server;
        private System.Windows.Forms.Button button_sendComplexMessage;
        private System.Windows.Forms.Button button_sendMessage;
        private System.Windows.Forms.Button button_stopListening;
        private System.Windows.Forms.Button button_abortivelyCloseClients;
        private System.Windows.Forms.Button button_startListening;
        private System.Windows.Forms.Button button_disconnectClients;
        private System.Windows.Forms.TextBox textBox_message;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_port;
    }
}

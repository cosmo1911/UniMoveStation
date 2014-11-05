namespace UniMoveStation
{
    partial class MainWindow
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox_output = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cameraPanel = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button_initMove = new System.Windows.Forms.Button();
            this.button_initTracking = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tcpServerNitoControl = new UniMoveStation.IO.TCPServerNitoControl();
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_listeningState = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_connectionCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.button_stopTracking = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.StatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_output
            // 
            this.textBox_output.BackColor = System.Drawing.SystemColors.Window;
            this.textBox_output.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_output.Location = new System.Drawing.Point(6, 19);
            this.textBox_output.Multiline = true;
            this.textBox_output.Name = "textBox_output";
            this.textBox_output.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_output.Size = new System.Drawing.Size(607, 130);
            this.textBox_output.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cameraPanel);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1240, 437);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cameras";
            // 
            // cameraPanel
            // 
            this.cameraPanel.AutoScroll = true;
            this.cameraPanel.AutoSize = true;
            this.cameraPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cameraPanel.ColumnCount = 2;
            this.cameraPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.cameraPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.cameraPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cameraPanel.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddColumns;
            this.cameraPanel.Location = new System.Drawing.Point(3, 16);
            this.cameraPanel.Name = "cameraPanel";
            this.cameraPanel.RowCount = 2;
            this.cameraPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.cameraPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.cameraPanel.Size = new System.Drawing.Size(1234, 418);
            this.cameraPanel.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.textBox_output);
            this.groupBox2.Location = new System.Drawing.Point(630, 502);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(619, 155);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Console";
            // 
            // button_initMove
            // 
            this.button_initMove.Location = new System.Drawing.Point(636, 471);
            this.button_initMove.Name = "button_initMove";
            this.button_initMove.Size = new System.Drawing.Size(131, 23);
            this.button_initMove.TabIndex = 5;
            this.button_initMove.Text = "Init Move";
            this.button_initMove.UseVisualStyleBackColor = true;
            this.button_initMove.Click += new System.EventHandler(this.button_initMove_Click);
            // 
            // button_initTracking
            // 
            this.button_initTracking.Location = new System.Drawing.Point(773, 471);
            this.button_initTracking.Name = "button_initTracking";
            this.button_initTracking.Size = new System.Drawing.Size(131, 23);
            this.button_initTracking.TabIndex = 6;
            this.button_initTracking.Text = "Init Tracking";
            this.button_initTracking.UseVisualStyleBackColor = true;
            this.button_initTracking.Click += new System.EventHandler(this.button_initTrackers_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tcpServerNitoControl);
            this.groupBox3.Location = new System.Drawing.Point(15, 452);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(609, 205);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Controls";
            // 
            // tcpServerNitoControl
            // 
            this.tcpServerNitoControl.Location = new System.Drawing.Point(6, 19);
            this.tcpServerNitoControl.Name = "tcpServerNitoControl";
            this.tcpServerNitoControl.Size = new System.Drawing.Size(597, 180);
            this.tcpServerNitoControl.TabIndex = 0;
            // 
            // StatusStrip
            // 
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_listeningState,
            this.toolStripStatusLabel_connectionCount});
            this.StatusStrip.Location = new System.Drawing.Point(0, 660);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(1264, 22);
            this.StatusStrip.TabIndex = 8;
            this.StatusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_listeningState
            // 
            this.toolStripStatusLabel_listeningState.Name = "toolStripStatusLabel_listeningState";
            this.toolStripStatusLabel_listeningState.Size = new System.Drawing.Size(51, 17);
            this.toolStripStatusLabel_listeningState.Text = "Stopped";
            // 
            // toolStripStatusLabel_connectionCount
            // 
            this.toolStripStatusLabel_connectionCount.Name = "toolStripStatusLabel_connectionCount";
            this.toolStripStatusLabel_connectionCount.Size = new System.Drawing.Size(83, 17);
            this.toolStripStatusLabel_connectionCount.Text = "0 Connections";
            // 
            // button_stopTracking
            // 
            this.button_stopTracking.Enabled = false;
            this.button_stopTracking.Location = new System.Drawing.Point(910, 471);
            this.button_stopTracking.Name = "button_stopTracking";
            this.button_stopTracking.Size = new System.Drawing.Size(131, 23);
            this.button_stopTracking.TabIndex = 9;
            this.button_stopTracking.Text = "Stop Tracking";
            this.button_stopTracking.UseVisualStyleBackColor = true;
            this.button_stopTracking.Click += new System.EventHandler(this.button_stopTracking_Click);
            // 
            // MainWindow
            // 
            this.ClientSize = new System.Drawing.Size(1264, 682);
            this.Controls.Add(this.button_stopTracking);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.button_initTracking);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.button_initMove);
            this.Controls.Add(this.groupBox1);
            this.Name = "MainWindow";
            this.Text = "PS Move CLEye Server";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_output;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel cameraPanel;
        private System.Windows.Forms.Button button_initMove;
        private System.Windows.Forms.Button button_initTracking;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_listeningState;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_connectionCount;
        private IO.TCPServerNitoControl tcpServerNitoControl;
        private System.Windows.Forms.Button button_stopTracking;

    }
}


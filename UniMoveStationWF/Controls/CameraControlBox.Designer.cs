using System.Windows.Forms;
using System.Windows.Forms.Integration;
namespace UniMoveStation
{
    partial class CameraControlBox
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
            this.mainPanel = new System.Windows.Forms.TableLayoutPanel();
            this.imageHost = new System.Windows.Forms.Integration.ElementHost();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.subPanel = new System.Windows.Forms.TableLayoutPanel();
            this.checkBox_enableTracking = new System.Windows.Forms.CheckBox();
            this.checkBox_annotate = new System.Windows.Forms.CheckBox();
            this.textBox_X = new UniMoveStation.TextBoxWithLabel();
            this.textBox_Y = new UniMoveStation.TextBoxWithLabel();
            this.textBox_R = new UniMoveStation.TextBoxWithLabel();
            this.button_initCamera = new System.Windows.Forms.Button();
            this.mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.subPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.mainPanel.ColumnCount = 2;
            this.mainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 246F));
            this.mainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainPanel.Controls.Add(this.imageHost, 0, 0);
            this.mainPanel.Controls.Add(this.pictureBox, 0, 0);
            this.mainPanel.Controls.Add(this.subPanel, 1, 0);
            this.mainPanel.Location = new System.Drawing.Point(6, 14);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.RowCount = 1;
            this.mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 186F));
            this.mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.mainPanel.Size = new System.Drawing.Size(492, 186);
            this.mainPanel.TabIndex = 0;
            // 
            // imageHost
            // 
            this.imageHost.BackColor = System.Drawing.SystemColors.Desktop;
            this.imageHost.Location = new System.Drawing.Point(249, 3);
            this.imageHost.Name = "imageHost";
            this.imageHost.Size = new System.Drawing.Size(240, 180);
            this.imageHost.TabIndex = 0;
            this.imageHost.Text = "elementHost1";
            this.imageHost.Child = null;
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(3, 3);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(240, 180);
            this.pictureBox.TabIndex = 7;
            this.pictureBox.TabStop = false;
            // 
            // subPanel
            // 
            this.subPanel.ColumnCount = 2;
            this.subPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.subPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.subPanel.Controls.Add(this.checkBox_enableTracking, 0, 1);
            this.subPanel.Controls.Add(this.checkBox_annotate, 0, 2);
            this.subPanel.Controls.Add(this.textBox_X, 1, 0);
            this.subPanel.Controls.Add(this.textBox_Y, 1, 1);
            this.subPanel.Controls.Add(this.textBox_R, 1, 2);
            this.subPanel.Controls.Add(this.button_initCamera, 0, 0);
            this.subPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.subPanel.Location = new System.Drawing.Point(3, 189);
            this.subPanel.Name = "subPanel";
            this.subPanel.RowCount = 6;
            this.subPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66666F));
            this.subPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66666F));
            this.subPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66666F));
            this.subPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66666F));
            this.subPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66666F));
            this.subPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66666F));
            this.subPanel.Size = new System.Drawing.Size(240, 14);
            this.subPanel.TabIndex = 1;
            // 
            // checkBox_enableTracking
            // 
            this.checkBox_enableTracking.Location = new System.Drawing.Point(3, 5);
            this.checkBox_enableTracking.Name = "checkBox_enableTracking";
            this.checkBox_enableTracking.Size = new System.Drawing.Size(104, 20);
            this.checkBox_enableTracking.TabIndex = 0;
            this.checkBox_enableTracking.Text = "Enable Tracking";
            this.checkBox_enableTracking.Click += new System.EventHandler(this.checkBox_enableTracking_Click);
            // 
            // checkBox_annotate
            // 
            this.checkBox_annotate.Location = new System.Drawing.Point(3, 7);
            this.checkBox_annotate.Name = "checkBox_annotate";
            this.checkBox_annotate.Size = new System.Drawing.Size(104, 20);
            this.checkBox_annotate.TabIndex = 1;
            this.checkBox_annotate.Text = "Annotate";
            // 
            // textBox_X
            // 
            this.textBox_X.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_X.LabelText = "X";
            this.textBox_X.Location = new System.Drawing.Point(122, 3);
            this.textBox_X.Name = "textBox_X";
            this.textBox_X.ReadOnly = true;
            this.textBox_X.Size = new System.Drawing.Size(115, 20);
            this.textBox_X.TabIndex = 6;
            this.textBox_X.Text = "0";
            // 
            // textBox_Y
            // 
            this.textBox_Y.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_Y.LabelText = "Y";
            this.textBox_Y.Location = new System.Drawing.Point(122, 5);
            this.textBox_Y.Name = "textBox_Y";
            this.textBox_Y.ReadOnly = true;
            this.textBox_Y.Size = new System.Drawing.Size(115, 20);
            this.textBox_Y.TabIndex = 7;
            this.textBox_Y.Text = "0";
            // 
            // textBox_R
            // 
            this.textBox_R.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_R.LabelText = "R";
            this.textBox_R.Location = new System.Drawing.Point(122, 7);
            this.textBox_R.Name = "textBox_R";
            this.textBox_R.ReadOnly = true;
            this.textBox_R.Size = new System.Drawing.Size(115, 20);
            this.textBox_R.TabIndex = 8;
            this.textBox_R.Text = "0";
            // 
            // button_initCamera
            // 
            this.button_initCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button_initCamera.Location = new System.Drawing.Point(3, 3);
            this.button_initCamera.Name = "button_initCamera";
            this.button_initCamera.Size = new System.Drawing.Size(113, 1);
            this.button_initCamera.TabIndex = 9;
            this.button_initCamera.Text = "Init Camera";
            this.button_initCamera.UseVisualStyleBackColor = true;
            this.button_initCamera.Click += new System.EventHandler(this.button_initCamera_Click);
            // 
            // CameraControlBox
            // 
            this.Controls.Add(this.mainPanel);
            this.Location = new System.Drawing.Point(227, 120);
            this.Name = "CameraControlBox";
            this.Size = new System.Drawing.Size(504, 206);
            this.mainPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.subPanel.ResumeLayout(false);
            this.subPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private TableLayoutPanel mainPanel;
        private TableLayoutPanel subPanel;
        private PictureBox pictureBox;
        private Button button_initCamera;
        private CheckBox checkBox_enableTracking;
        private CheckBox checkBox_annotate;
        private TextBoxWithLabel textBox_X;
        private TextBoxWithLabel textBox_Y;
        private TextBoxWithLabel textBox_R;
        private ElementHost imageHost;
    }
}

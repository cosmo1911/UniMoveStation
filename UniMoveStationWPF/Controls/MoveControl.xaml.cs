using ColorWheel.Core;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using UniMove;
using UnityEngine;
using SharpMove;
using System.Collections.Generic;
using System.Windows;
using System.Threading;
using System.Windows.Data;

namespace UniMoveStation
{
    /// <summary>
    /// Interaction logic for MoveControl.xaml
    /// </summary>
    public partial class MoveControl : UserControl
    {
        private SharpMoveController move;
        private int id;
        private Palette pallette;
        private List<CheckBox> checkBoxes;
        private List<PSMoveButton> buttons;

        //public MoveControl()
        //{
        //    InitializeComponent();
        //}

        public MoveControl(int id)
        {
            InitializeComponent();
            DataContext = this;
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
            this.id = id;

            initColorWheel();

            buttons = new List<PSMoveButton>()
            {
                PSMoveButton.Circle, PSMoveButton.Cross, PSMoveButton.Triangle, PSMoveButton.Square,
                PSMoveButton.Move, PSMoveButton.PS, PSMoveButton.Start, PSMoveButton.Select, 
                PSMoveButton.Trigger
            };

            checkBoxes = new List<CheckBox>()
            {
                checkBox_circle, checkBox_cross, checkBox_triangle, checkBox_square,
                checkBox_move, checkBox_ps, checkBox_start, checkBox_select
            };
        }

        private void initColorWheel()
        {
            pallette = Palette.Create(new RGBColorWheel(), System.Windows.Media.Colors.Blue, PaletteSchemaType.Analogous, 1);
            colorWheel.Palette = pallette;

            colorWheel.ColorsUpdated += colorWheel_ColorsUpdated;
        }

        private Color getPalletteColor(int index)
        {
            System.Windows.Media.Color color = colorWheel.Palette.Colors[index].RgbColor;
            
            float r = ((byte)color.R) / 51f;
            float g = ((byte)color.G) / 51f;
            float b = ((byte)color.B) / 51f;

            return new Color(r, g, b);
        }

        private void refreshDisplay()
        {
            foreach (PSMoveButton button in buttons)
            {
                if (button == PSMoveButton.Trigger)
                {
                    progressBar_trigger.Value = move.Trigger * 255;
                }
                else
                {
                    checkBoxes[buttons.IndexOf(button)].IsChecked = move.GetButton(button);
                }
            }

            textBox_statusInfo.Text = move.StatusInfo();
        }

        private void connect(int id)
        {
            move = new SharpMoveController();
            PSMoveConnectStatus status = move.Init(id);
            if (status == PSMoveConnectStatus.OK)
            {

                move.SetLED(getPalletteColor(0));

                move.initBackgroundWorker();

                move.OnControllerDisconnected += move_OnControllerDisconnected;
                move.OnControllerUpdated += move_OnControllerUpdated;
                button_toggleConnection.Content = "Disconnect";
            }
            else
            {
                textBox_statusInfo.Text = string.Format("Connect Status: {0}", status.ToString());
                move = null;
            }
        }

        private void disconnect()
        {
            if (move != null)
            {
                move.cancelBackgroundWorker();
                button_toggleConnection.Content = "Connect";
                move.Disconnect();
                move = null;
            }
        }

        #region [ Misc ]
        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            disconnect();
        }

        private void move_OnControllerUpdated(object sender, EventArgs e)
        {
            refreshDisplay();
        }

        private void move_OnControllerDisconnected(object sender, EventArgs e)
        {
            disconnect();
        }

        private void colorWheel_ColorsUpdated(object sender, EventArgs e)
        {
            if(move != null)
            {
                move.SetLED(getPalletteColor(0));
            }
        }

        private void button_toggleConnection_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (move == null)
            {
                connect(id);
            }
            else
            {
                disconnect();
            }
        }
        #endregion [ Misc ]
    } // MoveControl
} // namespace
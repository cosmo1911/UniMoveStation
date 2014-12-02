using System;
using System.Windows;
using System.Windows.Controls;
using UniMoveStation.ViewModel;

namespace UniMoveStation.View
{
    /// <summary>
    /// Description for CameraView.
    /// </summary>
    public partial class SingleCameraView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the CameraView class.
        /// </summary>
        public SingleCameraView()
        {
            InitializeComponent();
            DataContextChanged += SingleCameraView_DataContextChanged;
            Loaded += (s, e) =>
                {
                    Window.GetWindow(this).Closing +=
                        (s1, e1) =>
                        {
                            if(DataContext != null)
                            {
                                ((SingleCameraViewModel)DataContext).Cleanup();
                            }
                        };
                };
        }

        void SingleCameraView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null && e.OldValue is SingleCameraViewModel) 
                Console.WriteLine("old " + ((SingleCameraViewModel)e.OldValue).Camera.Name);
            if(e.NewValue != null && e.NewValue is SingleCameraViewModel) 
                Console.WriteLine("new " + ((SingleCameraViewModel) e.NewValue).Camera.Name);
        }
    }
}
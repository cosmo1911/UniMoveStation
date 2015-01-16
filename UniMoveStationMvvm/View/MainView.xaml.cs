using MahApps.Metro.Controls;
using System;
using System.Windows;
using UniMoveStation.ViewModel;

namespace UniMoveStation.View
{
    /// <summary>
    /// Description for MvvmView1.
    /// </summary>
    public partial class MainView : MetroWindow
    {
        /// <summary>
        /// Initializes a new instance of the MvvmView1 class.
        /// </summary>
        public MainView()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                Window.GetWindow(this).Closing +=
                    (s1, e1) =>
                    {
                        ViewModelLocator.Cleanup();
                    };
            };
        }
    }
}
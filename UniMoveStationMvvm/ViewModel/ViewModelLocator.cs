/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:UniMoveStation"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using System.Windows;
using UniMoveStation.ViewModel.Flyout;

namespace UniMoveStation.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        public static ViewModelLocator Instance
        {
            get { return Application.Current.Resources["ViewModelLocator"] as ViewModelLocator; }
        }

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<SettingsViewModel>(true);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<NavigationViewModel>();
            SimpleIoc.Default.Register<ServerViewModel>();
            SimpleIoc.Default.Register<CamerasViewModel>();
            SimpleIoc.Default.Register<AddMotionControllerViewModel>();
            SimpleIoc.Default.Register<AddCameraViewModel>();
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        public CamerasViewModel Cameras
        {
            get { return ServiceLocator.Current.GetInstance<CamerasViewModel>(); }
        }

        public ServerViewModel Server
        {
            get { return ServiceLocator.Current.GetInstance<ServerViewModel>(); }
        }

        public NavigationViewModel Navigation
        {
            get { return ServiceLocator.Current.GetInstance<NavigationViewModel>(); }
        }

        public AddMotionControllerViewModel AddMotionController
        {
            get { return ServiceLocator.Current.GetInstance<AddMotionControllerViewModel>(); }
        }

        public AddCameraViewModel AddCamera
        {
            get { return ServiceLocator.Current.GetInstance<AddCameraViewModel>(); }
        }

        public SettingsViewModel Settings
        {
            get { return ServiceLocator.Current.GetInstance<SettingsViewModel>(); }
        }
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
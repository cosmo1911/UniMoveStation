using UniMoveStation.Representation.ViewModel.Flyout;

namespace UniMoveStation.Representation.MessengerMessage
{
    public class ToggleFlyoutMessage
    {
        public ToggleFlyoutMessage(FlyoutBaseViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public FlyoutBaseViewModel ViewModel
        {
            get;
            private set;
        }
    }
}

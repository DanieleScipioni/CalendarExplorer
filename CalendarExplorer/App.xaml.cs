using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace CalendarExplorer
{
    sealed partial class App
    {
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (!(Window.Current.Content is Samples.Calendar.CalendarExplorer))
            {
                Window.Current.Content = new Samples.Calendar.CalendarExplorer();
            }

            if (!e.PrelaunchActivated)
            {
                Window.Current.Activate();
            }
        }

        private static void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}

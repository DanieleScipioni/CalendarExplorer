using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CalendarExplorer.Samples.Calendar
{
    public sealed partial class CalendarExplorer
    {
        private AppointmentStore _appointmentStore;

        private IReadOnlyList<AppointmentCalendar> _appointmentCalendarAdapters;
        private AppointmentCalendar _appointmentCalendar;
        private ObservableCollection<Appointment> _appointments;

        public CalendarExplorer()
        {
            InitializeComponent();

            Loaded += async (sender, args) =>
            {
                await RequestStore();
                await LoadCalendars();
            };
        }

        private async Task RequestStore()
        {
            _appointmentStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadWrite);
        }

        private async Task LoadCalendars()
        {
            _appointmentCalendarAdapters = await _appointmentStore.FindAppointmentCalendarsAsync(FindAppointmentCalendarsOptions.IncludeHidden);

            CalendarListView.ItemsSource = _appointmentCalendarAdapters;
        }

        private async void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (CalendarListView.Visibility == Visibility.Visible)
            {
                await LoadCalendars();
            }
            else
            {
                await LoadAppointments();
            }
        }

        private async void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (AppointmentSection.Visibility == Visibility.Visible)
            {
                await AddAppointment();
            }
        }

        private async void ListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var appointmentCalendar = (AppointmentCalendar) e.ClickedItem;
            CalendarListView.Visibility = Visibility.Collapsed;
            AppointmentSection.Visibility = Visibility.Visible;
            BackButton.Visibility = Visibility.Visible;
            _appointmentCalendar = appointmentCalendar;
            AppointmentListView.Header = _appointmentCalendar.DisplayName;
            await LoadAppointments();
        }

        private async Task LoadAppointments()
        {
            AppointmentListView.ItemsSource = _appointments = new ObservableCollection<Appointment>(
                await _appointmentCalendar.FindAppointmentsAsync(
                    DateTimeOffset.Now.Date.Add(TimeSpan.FromDays(-8)),
                    TimeSpan.FromDays(16),
                    new FindAppointmentsOptions
                    {
                        IncludeHidden = true
                    }));
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            CalendarListView.Visibility = Visibility.Visible;
            AppointmentSection.Visibility = Visibility.Collapsed;
            BackButton.Visibility = Visibility.Collapsed;
            AppointmentListView.ItemsSource = null;
            _appointmentCalendar = null;
            AppointmentListView.Header = string.Empty;
        }

        private async Task AddAppointment()
        {
            var now = DateTimeOffset.Now;

            var appointment = new Appointment
            {
                Subject = "Appointment",
                StartTime = now,
                Duration = TimeSpan.FromMinutes(30),
            };

            await _appointmentCalendar.SaveAppointmentAsync(appointment);
            if (appointment.LocalId != string.Empty) _appointments.Add(appointment);

            var appointmentRecurrence = new AppointmentRecurrence {Unit = AppointmentRecurrenceUnit.Weekly, Interval = 1, DaysOfWeek = (AppointmentDaysOfWeek) (now.DayOfWeek + 1)};
            appointment = new Appointment
            {
                Subject = "Appointment with recurrence",
                StartTime = now.AddHours(1),
                Duration = TimeSpan.FromMinutes(30),
                Recurrence = appointmentRecurrence
            };
            
            await _appointmentCalendar.SaveAppointmentAsync(appointment);
            if (appointment.LocalId != string.Empty) _appointments.Add(appointment);
        }

        private async void DeleteAppointmentButton_OnClick(object sender, RoutedEventArgs e)
        {
            var frameworkElement = (FrameworkElement) sender;
            var appointment = (Appointment) frameworkElement.DataContext;
            await _appointmentCalendar.DeleteAppointmentAsync(appointment.LocalId);
            _appointments.Remove(appointment);
        }
    }
}
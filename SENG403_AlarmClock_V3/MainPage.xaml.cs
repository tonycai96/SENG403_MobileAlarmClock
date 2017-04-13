using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SENG403_AlarmClock_V3
{
    /// <summary>
    /// The point of access to the mobile alarm app. Because we haven't figured out how to do page navigation well (in UWP),
    /// this class contains the main page of the application (where the user can view the time, view the list of set alarms,
    /// modify existing alarms, and create new alarms), the edit alarm page, and the alarm notification page (which is the
    /// page that pops up when an alarm goes off). 
    /// 
    /// This class is also responsible for handling persistent storage (loading and saving alarms to local storage), and
    /// handling alarm notification when the first alarm goes off (subsequent alarm notifications are handled by the 
    /// AlarmUserControl class because those notifications would only change the AlarmUserControlUI).
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static DateTime currentTime;
        public static String[] AlarmSoundsList= new String[] {"Alarm 1", "Alarm 2", "Alarm 3"};
        public DispatcherTimer dispatcherTimer = new DispatcherTimer();

        /// <summary>
        /// The name of the file which stores alarm information.
        /// </summary>
        public static string ALARMS_FILE = "alarms.txt";

        public MainPage()
        {
            InitializeComponent();
            alarmtoneSelector.ItemsSource = AlarmSoundsList;
            
            currentTime = DateTime.Now;
            updateTimeDisplay(currentTime);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            dispatcherTimer.Start();
        }

        /// <summary>
        /// A helper method for updating the current date and time displayed displayed in the main page GUI.
        /// </summary>
        /// <param name="time"> Current time </param>
        private void updateTimeDisplay(DateTime time)
        {
            HourText.Text = time.ToString("hh:mm");
            MinuteText.Text = time.ToString(":ss");
            AMPMText.Text = time.ToString("tt");
            DayDateText.Text = time.ToString("dddd, MMMM dd, yyyy");

            AlarmNotificationWindowTime.Text = time.ToString("hh:mm:ss tt");
            AlarmNotificationWindowDate.Text = time.ToString("dddd, MMMM dd, yyyy");
        }

        /// <summary>
        /// Updates the system every second.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherTimer_Tick(object sender, object e)
        {
            currentTime = DateTime.Now;
            updateTimeDisplay(currentTime);

            foreach (AlarmUserControl u in AlarmList_Panel.Children)
            {
                u.requestAlarmWithCheck(currentTime);
            }
        }

        /// Persistent Storage

        /// <summary>
        /// This method is called whenever the user navigates to this page so that the most up-to-date
        /// list of alarms will be loaded from persistent storage and displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void pageLoaded(Object sender, RoutedEventArgs e)
        {
            await loadAlarmsFromJSON();
            currentTime = DateTime.Now;
            foreach (AlarmUserControl u in AlarmList_Panel.Children)
            {
                if (u.alarm.currentState == AlarmState.FIRST_TO_GO_OFF)
                {
                    openAlarmNotificationWindow(u.alarm.label);
                    AlarmsManager.IS_ALARM_NOTIFICATION_OPEN = true;
                }
            }
        }

        private async void pageUnloaded(object sender, RoutedEventArgs e)
        {
            await saveAlarmsToJSON(getAlarms());
        }

        /// <summary>
        /// Save the current list of alarms into the ALARMS_FILE.
        /// </summary>
        /// <param name="alarms"> List of alarms to be saved to ALARMS_FILE. </param>
        /// <returns></returns>
        public async Task saveAlarmsToJSON(List<Alarm> alarms)
        {
            var serializer = new DataContractJsonSerializer(typeof(List<Alarm>));
            using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(
                          ALARMS_FILE,
                          CreationCollisionOption.ReplaceExisting))
            {
                serializer.WriteObject(stream, alarms);
            }
        }

        /// <summary>
        /// Load Alarm data from ALARM_FILE and uses that data to intialize the StackPanel that displays the alarms.
        /// </summary>
        /// <returns></returns>
        private async Task loadAlarmsFromJSON()
        {
            List<Alarm> alarms;
            var jsonSerializer = new DataContractJsonSerializer(typeof(List<Alarm>));
            try
            {
                var myStream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(ALARMS_FILE);

                alarms = (List<Alarm>)jsonSerializer.ReadObject(myStream);

                foreach (var a in alarms)
                {
                    AlarmUserControl alarmDisplay = new AlarmUserControl(this, a);
                    alarmDisplay.updateDisplay();
                    AlarmList_Panel.Children.Add(alarmDisplay);
                }
            }
            catch (Exception)
            {
                //Do nothing, file doesn't exist
            }
        }

        /// <summary>
        /// Gets the list of currently set alarms.
        /// </summary>
        /// <returns>The list of alarms currently in the StackPanel which displays the list of alarms set by the user. </returns>
        public List<Alarm> getAlarms()
        {
            List<Alarm> ret = new List<Alarm>();
            foreach (AlarmUserControl u in AlarmList_Panel.Children)
            {
                if (u.alarm.initialized)
                    ret.Add(u.alarm);
            }
            return ret;
        }

        /// General user input handling
        private void AddAlarmButton_Click(object sender, RoutedEventArgs e)
        {
            AlarmUserControl alarmControl = new AlarmUserControl(this, new Alarm(AlarmsManager.SNOOZE_TIME));
            AlarmList_Panel.Children.Add(alarmControl);
        }

        private async void AlarmsSettingsButton_Clicked(object sender, RoutedEventArgs e)
        {
            await saveAlarmsToJSON(getAlarms());
            dispatcherTimer.Tick -= DispatcherTimer_Tick;
            Frame.Navigate(typeof(SettingsPage));
        }

        internal void openEditPage()
        {
            EditAlarmPage.Visibility = Visibility.Visible;
        }

        //Edit Alarm Window
        private void DoneEditAlarmButtonClicked(object sender, RoutedEventArgs e)
        {
            foreach (AlarmUserControl u in AlarmList_Panel.Children)
            {
                if (u.currentState == State.EDIT)
                {
                    u.currentState = State.IDLE;
                    TimeSpan ts = timePicker.Time;
                    string label = AlarmLabelTextbox.Text;
                    if (!repeatCheckbox.IsChecked == true)
                    {
                        u.setOneTimeAlarm(DateTime.Today.Add(ts), label);
                    }
                    else if (Monday.IsChecked == true)
                    {
                        u.setWeeklyAlarm(DayOfWeek.Monday, ts, label);
                    }
                    else if (Tuesday.IsChecked == true)
                    {
                        u.setWeeklyAlarm(DayOfWeek.Tuesday, ts, label);
                    }
                    else if (Wednesday.IsChecked == true)
                    {
                        u.setWeeklyAlarm(DayOfWeek.Wednesday, ts, label);
                    }
                    else if (Thursday.IsChecked == true)
                    {
                        u.setWeeklyAlarm(DayOfWeek.Thursday, ts, label);
                    }
                    else if (Friday.IsChecked == true)
                    {
                        u.setWeeklyAlarm(DayOfWeek.Friday, ts, label);
                    }
                    else if (Saturday.IsChecked == true)
                    {
                        u.setWeeklyAlarm(DayOfWeek.Saturday, ts, label);
                    }
                    else if (Sunday.IsChecked == true)
                    {
                        u.setWeeklyAlarm(DayOfWeek.Sunday, ts, label);
                    }
                    else if (Daily.IsChecked == true)
                    {
                        u.setDailyAlarm(ts, label);
                    }
                }
            }
            EditAlarmPage.Visibility = Visibility.Collapsed;
        }

        private void CancelEditAlarmButtonClicked(object sender, RoutedEventArgs e)
        {
            foreach (AlarmUserControl u in AlarmList_Panel.Children)
            {
                if (u.currentState == State.EDIT) u.currentState = State.IDLE;
            }
            EditAlarmPage.Visibility = Visibility.Collapsed;
        }

        private void RepeatingAlarmCheckboxChecked(object sender, RoutedEventArgs e)
        {
            datePicker.Visibility = Visibility.Collapsed;
            repeatedAlarms.Visibility = Visibility.Visible;
        }

        private void RepeatingAlarmCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
            datePicker.Visibility = Visibility.Visible;
            repeatedAlarms.Visibility = Visibility.Collapsed;
        }

        // Alarm Notification Window
        internal void openAlarmNotificationWindow(string text)
        {
            AlarmLabel.Text = text;
            AlarmNotifyMessage.Text = "An alarm has gone off at " + currentTime.ToString("hh:mm:ss tt");
            AlarmNotification.Visibility = Visibility.Visible;
        }

        private void DismissButtonClick(object sender, RoutedEventArgs e)
        {
            foreach (AlarmUserControl u in AlarmList_Panel.Children)
            {
                if (u.alarm.currentState.Equals(AlarmState.FIRST_TO_GO_OFF))
                    u.alarm.updateAlarmTime();
            }
            AlarmNotification.Visibility = Visibility.Collapsed;
            AlarmsManager.IS_ALARM_NOTIFICATION_OPEN = false;
        }

        private void SnoozeButtonClick(object sender, RoutedEventArgs e)
        {
            foreach (AlarmUserControl u in AlarmList_Panel.Children)
                if (u.alarm.currentState == AlarmState.FIRST_TO_GO_OFF)
                    u.alarm.snooze();
            AlarmNotification.Visibility = Visibility.Collapsed;
            AlarmsManager.IS_ALARM_NOTIFICATION_OPEN = false;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}

using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace SENG403_AlarmClock_V3
{
    /// <summary>
    /// State of the AlarmUserControl
    /// TODO: get rid of these states
    /// </summary>
    public enum State { IDLE, EDIT, NOTIFY };

    /// <summary>
    /// This class handles the logic associated with its alarm (such as checking whether that alarm should go off and
    /// whether that alarm should display a notification) as well as event handling and data updating for its alarm.
    /// </summary>
    public sealed partial class AlarmUserControl : UserControl
    {
        /// <summary>
        /// The page which is the parent page of AlarmUserControl. This is required so that the AlarmUserControl can
        /// request UI change on the main page.
        /// </summary>
        private MainPage mainPage;

        public Alarm alarm { get; set; }

        public State currentState = State.IDLE;

        public AlarmUserControl(MainPage page, Alarm alarm)
        {
            this.InitializeComponent();
            mainPage = page;
            this.alarm = alarm;
        }

        private void EnableDisableAlarm_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!alarm.initialized) return;
            if (EnableDisableAlarm_Button.Content.Equals("Enable"))
                enable();
            else
                disable();
        }

        private void EditAlarm_Click(object sender, RoutedEventArgs e)
        {
            currentState = State.EDIT;
            mainPage.openEditPage();
        }

        private void DeleteAlarm_Click(object sender, RoutedEventArgs e)
        {
            StackPanel parent = (StackPanel)this.Parent;
            foreach(AlarmUserControl alarm in parent.Children)
            {
                if(this.Equals(alarm))
                {
                    parent.Children.Remove(this);
                    break;
                }
            }
        }

        /// <summary>
        /// Update the display of the alarm label to match alarm info.
        /// </summary>
        internal void updateDisplay()
        {
            if (!alarm.initialized)
            {
                AlarmTypeLabel.Text = "";
                AlarmTimeLabel.Text = "Alarm Not Set";
                return;
            }
            if (alarm.repeatIntervalDays == 7)
            {
                if (alarm.defaultNotificationTime.DayOfWeek == DayOfWeek.Monday) AlarmTypeLabel.Text = "Monday";
                if (alarm.defaultNotificationTime.DayOfWeek == DayOfWeek.Tuesday) AlarmTypeLabel.Text = "Tuesday";
                if (alarm.defaultNotificationTime.DayOfWeek == DayOfWeek.Wednesday) AlarmTypeLabel.Text = "Wednesday";
                if (alarm.defaultNotificationTime.DayOfWeek == DayOfWeek.Thursday) AlarmTypeLabel.Text = "Thursday";
                if (alarm.defaultNotificationTime.DayOfWeek == DayOfWeek.Friday) AlarmTypeLabel.Text = "Friday";
                if (alarm.defaultNotificationTime.DayOfWeek == DayOfWeek.Saturday) AlarmTypeLabel.Text = "Saturday";
                if (alarm.defaultNotificationTime.DayOfWeek == DayOfWeek.Sunday) AlarmTypeLabel.Text = "Sunday";
                AlarmTimeLabel.Text = alarm.defaultNotificationTime.TimeOfDay.ToString();
            }
            else if (alarm.repeatIntervalDays == 1)
            {
                AlarmTypeLabel.Text = "Daily";
                AlarmTimeLabel.Text = alarm.defaultNotificationTime.TimeOfDay.ToString();
            }
            else if (alarm.repeatIntervalDays == -1)
            {
                AlarmTypeLabel.Text = "No Repeat";
                AlarmTimeLabel.Text = alarm.defaultNotificationTime.ToString();
            }
            AlarmLabel.Text = alarm.label;
        }

        /// <summary>
        /// Helper method for setting an alarm that only goes off once.
        /// </summary>
        internal void setOneTimeAlarm(DateTime alarmTime, string label)
        {
            alarm.setOneTimeAlarm(alarmTime);
            if (label.Equals("")) alarm.label = "Unlabelled alarm";
            else alarm.label = label;
            updateDisplay();
        }

        /// <summary>
        /// Helper method for setting an alarm that only goes off weekly.
        /// </summary>
        internal void setWeeklyAlarm(DayOfWeek alarmDay, TimeSpan alarmTime, string label)
        {
            alarm.setWeeklyAlarm(alarmDay, alarmTime);
            if (label.Equals("")) alarm.label = "Unlabelled alarm";
            else alarm.label = label;
            updateDisplay();
        }

        /// <summary>
        /// Helper method for setting an alarm that only goes off daily.
        /// </summary>
        internal void setDailyAlarm(TimeSpan ts, string label)
        {
            alarm.setDailyAlarm(ts);
            if (label.Equals("")) alarm.label = "Unlabelled alarm";
            else alarm.label = label;
            updateDisplay();
        }

        /// <summary>
        /// This method handles checking whether an alarm should go off and updating the UI if the alarm should go off.
        /// It listens to the timer belonging to the MainPage class.
        /// </summary>
        /// <param name="currentTime"></param>
        internal void requestAlarmWithCheck(DateTime currentTime)
        {
            if (!AlarmEnabledToggle.IsOn || alarm.currentState != AlarmState.IDLE) return;
            if (alarm.currentNotificationTime.CompareTo(currentTime) <= 0)
            {
                if (!AlarmsManager.IS_ALARM_NOTIFICATION_OPEN)
                {
                    alarm.playAlarmSound();
                    AlarmsManager.IS_ALARM_NOTIFICATION_OPEN = true;
                    mainPage.openAlarmNotificationWindow(alarm.label);
                    alarm.currentState = AlarmState.FIRST_TO_GO_OFF;
                }
                else
                {
                    EnableDisableAlarm_Button.Visibility = Visibility.Collapsed;
                    EditAlarm_Button.Visibility = Visibility.Collapsed;
                    DismissAlarmButton.Visibility = Visibility.Visible;
                    SnoozeAlarmButton.Visibility = Visibility.Visible;
                    alarm.currentState = AlarmState.SIDE_NOTIFICATION;
                }
            }
        }

        private void DismissAlarmButtonClick(object sender, RoutedEventArgs e)
        {
            EnableDisableAlarm_Button.Visibility = Visibility.Visible;
            EditAlarm_Button.Visibility = Visibility.Visible;
            DismissAlarmButton.Visibility = Visibility.Collapsed;
            SnoozeAlarmButton.Visibility = Visibility.Collapsed;
            WarningMessage.Visibility = Visibility.Collapsed;
            alarm.updateAlarmTime();
        }

        private void SnoozeAlarmButtonClick(object sender, RoutedEventArgs e)
        {
            EnableDisableAlarm_Button.Visibility = Visibility.Visible;
            EditAlarm_Button.Visibility = Visibility.Visible;
            DismissAlarmButton.Visibility = Visibility.Collapsed;
            SnoozeAlarmButton.Visibility = Visibility.Collapsed;
            WarningMessage.Visibility = Visibility.Collapsed;
            alarm.snooze();
        }

        /// <summary>
        /// Disable the alarm associated with this AlarmUserControl and updating the UI accordingly
        /// </summary>
        internal void disable()
        {
            alarm.enabled = false;
            AlarmEnabledToggle.IsOn = false;
        }

        /// <summary>
        /// Enable the alarm associated with this AlarmUserControl and updating the UI accordingly
        /// </summary>
        internal void enable()
        {
            alarm.enabled = true;
            AlarmEnabledToggle.IsOn = true;
        }
    }
}

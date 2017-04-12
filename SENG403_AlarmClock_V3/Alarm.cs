using SENG403_AlarmClock_V3;
using System;
using System.Runtime.Serialization;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace SENG403_AlarmClock_V3
{
    /// <summary>
    /// State of the alarm
    /// IDLE: Alarm has not rung (could be either enabled or disabled
    /// FIRST_TO_GO_OFF: Alarm is the first alarm that rang (out of all the alarms which are currently ringing). 
    /// If multiple alarms go off, the first alarm that rang will be the only alarm that displays a notification window.
    /// SIDE_NOTIFICATION: Alarm rang after some other alarm, which has not been dismissed yet.
    /// </summary>
    public enum AlarmState { IDLE, FIRST_TO_GO_OFF, SIDE_NOTIFICATION }

    /// <summary>
    /// This class encapsulates the logic and data associated with an alarm. 
    /// </summary>
    [DataContract]
    public class Alarm
    {
        /// <summary>
        /// defaultNotificationTime is the default time for which the alarm goes off. This is only used for repeating alarms.
        /// This variable keeps track of when an alarm should go off again in the next cycle (which doesn't depend on
        /// when the alarm is dismissed).
        /// </summary>
        [DataMember]
        public DateTime defaultNotificationTime { get; set; }

        /// <summary>
        /// currentNotificationTime keeps track of the alarm should ring again. This is usually the same as defaultAlarmTime, except
        /// when the user snooze the alarm, in which defaultAlarmTime doesn't change, but notifyAlarmTime := notifyAlarmTime + snoozeTime.
        /// </summary>
        [DataMember]
        public DateTime currentNotificationTime { get; set; } //when the alarm should go off after being snoozed

        [DataMember]
        public double snoozeTime { get; set; }

        [DataMember]
        public bool enabled { get; set; }

        /// <summary>
        /// How many days before the alarm goes off again (-1 if the alarm doesn't repeat
        /// </summary>
        [DataMember]
        public int repeatIntervalDays { get; set; }

        [DataMember]
        public string label { get; set; }

        [DataMember]
        public bool initialized { get; set; }

        [DataMember]
        public AlarmState currentState { get; set;}

        private const string DEFAULT_ALARM_SOUND = @"C:\Users\tcai\Documents\Visual Studio 2015\Projects\SENG403_G6_v2\SENG403_AlarmClock_V2\Sounds\missileAlert.wav";
        public MediaPlayer mediaPlayer { get; set; }

        /// <summary>
        /// Helper method for setting weekly alarms.
        /// </summary>
        /// <param name="day">Day of the week</param>
        /// <param name="alarmTime">Time of day of the alarm</param>
        internal void setWeeklyAlarm(DayOfWeek day, TimeSpan alarmTime)
        {
            enabled = false;
            initialized = true;
            repeatIntervalDays = 7;
            defaultNotificationTime = DateTime.Today.AddDays(day - DateTime.Now.DayOfWeek).Add(alarmTime);
            if (defaultNotificationTime.CompareTo(DateTime.Now) <= 0)
                defaultNotificationTime = defaultNotificationTime.AddDays(repeatIntervalDays);
            currentNotificationTime = defaultNotificationTime;
        }

        /// <summary>
        /// Helper method for setting daily alarms.
        /// </summary>
        /// <param name="alarmTime">Time of day of the alarm</param>
        internal void setDailyAlarm(TimeSpan alarmTime)
        {
            enabled = false;
            initialized = true;
            repeatIntervalDays = 1;
            defaultNotificationTime = DateTime.Today.Add(alarmTime);
            if (defaultNotificationTime.CompareTo(DateTime.Now) <= 0)
                defaultNotificationTime = defaultNotificationTime.AddDays(1);
            currentNotificationTime = defaultNotificationTime;
        }

        /// <summary>
        /// Helper method for setting alarm that only goes off once.
        /// </summary>
        /// <param name="dateTime">Date and time of when the alarm should go off. </param>
        internal void setOneTimeAlarm(DateTime dateTime)
        {
            initialized = true;
            repeatIntervalDays = -1;
            currentNotificationTime = defaultNotificationTime = dateTime;
        }

        public Alarm(double snoozeTime)
        {
            label = "An alarm";
            currentState = AlarmState.IDLE;
            enabled = initialized = false;
            this.snoozeTime = snoozeTime;
        }

        public void playAlarmSound()
        {
            mediaPlayer = new MediaPlayer();
            Uri pathUri = new Uri("ms-appx:///Assets/missileAlert.wav");
            mediaPlayer.Source = MediaSource.CreateFromUri(pathUri);
            mediaPlayer.Play();
        }

        /// <summary>
        /// Copy Constructor for Alarm class
        /// </summary>
        /// <param name="newAlarm"></param>
        public Alarm(Alarm newAlarm)
        {
            snoozeTime = newAlarm.snoozeTime;
        }

        public void snooze()
        {
            mediaPlayer.Pause();
            currentState = AlarmState.IDLE;
            currentNotificationTime = MainPage.currentTime.AddMinutes(snoozeTime);
        }

        /// <summary>
        /// Updates the defaultNotificationTime and currentNotificationTime when an alarm is dismissed.
        /// </summary>
        internal void updateAlarmTime()
        {
            mediaPlayer.Pause();
            currentState = AlarmState.IDLE;
            if (repeatIntervalDays != -1)
            {
                defaultNotificationTime = defaultNotificationTime.AddDays(repeatIntervalDays);
                currentNotificationTime = defaultNotificationTime;
            }
            else
            {
                enabled = false;
            }
        }
    }
}
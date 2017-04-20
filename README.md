************************************************************************************************************************

 
                                                    Alarm Clock v2.0
                                                         Group: 6
                                                 The Dining Philosophers
                                        SENG 403: Software Development in Groups                                                 

************************************************************************************************************************

Introduction
------------

Platform: C# 
IDE: Microsoft Visual Studio
UI: UWP


This is a simple alarm clock created to meet the requirements of a project module of the SENG 403 course. The alarm clock was developed in C#, designed to function in the Windows environment.  It has two major components: 1) A windows desktop version, 2) a Windows phone version. This repository includes all development associated with the latter (the phone version). All these systems were developed in an Agile Software development framework, where functionalities were incrementally added to satisfy client requests.   


Branches 
--------


Master - This branch includes all of the development associated with the mobile phone version alarm clock. It is up to date and ready for release.  






Important software components 
-------------------------------

-  MainPage.xaml.cs – The program logic contained in this file handles the interaction between the user and the home page. The user interaction with the edit alarm page is handled here as well.  The functions/classes are responsible for loading/saving alarm data from persistent storage, handle user input, and update the displayed time.   

-  AlarmUserControl.xaml.cs – This program component contains the necessary logic for deciding when the alarm should go off. Also, it is here where it is determined how the alarm window should be updated (for example, what alarm data should be displayed). 

- Alarm.cs – This handles the relevant alarm information. Likewise, functions contained here, update the alarm notification time. 

- SettingsPage.xaml.cs – Here, we update the global alarm settings from the user input. This is subsequently used by other program components. 

AlarmsManager.cs – This small program keeps track of global settings for all alarms. 


Important Features
--------------------------


- Large Central Digital Clock with date. 
- Ability to set time and date of the Alarm
- Ability to label Alarms
- Ability to set up weekly alarm
- Ability to set repeating or non-repeating alarms
- Set Snooze time
- Snooze/Dismiss alarm 
- Edit alarm settings (Enable, disable, set snooze)
- Ability to set up multiple alarms
- Ability to change alarm sound





      












# Unity-multiplatform-sign-in-GPGS-Apple-

# Introduction
Having spent some time setting up log in, cloud saves for apple and android, i've realised how hard is it to find complete solutions to smooth and easy setup for GPGS and apple game kit logins. This is an example project, where you can find a complete solution for authenticatio and cloud saves using GPGS and apple game center.


# Note
Each authentication method uses a different cloud save method. Guest (offline) users data is saved using PlayerPrefs, for Google play games services im using their own cloud save method and for apple - Unity Authentication and cloud save services

I am using Google play games services version v10.14 instead of the newest one (v11.01). More about this below

# Setting up
## Saved data
For data serialization i've written the DataParsing class. SavingData is a serialisable class that contains all the variables that need to be saved/loaded. Methods basically convert the SavingData object to/from bytes/string (json). 

To add your own variables, add them to the SavingData class and in ``` returnSavingBytes(), returnSavingString(), loadDataFromString(), loadDataFromSDO()``` Methods, copy them to/from the SavingData object. In my case i'm loading/copying them from ```PlayerData``` (a public static class im using to hold all variables)

## Guest (offline)

No setting up is needed

## Google play games services

Before following the tutorial below, instead of downloading the v11.01 version download the v10.14 version [here](https://github.com/playgameservices/play-games-plugin-for-unity/releases/tag/v10.14). The new version removed some functionality - most importantly logging out. It always tries a silent sign in on launch and i cant figure out why it doesnt work for me :) 


Follow this [tutorial](https://forum.unity.com/threads/tutorial-authentication-with-google-play-games.1409151/) for complete setup of GPGS authentication. 

### Debugging GPGS 

To find any errors/see logs for the login/saving process on android you need to use the ADB logcat.
Download the Android logcat package from package manager. Open it in window > analysis > android logcat. Connect your device with usb (make sure to enable usb debugging in phone settings). To filter just the unity logs, press on filter and choose the open application (ussually on top).

If GPGS retuns statuses like Canceled, Developer error and etc. you won't see the true error in the current app filter, you have to turn off the filter and search for the actual error log in the whole mess (let me know if there's a better way to do this). 

### Common issues setting up GPGS

1. You have to publish the google play games services project. It is not the actual game, just the play games services, go to the google play developer console >  Play games services > configuration > review and publish. If you ever do changes to GPGS after the publishing, you will have to publish again.
2. Tester accounts. Make sure, that the email you are using to log in is in the list of testers under Play games services > Testers. 
3. ... 

## Apple game center login

All of this has to be done on a mac. If you don't have a mac - you could try to look into renting a mac online or using a friend's mac with teamviewer (what i did). 

Apart from setting up the project in apple game centre, you also have to do this (note : this is copied from official unity [documentation](https://docs.unity.com/ugs/manual/authentication/manual/platform-signin-apple-game-center)):

1. Set up and install the Apple Game Kit Unity plugin, found in the Apple Unity Plug-ins repository. The GameKit framework is used to implement Apple Game Center features including player identity. 

2. Add Apple Game Center as an ID provider for Unity:
 In the Unity Editor menu, go to Edit > Project Settings…, then select Services > Authentication from the navigation menu.
Set ID Providers to Apple Game Center, then select Add.
Enter the Bundle ID from the Apple developer console in the Bundle ID text field, then select Save. The Bundle ID should look like this: "com.something.somethingelse".

### Debugging apple

To see in game logs you have to build from xcode to your iphone.

Because apple won't let unity logs into ios logs, the only other way i found how to do this (when you can't connect the phone to xcode) was - setting up a php script on a vps that accepts https requests and saves the logs into a text file. Then in unity i put a bunch of try catch blocks, that sent the erors to the vps.

### Common issues with apple

1. Game centre login won't work until you add a leaderboard in the appstore connect page. Even though you don't use the leaderboard - game centre login for some reason won't work until you add a dummy leaderboard. Not sure if this is for everyone, but for me and some people online it worked.
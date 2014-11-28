AudioRecorder
=============

AudioRecorder example application demonstrates how to record and play audio on
Windows Phone 8 devices. XNA Framework Audio API and Windows Audio Session API
(WASAPI) are covered by the application. 

![Main page](/doc/audio_recorder_main_page_small.png?raw=true)&nbsp;
![Audio files page](/doc/audio_recorder_files_page_small.png?raw=true)

This example application is hosted in GitHub:
https://github.com/Microsoft/audio-recorder

Developed with:

 * Microsoft Visual Studio Express for Windows Phone 2012.

Compatible with:

 * Windows Phone 8

Tested to work on:

 * Nokia Lumia 920 
 * Nokia Lumia 925
 * Nokia Lumia 1520 


Instructions
------------

Make sure you have the following installed:

 * Windows 8
 * Windows Phone SDK 8.0

To build and run the sample:

 * Open the SLN file
   * File > Open Project, select the file AudioRecorder.sln
 * Select the target, for example 'Emulator WXGA'.
 * Press F5 to build the project and run it on the Windows Phone Emulator.

To deploy the sample on Windows Phone device:
 * See the official documentation for deploying and testing applications on
   Windows Phone devices at http://msdn.microsoft.com/en-us/library/windowsphone/develop/ff402565(v=vs.105).aspx


About the implementation
------------------------

Important folders:

| Folder | Description |
| ------ | ----------- |
| / | Contains the project file, the license information and this file (README.md) |
| AudioRecorder | Root folder for the implementation files. |
| AudioRecorder/Assets | Graphic assets like icons and tiles. |
| AudioRecorder/Properties | Application property files. |
| AudioRecorder/Resources | Application resources. |
| WasapiAudioComp | Root folder of Windows Phone Runtime component for WASAPI implementation files. |


Important classes:

| File | Description |
| ---- | ----------- |
| MainPage | This class is the main UI of the app. |
| AudioManager | Handles all the UI related audio actions. |
| XnaAudio | Handles the recording and playback of audio using XNA Audio API. |
| WasapiAudio | Handles the audio capturing and rendering using WASAPI. |


For more information about audio handling in Windows Phone 8 devices, see an
article available at http://developer.nokia.com/Community/Wiki/Audio_recording_and_playback_options_in_Windows_Phone.


Known issues
------------

No known issues.


License
-------

See the license file delivered with this project.
The license is also available online at
https://github.com/Microsoft/audio-recorder/blob/master/License.txt


Version history
---------------

 * 0.3.0.0 Added 720p resolution support and yet another missing dependency fix.
 * 0.2.0.0 Added a missing dependency affecting others than ARM device builds.
 * 0.1.0.0 First beta release.

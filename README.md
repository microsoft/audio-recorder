AudioRecorder
=============

AudioRecorder example application demonstrates how to record and play audio on Windows Phone 8 devices. XNA Framework Audio API and Windows Audio Session API (WASAPI) are covered by the application. 

This example application is hosted in GitHub:
https://github.com/nokia-developer/audio-recorder

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
 * See the official documentation for deploying and testing applications on Windows Phone devices at http://msdn.microsoft.com/en-us/library/windowsphone/develop/ff402565(v=vs.105).aspx


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


For more information about audio handling in Windows Phone 8 devices, see an article available at http://developer.nokia.com/Community/Wiki/Audio_recording_and_playback_options_in_Windows_Phone.


Known issues
------------

No known issues.


License
-------

    Copyright © 2013 Nokia Corporation. All rights reserved.
    
    Nokia, Nokia Developer, and HERE are trademarks and/or registered trademarks of
    Nokia Corporation. Other product and company names mentioned herein may be
    trademarks or trade names of their respective owners.
    
    License
    Subject to the conditions below, you may use, copy, modify and/or merge copies
    of this software and associated content and documentation files (the “Software”)
    to test, develop, publish, distribute, sub-license and/or sell new software
    derived from or incorporating the Software, solely in connection with Nokia
    devices. Some of the documentation, content and/or software maybe licensed under
    open source software or other licenses. To the extent such documentation,
    content and/or software are included, licenses and/or other terms and conditions
    shall apply in addition and/or instead of this notice. The exact terms of the
    licenses, disclaimers, acknowledgements and notices are reproduced in the
    materials provided, or in other obvious locations. No other license to any other
    intellectual property rights is granted herein.
    
    This file, unmodified, shall be included with all copies or substantial portions
    of the Software that are distributed in source code form.
    
    The Software cannot constitute the primary value of any new software derived
    from or incorporating the Software.
    
    Any person dealing with the Software shall not misrepresent the source of the
    Software.
    
    Disclaimer
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
    FOR A PARTICULAR PURPOSE, QUALITY AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES (INCLUDING,
    WITHOUT LIMITATION, DIRECT, SPECIAL, INDIRECT, PUNITIVE, CONSEQUENTIAL,
    EXEMPLARY AND/ OR INCIDENTAL DAMAGES) OR OTHER LIABILITY, WHETHER IN AN ACTION
    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
    SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    
    Nokia Corporation retains the right to make changes to this document at any
    time, without notice.


Version history
---------------

 * 0.3.0.0 Added 720p resolution support and yet another missing dependency fix.
 * 0.2.0.0 Added a missing dependency affecting others than ARM device builds.
 * 0.1.0.0 First beta release.

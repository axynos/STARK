![Logo Image](http://i.imgur.com/Nryqfgk.pngg)
## Source Audio Mixer and TTS

## [DOWNLOAD (Grim's Version)](https://github.com/GrimReaperFloof/STARK/blob/master/DOWNLOAD%20EXE%20FILE%20HERE/STARK.exe?raw=true)
## [DOWNLOAD (Original Version)](https://github.com/axynos/STARK/releases/latest)

STARK is an application that allows you to read commands from Source games and playback audio files, or use Text-to-Speech.
This is a project that evolved from a proof-of-concept [axynos](https://github.com/axynos) made a while ago using AutoHotKey [link](https://github.com/axynos/CSGO-Text-To-Speech).

Donate to [axynos](https://github.com/axynos) --> [![Donation Image](http://i.imgur.com/fbH2hRv.png)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=LB5YVGD9F8U5L) if you want to or contribute to the project with code (see contributing section).

### Dependencies
* [Virtual Audio Cable](http://software.muzychenko.net/eng/vac.htm). You can also use a free alternative called [VB-CABLE](http://vb-audio.pagesperso-orange.fr/Cable/index.htm) ([Direct download link](http://vbaudio.jcedeveloppement.com/Download_CABLE/VBCABLE_Driver_Pack43.zip))


### How to set up
[Either watch this video or follow the steps below.](https://www.youtube.com/watch?v=fi5I6bzy2f8&feature=youtu.be)

1. Download [STARK](http://google.com) and Virtual Audio Cable
2. Install 32/64 bit version of Virtual Audio Cable according to your Windows version
3. Open the x86/x64 folder from the installation folder of Virtual Audio Cable
4. Open vcctlpan.exe as Administrator
5. In the top-left corner change Cables to 2 and click set
6. Close vcctlpan.exe
7. Open Steam
8. Go to Settings and select Voice
9. Change your audio device to be Line 2
10. Close Settings
11. Open STARK
12. Select the Setup tab
13. Change the Microphone to your actual microphone
14. Change the Standard Output to Line 1
15. Change the Loopback Output to your Speakers/Headphones
16. Choose the Watch Folder(the folder where it will look for audio files)
17. Make sure it has found your SteamApps Folder (if it hasn't, make sure Steam is open)
18. Make sure the game you want to play is selected
19. Leave STARK open and go to the folder where you installed Virtual Audio Cable from
20. Open the x86/x64 folder and open audiorepeater_ks.exe
21. Set the Wave In to Line 1
22. Set the Wave Out to Line 2
23. Set the Total buffer to something around 20-80 ms (this value depends on your pc's specs, higher end pc-s can have lower values)
24. Set the Parts to 4
25. Click Start
26. Open the game you selected in STARK
27. Open the developer console and type: exec stark
28. STARK should now be working and the list of commands you can use should be displayed in the console.

### Contributing
If you wish to contribute you can help [axynos](https://github.com/axynos) by either:
* Committing more features/bugfixes/improvements
* [Donating some cash to axynos to help feed his coffee addiction](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=LB5YVGD9F8U5L)

Also, reminder: this version of STARK only works with Team Fortress 2, I
did this because selectedGame is broken, and tf2 is the only source game
I play so yeah. You can change the game by pressing CTRL + SHIFT + F in
Visual Studio and search for "tf\\" in the entire solution, then replace
the results with the source game of your choice.

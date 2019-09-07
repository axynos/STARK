![Logo Image](http://i.imgur.com/Nryqfgk.pngg)
## Source Audio Mixer and TTS

## [DOWNLOAD](https://github.com/GrimReaperFloof/STARK/releases)

STARK is an application that allows you to read commands from Source games and playback audio files, or use Text-to-Speech.
This is a project that evolved from a proof-of-concept [axynos](https://github.com/axynos) made a while ago using AutoHotKey [link](https://github.com/axynos/CSGO-Text-To-Speech).

Donate to [axynos](https://github.com/axynos) --> [![Donation Image](http://i.imgur.com/fbH2hRv.png)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=LB5YVGD9F8U5L) if you want to or contribute to the project with code (see contributing section).

### Dependencies
* [Virtual Audio Cable](http://software.muzychenko.net/eng/vac.htm). You can also use a free alternative called [VB-CABLE](http://www.vb-audio.com/Cable/index.htm) ([Direct download link](http://vbaudio.jcedeveloppement.com/Download_CABLE/VBCABLE_Driver_Pack43.zip))


### How to set up
1. Download [STARK](http://google.com) and Virtual Audio Cable (or [VB-CABLE](http://www.vb-audio.com/Cable/index.htm), which is a free alternative)
2. Install 32/64 bit version of Virtual Audio Cable according to your Windows version (or VB-CABLE)
3. Open the x86/x64 folder from the installation folder of Virtual Audio Cable (or VB-CABLE)
4. Open vcctlpan.exe as Administrator (not required if you have VB-CABLE)
5. In the top-left corner change Cables to 2 and click set (not required if you have VB-CABLE)
6. Close vcctlpan.exe (not required if you have VB-CABLE)
7. Open Steam
8. Go to Settings and select Voice
9. Change your audio device to be Line 2 (Change your audio device to "CABLE Output" if you have VB-CABLE)
10. Close Settings
11. Put STARK in an empty folder because it will create some useful files
12. Open STARK
13. Go into the folder you opened STARK from
14. Open game.txt and set your game
15. Close and re-open STARK for it to take effect
16. Select the Setup tab
17. Change the Microphone to your actual microphone
18. Change the Standard Output to Line 1 ("CABLE Output" if you have VB-CABLE)
19. Change the Loopback Output to your Speakers/Headphones
20. Choose the Watch Folder(the folder where it will look for audio files)
21. Make sure it has found your SteamApps Folder (if it hasn't, make sure Steam is open)
22. Leave STARK open and go to the folder where you opened STARK from and open whitelisted_users.txt
23. Add your Steam Community name (the name you see on your own profile) to the file and save (you don't need to restart STARK for it to take effect this time)
24. Go to the folder where you installed Virtual Audio Cable from (skip this if you have VB-CABLE)
25. Open the x86/x64 folder and open audiorepeater_ks.exe (skip this if you have VB-CABLE)
26. Set the Wave In to Line 1 (skip this if you have VB-CABLE)
27. Set the Wave Out to Line 2 (skip this if you have VB-CABLE)
28. Set the Total buffer to something around 20-80 ms (this value depends on your pc's specs, higher end pc-s can have lower values) (skip this if you have VB-CABLE)
29. Set the Parts to 4 (skip this if you have VB-CABLE)
30. Click Start (skip this if you have VB-CABLE)
31. Open the game you have set in game.txt
32. Open the developer console and type: exec stark
33. STARK should now be working and the list of commands you can use should be displayed in the console.

### Contributing
If you wish to contribute you can help [axynos](https://github.com/axynos) by either:
* Committing more features/bugfixes/improvements
* [Donating some cash to axynos to help feed his coffee addiction](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=LB5YVGD9F8U5L)

using System;
using System.IO;
using System.Text;
using System.Timers;
using System.Windows;

namespace STARK {
    class CommandReader {

        string path = @"E:\Steam\SteamApps\common\Counter-Strike Global Offensive\csgo\!tts-axynos.slf";
        Command synthCmd;
        Command audioCmd;
        Command pauseCmd;
        Command resumeCmd;
        Command stopCmd;

        Timer loop; //no, you can't use FileSystemWatcher, it doesn't work. I tried.
        StreamReader reader;

        QueuedSpeechSynthesizer qss;
        AudioPlaybackEngine ape;
        AudioFileManager afm;

        bool changingPath = false;

        public CommandReader(ref QueuedSpeechSynthesizer qss, ref AudioPlaybackEngine ape, ref AudioFileManager afm, string synthCmd) {
            Setup(path);
            StartReadLoop();
            this.qss = qss;
            this.ape = ape;
            this.afm = afm;
            
            this.synthCmd = new Command(synthCmd);
            audioCmd = new Command(".play");
            pauseCmd = new Command(".pause");
            resumeCmd = new Command(".resume");
            stopCmd = new Command(".stop");
        }


        private void parseCommand(string line) {
            if (line != "") {
                if (ContainsCommand(line, synthCmd)) {
                    if (qss != null) {
                        var parts = getParts(line, synthCmd);
                        string prompt = parts[1];
                        string player = getPlayer(parts[0]);

                        qss.AddToQueue(new QSSQueueItem(prompt, player));
                    }
                }
                else if (ContainsCommand(line, audioCmd)) {
                    if (ape != null) {
                        var parts = getParts(line, audioCmd);
                        string player = getPlayer(parts[0]);
                        //Removes the leading space from the command arg
                        string arg1 = new StringBuilder(parts[1]).Remove(0, 1).ToString();

                        int id;
                        if (int.TryParse(arg1, out id) && id >= 0) {
                            if (id < afm.getCollection().Count) {
                                ape.Play(id);
                            }
                        } else {
                            
                        }
                    }
                }
                else if (ContainsCommand(line, pauseCmd)) {
                    ape.Pause();
                }
                else if (ContainsCommand(line, resumeCmd)) {
                    ape.Resume();
                }
                else if (ContainsCommand(line, stopCmd)) {
                    ape.Stop();
                }
            }
        }

        private void StartReadLoop() {
            loop = new Timer(100);
            loop.AutoReset = false;
            loop.Elapsed += Loop_Elapsed;
            loop.Start();
        }

        private void Setup(string path) {
            reader = new StreamReader(File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite));
            //skips the lines that are in the file on load so we don't get historic commands
            reader.ReadToEnd();
        }

        private bool ContainsCommand(string line, Command command) {
            if (line.Contains(command.getCommand())) return true;
            else return false;
        }

        private void Loop_Elapsed(object sender, ElapsedEventArgs e) {
            if (changingPath == false && File.Exists(path)) {
                //goes through lines that don't interest us at once, no need to loop again between each line
                while (true) {
                    var line = reader.ReadLine();
                    if (line == null) break;
                    parseCommand(line);
                }
            }
            loop.Start();
        }

        //CHANGE METHODS
        #region "Change Methods"
        public void ChangePath(string path) {
            changingPath = true;
            if (File.Exists(path)) {
                reader.Close();
                this.path = path;
                Setup(path);
            }
            changingPath = false;
        }

        public void ChangeSynthCommand(string newCmd) {
            synthCmd.changeCommand(newCmd);
        }
        #endregion

        //GETTERS
        #region "Getters"
        //use only with actual fucking parts[0], not some random shit, you fuck
        private string getPlayer(string input) {
            return new StringBuilder(input).Remove(input.Length - 2, 2).ToString();
        }

        private string[] getParts(string line, Command command) {
            return line.Split(command.getSplitter(), StringSplitOptions.None);
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace STARK {
    public class ConsoleUI {

        List<string> startUpContent;
        List<string> trackListContent;
        List<string> helpContent;
        List<TrackListItem> tracks;

        AudioFileManager afm;
        SourceGame game;
        MainWindow mw;

        //AudioFileManager afm;

        public ConsoleUI(SourceGame game, ref AudioFileManager afm, ref MainWindow mw) {
            this.afm = afm;
            startUpContent = new List<string>();
            trackListContent = new List<string>();
            helpContent = new List<string>();
            tracks = new List<TrackListItem>();
            this.mw = mw;
            this.game = game;

            Render();
        }

        public void Render() {
            DeleteFiles();

            startUpContent = null;
            trackListContent = null;
            helpContent = null;
            tracks = null;

            startUpContent = new List<string>();
            trackListContent = new List<string>();
            helpContent = new List<string>();
            tracks = new List<TrackListItem>();
 
            //Create help menu
            AddLines(ref helpContent, new string[] {
                "clear",
                "echo \"================ STARK by axynos ================\"",
                "echo \"\"",
                "echo \"     + Available *CONSOLE* commands:\"",
                "echo \"\"",
                "echo \"      * Media\"",
                "echo \"        - exec s_play_<id> - play file by id\"",
                "echo \"        - s_pause       - pause playback\"",
                "echo \"        - s_resume      - resume playback\"",
                "echo \"        - s_stop        - stop playback\"",
                "echo \"        - s_tracklist   - show tracklist\"",
                "echo \"        - s_help        - show this menu\"",
                "echo \"\"",
                "echo \"     + Available *CHAT* commands:\"",
                "echo \"\"",
                "echo \"      * Media\"",
                "echo \"        - .play <id>    - play file by id\"",
                "echo \"        - .play <name>  - play file by name\"",
                "echo \"        - .play <tag>   - play file by tag\"",
                "echo \"        - .pause        - pause playback\"",
                "echo \"        - .resume       - resume playback\"",
                "echo \"        - .stop         - stop playback\"",
                "echo \"\"",
                "echo \"      * Text-to-Speech\"",
                "echo \"        - " + mw.TTS_CommandTextBox.Text + " <text> - say text from tts\"",
                "echo \"\"",
                "echo \"=================================================\""
            });

            //Create startup
            AddLines(ref startUpContent, new string[] {
                "clear",
                "alias s_pause \"exec s_pause\"",
                "alias s_resume \"exec s_resume\"",
                "alias s_stop \"exec s_stop\"",
                "alias s_tracklist \"exec s_tracklist\"",
                "alias s_help \"exec s_help\"",
                "con_logfile !tts-axynos.log",
                "exec s_help"
            });

            //Create list of tracks
            foreach (AudioPlaybackItem item in afm.getCollection()) {
                tracks.Add(new TrackListItem("echo \"     " + item.id + " :: " + item.name + " :: " + item.displayTags + "\"", item.id));
            }

            //Create tracklist
            AddLines(ref trackListContent, new string[] {
                "clear",
                "echo \"============ STARK by axynos ============\"",
                "echo \"\"",
                "echo \"   + Available audio tracks:\"",
            });

            foreach (TrackListItem track in tracks) {
                trackListContent.Add(track.display);
            }

            AddLines(ref trackListContent, new string[] {
                "echo \"\"",
                "echo \"=========================================\""
            });

            WriteToFiles();
        }


        private void AddLines(ref List<string> target, string[] input) {
            foreach (string line in input) {
                target.Add(line);
            }
        }

        private void WriteToFiles() {
            WriteToFile("s_help", helpContent);
            WriteToFile("stark", startUpContent);
            WriteToFile("s_pause", new List<string>() { "echo \"STARK: Pausing audio track playback.\"", "echo \"CONSOLE : .pause\"" });
            WriteToFile("s_resume", new List<string>() { "echo \"STARK: Resuming audio track playback.\"", "echo \"CONSOLE : .resume\"" });
            WriteToFile("s_stop", new List<string>() { "echo \"STARK: Stopping audio track playback.\"", "echo \"CONSOLE : .stop\"" });
            WriteToFile("s_tracklist", trackListContent);
            //todo save track play files
            
            //save track play files
            if (tracks != null && tracks.Count > 0) {
                foreach (TrackListItem item in tracks) {
                    WriteToFile(item.fileName, new List<string> {
                        "echo \"Playing track: \"" + afm.getCollection()[item.id].name,
                        "echo \"CONSOLE : .play " + item.id + "\"",
                        item.display
                    });
                }
            }   
        }

        public void DeleteFiles() {
            if (Directory.Exists(PathManager.steamApps + MainWindow.gameDir + @"\cfg")) {
                DeleteFile("s_help");
                DeleteFile("stark");
                DeleteFile("s_pause");
                DeleteFile("s_resume");
                DeleteFile("s_stop");
                DeleteFile("s_tracklist");
            
                if (tracks != null && tracks.Count > 0) {
                    foreach (TrackListItem item in tracks) {
                        DeleteFile(item.fileName);
                    }
                }
            }

        }

        private void DeleteFile(string fileName) {
            string path = PathManager.steamApps + MainWindow.gameDir + @"\cfg" + "\\" + fileName + ".cfg";

            try {
                if (File.Exists(path)) File.Delete(path);
            } catch (IOException e) {
                MessageBox.Show(e.Message);
            }
        }

        private void WriteToFile(string fileName, List<string> content) {
            string path = PathManager.steamApps + MainWindow.gameDir + @"\cfg" + "\\" + fileName + ".cfg";

            try {
                if (!File.Exists(path)) {
                    File.Create(path).Close();
                }

                using (StreamWriter writer = new StreamWriter(File.Open(path, FileMode.Truncate, FileAccess.ReadWrite, FileShare.Delete))) {
                    try {
                        foreach (string line in content) {
                            writer.WriteLine(line);
                        }
                    } catch (IOException e) {
                        
                    }

                }
            } catch (IOException e) {
                
                mw.setConUItoNull();
            }
        }

        private void WriteToFile(string fileName, string content) {
            string path = PathManager.steamApps + MainWindow.gameDir + @"\cfg" + "\\" + fileName + ".cfg";


            if (!File.Exists(path)) {
                File.Create(path).Close();
            }

            using (StreamWriter writer = new StreamWriter(File.Open(path, FileMode.Truncate, FileAccess.ReadWrite, FileShare.Delete))) {
                try {
                    writer.WriteLine(content);
                }
                catch (IOException e) {
                    MessageBox.Show(e.Message);
                }
            }
        }
    }

    class TrackListItem {

        public int id;
        public string display;
        public string fileName;

        public TrackListItem(string display, int id) {
            this.display = display;
            this.id = id;
            buildFileName();
        }

        private void buildFileName() {
            fileName = "s_play_" + id;
        }
    }
}

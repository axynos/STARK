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

        SourceGame game;

        //AudioFileManager afm;

        public ConsoleUI(SourceGame game) {
            //afm = (App.Current.MainWindow as MainWindow).getAFM();
            startUpContent = new List<string>();
            trackListContent = new List<string>();
            helpContent = new List<string>();
            this.game = game;

            Render();
        }

        public void Render() {
            startUpContent.Clear();
            trackListContent.Clear();
            helpContent.Clear();

            AddLines(ref startUpContent, new string[25] {
                "clear",
                "echo \"============ STARK by axynos ============\"",
                "echo \"\"",
                "echo \"     + Available *CONSOLE* commands:\"",
                "echo \"\"",
                "echo \"      * Media\"",
                "echo \"        - s_play <id> - play file by id\"",
                "echo \"        - s_pause     - pause playback\"",
                "echo \"        - s_resume    - resume playback\"",
                "echo \"        - s_stop      - stop playback\"",
                "echo \"        - s_tracklist - show tracklist\"",
                "echo \"        - s_help      - show this menu\"",
                "echo \"\"",
                "echo \"     + Available *CHAT* commands:\"",
                "echo \"\"",
                "echo \"      * Media\"",
                "echo \"        - .play <id>  - play file by id\"",
                "echo \"        - .pause      - pause playback\"",
                "echo \"        - .resume     - resume playback\"",
                "echo \"        - .stop       - stop playback\"",
                "echo \"\"",
                "echo \"      * Text-to-Speech\"",
                "echo \"        - .tts <text> - say text from tts\"",
                "echo \"\"",
                "echo \"=========================================\""
            });

            WriteToFiles();
        }

        public void DeleteFiles() {

        }

        private void AddLines(ref List<string> target, string[] input) {
            foreach (string line in input) {
                target.Add(line);
            }
        }

        private void WriteToFiles() {
            string path = game.cfgDir + @"\shelp.cfg";
            MessageBox.Show(path);

            //MessageBox.Show(path);

            //if (File.Exists(path)) {
            //    File.Delete(path);
            //    File.Create(path);
            //} else {
            //    File.Create(path);
            //}

            try {
                File.Create(path).Close();
                //TextWriter writer = new StreamWriter(path, true);
                //writer.WriteLine("wtffff");
                //writer.Flush();
                //writer.Close();
            } catch (Exception e) {
                MessageBox.Show(e.ToString());
            }
            //for (int i = 0; i < startUpContent.Count; i++) {
            //    writer.WriteLine(startUpContent[i]);
            //}
            //foreach (string line in startUpContent) {

            //}
        }
    }
}

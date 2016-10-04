using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace STARK {
    public class AudioPlaybackItem {
        public int id { get; set; }
        public bool isDefault { get; set; } = false;
        public string name { get; set; }
        public int volume { get; set; }
        public string displayTags { get; set; } = "";
        
        string fileName { get; set; }
        public string path { get; set; }
        public ObservableCollection<string> tags { get; set; }

        AudioFileManager afm;

        public AudioPlaybackItem(string name, string path, int volume) {
            this.name = name;
            this.path = path;
            this.fileName = Path.GetFileName(path);
            this.volume = volume;
            afm = getAFM();
            tags = new ObservableCollection<string>();

            //update the list in the ui
            tags.CollectionChanged += Tags_CollectionChanged;

            //if the name is empty, it will generate a new name
            GenerateName();
            GenerateTags();
        }


        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            GenerateTagsDisplayString();
        }

        //GETTERS
        #region "getters"
        public string GetPath() {
            return path;
        }

        private AudioFileManager getAFM() {
            AudioFileManager afm = null;
            App.Current.Dispatcher.Invoke(delegate {
                afm = (Application.Current.MainWindow as MainWindow).getAFM();
            });

            if (afm == null) return null;

            return afm;
        }
        #endregion

        //GENERATION METHODS
        #region "Generation Methods"
        private void GenerateName() {
            if (name == "") name = Path.GetFileNameWithoutExtension(path);
        }

        private void GenerateTags() {
            string[] potTags = name.Split(new string[] { " ", ".", "-", "_" }, StringSplitOptions.None);
            App.Current.Dispatcher.Invoke(delegate {
                foreach (string tag in potTags) {
                    if (!IsNumeric(tag)) {
                        if (!afm.getTagsTracker().Contains(tag)) {
                            tags.Add(tag);
                            afm.getTagsTracker().Add(tag);
                        }
                    }
                }
            });
        }

        public void GenerateTagsDisplayString() {
            var thing = new StringBuilder();
            bool isFirst = true;
            foreach (string tag in tags) {
                if (!isFirst) thing.Append(", ");
                if (isFirst) isFirst = false;
                thing.Append(tag);
            }

            displayTags = thing.ToString();
        }
        #endregion

        //CHANGE METHODS
        #region "changeMethods"
        public void ChangeId(int id) {
            this.id = id;
        }

        public void changeVolume(int volume) {
            if (volume < 0) volume = 0;
            this.volume = volume;
        }

        public void changeName(string name) {
            if (name == "") this.name = Path.GetFileNameWithoutExtension(fileName);
            else this.name = name;
        }

        public void changeFileName(string fileName) {
            this.fileName = fileName;
        }
        #endregion

        //UTILS
        #region "utils"
        private float IntToFloat(int input) {
            return 0.01f * input;
        }

        private bool IsNumeric(string input) {
            double test;
            return double.TryParse(input, out test);
        }
        #endregion
    }
}

using System.Collections.ObjectModel;
using System.IO;

namespace STARK {
    class AudioPlaybackItem {
        string name;
        string fileName;
        float volume;
        
        ObservableCollection<string> tags;
        
        public AudioPlaybackItem(string name, string fileName, int volume) {
            if (name == "") this.name = Path.GetFileNameWithoutExtension(fileName);
            else this.name = name;

            fileName = name;
            this.volume = IntToFloat(volume);
        }


        //CHANGE METHODS
        #region "changeMethods"
        public void changeVolume(int volume) {
            if (volume < 0) volume = 0;
            this.volume = IntToFloat(volume);
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
        #endregion
    }
}

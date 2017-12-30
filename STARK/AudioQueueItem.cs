namespace STARK
{
    public class AudioQueueItem {

		public string title { get; set; }

        public string filePath { get; set; }

        public System.TimeSpan duration { get; set; }

        public string requester { get; set; }

        public AudioQueueItem(string title, string filePath, System.TimeSpan duration, string requester) {
            this.title = title;
            this.filePath = filePath;
            this.duration = duration;
            this.requester = requester;
        }
	}
}

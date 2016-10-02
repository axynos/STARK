using System;

namespace STARK {
    public class QSSQueueItem {

		public String QIText { get; set; }

		public string QISource { get; set; }

        public QSSQueueItem(string text, string source) {
            QIText = text;
            QISource = source;
        }
	}
}

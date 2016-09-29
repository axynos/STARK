using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace STARK {
	public class QSSQueueItem {

		public string QIText { get; set; }

		public String QISource { get; set; }

        public QSSQueueItem(string text, string source) {
            QIText = text;
            QISource = source;
        }
	}
}

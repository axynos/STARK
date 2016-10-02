using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

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

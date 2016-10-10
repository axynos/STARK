namespace STARK {
    public class Command {

        string command { get; set; }
        string[] splitter { get; set; }

        /// <summary>
        /// Creates a command and it's splitter based on the command identifier given.
        /// </summary>
        /// <param name="command">The command identifier</param>
        public Command(string command) {
            ConstructCommand(command);
        }

        /// <summary>
        /// Changes the command's identifier and generates a new splitter based on it
        /// </summary>
        /// <param name="cmd">The new identifier to be used</param>
        public void changeCommand(string cmd) {
            command = null;
            splitter = null;
            ConstructCommand(cmd);
        }

        /// <summary>
        /// Constructs a command and it's splitter based on the info given.
        /// </summary>
        /// <param name="cmd">The identifier that the splitter is based on.</param>
        private void ConstructCommand(string cmd) {
            command = cmd;
            splitter = new string[] { ": " + command, ":  " + command };
        }

        public string getCommand() {
            return command;
        }

        public string[] getSplitter() {
            return splitter;
        }

        public string getSplitterAsString() {
            return splitter[0];
        }
    }
}

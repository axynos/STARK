namespace STARK {
    public static class CommandManager {
        public static Command synthCmd = new Command("", true);
        public static Command playCmd = new Command(".play", true);
        public static Command playVideoCmd = new Command(".playvideo", true);
        public static Command pauseCmd = new Command(".pause", false);
        public static Command resumeCmd = new Command(".resume", false);
        public static Command stopCmd = new Command(".stop", false);
        public static Command skipCurrentCmd = new Command(".skip", false);
        public static Command clearQueueCmd = new Command(".clear", false);
        public static Command blockUserCmd = new Command(".block", true);
        public static Command blockWordCmd = new Command(".blockword", true);
    }
}

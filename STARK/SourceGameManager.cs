using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace STARK {
    public static class SourceGameManager {
        public static SourceGame selectedGame;

        public static readonly SourceGame csgo = new SourceGame
            (
                "Counter-Strike: Global Offensive",         //name
                @"\common\Counter-Strike Global Offensive", //dir
                @"\csgo\cfg",                               //cfgDir
                @"\csgo",                                   //libDir
                "csgo",                                     //exe
                730                                         //id
            );

        public static readonly SourceGame css = new SourceGame
            (
                "Counter-Strike: Source",                   //name
                @"\common\Counter-Strike Source",           //dir
                @"\cstrike\cfg",                            //cfgDir
                @"\cstrike",                                //libDir
                "hl2",                                      //exename
                240                                         //id
            );

        public static readonly SourceGame tf2 = new SourceGame
            (
                "Team Fortress 2",                          //name
                @"\common\Team Fortress 2",                 //dir
                @"\tf\cfg",                                 //cfgDir
                @"\tf",                                     //libDir
                "hl2",                                      //exe
                440                                         //id
            );

        public static readonly SourceGame gmod = new SourceGame
            (
                "Garry's Mod",                              //name
                @"\common\GarrysMod",                       //dir
                @"\garrysmod\cfg",                          //cfgDir
                @"\garrysmod",                              //libDir
                "hl2",                                      //exe
                4000                                        //id
            );

        public static readonly SourceGame hl2dm = new SourceGame
            (
                "Half-Life 2 DeathMatch",                   //name
                @"\common\half-life 2 deathmatch",          //dir
                @"\hl2mp\cfg",                              //cfgDir
                @"\hl2mp",                                  //libDir
                "hl2",                                      //exe
                320                                         //id
            );

        public static readonly SourceGame l4d = new SourceGame
            (
                "Left 4 Dead",                              //name
                @"\common\Left 4 Dead",                     //dir
                @"\left4dead\cfg",                          //cfgDir
                @"\left4dead",                              //libDir
                "hl2",                                      //exe
                500                                         //id
            );

        public static readonly SourceGame l4d2 = new SourceGame
            (
                "Left 4 Dead 2",                            //name
                @"\common\Left 4 Dead 2",                   //dir
                @"\left4dead2\cfg",                         //cfgDir
                @"\left4dead2",                             //libDir
                "left4dead2",                               //exe
                550                                         //id
            );

        public static readonly SourceGame dods = new SourceGame
            (
                "Day of Defeat Source",                     //name
                @"\common\day of defeat source",            //dir
                @"\dod\cfg",                                //cfgDir
                @"\dod",                                    //libDir
                "hl2",                                      //exe
                300                                         //id
            );

        public static readonly SourceGame insurg = new SourceGame
            (
                "Insurgency",                               //name
                @"\common\insurgency2",                     //dir
                @"\insurgency\cfg",                         //cfgDir
                @"\insurgency",                             //libDir
                "insurgency",                               //exe
                222880                                      //id
            );

        public static readonly SourceGame[] gamesList = new SourceGame[9] {
                csgo,
                css,
                tf2,
                gmod,
                hl2dm,
                l4d,
                l4d2,
                dods,
                insurg
    };

        public static readonly ObservableCollection<SourceGame> games = new ObservableCollection<SourceGame>(gamesList);

        public static void ChangeSelectedGame(SourceGame newSelectedGame) {
            selectedGame = newSelectedGame;
        }

        public static SourceGame getByName(string name) {
            foreach(SourceGame game in gamesList) {
                if (game.name.ToLower() == name.ToLower()) return game;
            }
            return null;
        }
    }


    public class SourceGame {

        private string _dir;
        private string _cfgDir;
        private string _libDir;

        public string name { get; }
        public string dir {
            get { return PathManager.steamApps + _dir; }
            set { _dir = value; }
        }
        public string cfgDir {
            get { return PathManager.steamApps + _dir + _cfgDir; }
            set { _cfgDir = value; }
        }
        public string libDir {
            get { return PathManager.steamApps + _dir + _libDir; }
            set { _libDir = value; }
        }
        public string exe { get; }
        public int id { get; }

        public SourceGame(string name, string dir, string cfgDir, string libDir, string exe, int id) {
            this.name = name;
            this.dir = dir;
            this.cfgDir = cfgDir;
            this.libDir = libDir;
            this.exe = exe;
            this.id = id;
        }

        public void launchGame() {
            Process.Start(new ProcessStartInfo("steam://rungameid/" + id));
        }
    }
}

using System;

namespace physical {
    public class main {
        public const string APP_NAME = "Physical";

        main () {
        }

        [STAThread]
        public static void Main () {
            using ( PhysicalWindow example = new PhysicalWindow() ) {
                example.Title = APP_NAME + " v0.0";
                example.Run( 30 );
            }
        }
    }
}


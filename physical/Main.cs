using System;

namespace physical {
    public class main {
        [STAThread]
        public static void Main () {
            using ( PhysicalWindow example = new PhysicalWindow() ) {
                example.Run( 60 );
            }
        }

        main () {
        }
    }
}


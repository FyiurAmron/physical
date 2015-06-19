using System;

namespace physical.math {
    public class MathUtils {
        static public readonly Random RNG = new Random();

        private MathUtils () {
            throw new InvalidOperationException();
        }

        static public float nextFloat () {
            return (float) RNG.NextDouble();
        }

        static public float nextFloat ( float min, float max ) {
            return (float) RNG.NextDouble() * ( max - min ) + min;
        }

        static public int floatToIntBits ( float f ) {
            return BitConverter.ToInt32( BitConverter.GetBytes( f ), 0 );
        }

        static public long doubleToLongBits ( double d ) {
            return BitConverter.DoubleToInt64Bits( d );
        }
    }
}


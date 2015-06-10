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
    }
}


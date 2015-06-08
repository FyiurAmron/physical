using System;

namespace physical.math {
    public class VectorFloat : Vector<float> {
        public VectorFloat ( int requiredSize ) : base( requiredSize ) {
        }

        public VectorFloat ( float[] data ) : base( data ) {
        }

        public VectorFloat ( float[] data, int requiredSize ) : base( data, requiredSize ) {
        }

        public float lengthSq () {
            float lenSum = 0;
            for ( int i = data.Length - 1; i >= 0; i-- )
                lenSum += data[i];
            return lenSum;
        }

        public float length () {
            return (float)Math.Sqrt( lengthSq() );
        }

        public void normalize () {
            float lenFactor = 1.0f / length();
            for ( int i = data.Length - 1; i >= 0; i-- )
                data[i] *= lenFactor;
            //return this;
        }
    }
}


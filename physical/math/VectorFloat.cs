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
            return lengthSq( data );
        }

        public float length () {
            return length( data );
        }

        public void normalize () {
            normalize( data );
        }

        // static part

        static public float lengthSq ( float[] data ) {
            float lenSum = 0;
            for ( int i = data.Length - 1; i >= 0; i-- )
                lenSum += data[i];
            return lenSum;
        }

        static public float length ( float[] data ) {
            return (float) Math.Sqrt( lengthSq( data ) );
        }

        static public void normalize ( float[] data ) {
            float lenFactor = 1.0f / length( data );
            for ( int i = data.Length - 1; i >= 0; i-- )
                data[i] *= lenFactor;
            //return this;
        }

        static public float dot ( float[] v1, float[] v2 ) { // simple regardless of dimension count
            int len = v1.Length;
            float sum = 0;
            for ( int i = 0; i < len; i++ )
                sum += v1[i] * v2[i];
            return sum;
        }

        static public void scale ( float[] v, float f ) {
            for ( int i = v.Length - 1; i >= 0; i-- )
                v[i] *= f;
        }

        static public float[] getScaled ( float[] v, float f ) {
            int len = v.Length;
            float[] ret = new float[len];
            for ( len--; len >= 0; len-- )
                ret[len] = v[len] * f;
            return ret;
        }

        static public void add ( float[] v, float[] t ) { // translate with t
            for ( int i = v.Length - 1; i >= 0; i-- )
                v[i] += t[i];
        }

        static public void subtract ( float[] v, float[] t ) { // translate with -t
            for ( int i = v.Length - 1; i >= 0; i-- )
                v[i] -= t[i];
        }

        static public float[] getSum ( float[] v, float[] t ) {
            int len = v.Length;
            float[] ret = new float[len];
            for ( len--; len >= 0; len-- )
                ret[len] = v[len] + t[len];
            return ret;
        }

        static public float[] getDiff ( float[] v, float[] t ) {
            int len = v.Length;
            float[] ret = new float[len];
            for ( len--; len >= 0; len-- )
                ret[len] = v[len] - t[len];
            return ret;
        }
    }
}


using System;

namespace physical.math {
    public class VectorFloat : Vector<float> {
        public VectorFloat ( int requiredSize ) : base( requiredSize ) {
        }

        public VectorFloat ( float[] data ) : base( data ) {
        }

        public VectorFloat ( float[] data, int requiredSize ) : base( data, requiredSize ) {
        }

        public VectorFloat ( VectorFloat vector ) : base( vector ) {
        }

        /*
        public static VectorFloat operator + ( VectorFloat v1, VectorFloat v2 ) {
            add( v1.data, v2.data );
            return v1;
        }

        public static VectorFloat operator - ( VectorFloat v1, VectorFloat v2 ) {
            subtract( v1.data, v2.data );
            return v1;
        }
        */
        /*
        public static VectorFloat operator * ( VectorFloat v1, VectorFloat v2 ) {
            dot( v1.data, v2.data );
            return v1;
        }
        */
        /*
        public static VectorFloat operator * ( VectorFloat v1, float f ) {
            scale( v1.data, f );
            return v1;
        }

        public static VectorFloat operator * ( float f, VectorFloat v1 ) {
            scale( v1.data, f );
            return v1;
        }
        */

        public VectorFloat add ( VectorFloat vector ) {
            add( data, vector.data );
            return this;
        }

        public VectorFloat subtract ( VectorFloat vector ) {
            subtract( data, vector.data );
            return this;
        }

        public VectorFloat scale ( float scaler ) {
            scale( data, scaler );
            return this;
        }

        public VectorFloat getSum ( VectorFloat vector ) {
            return new VectorFloat( getSum( data, vector.data ) );
        }

        public VectorFloat getDiff ( VectorFloat vector ) {
            return new VectorFloat( getDiff( data, vector.data ) );
        }

        public VectorFloat getScaled ( float scaler ) {
            return new VectorFloat( getScaled( data, scaler ) );
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

        static public float[] scale ( float[] v, float f ) {
            for ( int i = v.Length - 1; i >= 0; i-- )
                v[i] *= f;
            return v;
        }

        static public float[] scale ( float[] v, float[] f ) {
            for ( int i = v.Length - 1; i >= 0; i-- )
                v[i] *= f[i];
            return v;
        }

        static public float[] getScaled ( float[] v, float f ) {
            int i = v.Length;
            float[] ret = new float[i];
            for ( i--; i >= 0; i-- )
                ret[i] = v[i] * f;
            return ret;
        }

        static public float[] getScaled ( float[] v, float[] f ) {
            int i = v.Length;
            float[] ret = new float[i];
            for ( i--; i >= 0; i-- )
                ret[i] = v[i] * f[i];
            return ret;
        }

        static public float[] add ( float[] v, float[] t ) { // translate with t
            for ( int i = v.Length - 1; i >= 0; i-- )
                v[i] += t[i];
            return v;
        }

        static public float[] subtract ( float[] v, float[] t ) { // translate with -t
            for ( int i = v.Length - 1; i >= 0; i-- )
                v[i] -= t[i];
            return v;
        }

        static public float[] getSum ( float[] v, float[] t ) {
            int len = v.Length;
            float[] ret = new float[len];
            for ( len--; len >= 0; len-- )
                ret[len] = v[len] + t[len];
            return ret;
        }

        static public float[] getDiff ( float[] v, float[] t ) {
            int i = v.Length;
            float[] ret = new float[i];
            for ( i--; i >= 0; i-- )
                ret[i] = v[i] - t[i];
            return ret;
        }
    }
}


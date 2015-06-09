using System;

namespace physical.math {
    public class Vector3f : VectorFloat {
        public const int SIZE = 3;

        public float X { get { return Data[0]; } set { Data[0] = value; } }

        public float Y { get { return Data[1]; } set { Data[1] = value; } }

        public float Z { get { return Data[2]; } set { Data[2] = value; } }

        public Vector3f ( Vector3f vector ) : base( vector ) {
        }

        public Vector3f ( float[] data ) : base( data, SIZE ) {
        }

        public Vector3f () : base( new float[SIZE] ) {
        }

        public Vector3f ( float x, float y, float z ) : base( new float[]{ x, y, z } ) {
        }

        public Vector3f set ( float x, float y, float z ) {
            X = x;
            Y = y;
            Z = z;
            return this;
        }

        public Vector3f scale ( float x, float y, float z ) {
            X *= x;
            Y *= y;
            Z *= z;
            return this;
        }

        public Vector3f cross ( Vector3f vector ) {
            return new Vector3f( cross( data, vector.data ) );
        }

        public Vector3f getNormal ( Vector3f v1, Vector3f v2 ) {
            return new Vector3f( getNormal( data, v1.data, v2.data ) );
        }

        static public float[] cross ( float[] v1, float[] v2 ) {
            return new float[] {
                v1[1] * v2[2] - v1[2] * v2[1],
                v1[2] * v2[0] - v1[0] * v2[2],
                v1[0] * v2[1] - v1[1] * v2[0]
            };
        }

        static public float[] getNormal ( float[] v1, float[] v2, float[] v3 ) {
            float[] norm = cross(
                               new float[]{ v2[0] - v1[0], v2[1] - v1[1], v2[2] - v1[2] },
                               new float[]{ v3[0] - v1[0], v3[1] - v1[1], v3[2] - v1[2] } );
            normalize( norm );
            return norm;
        }
    }
}


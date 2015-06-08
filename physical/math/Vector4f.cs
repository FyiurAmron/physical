using System;

namespace physical.math {
    public class Vector4f : VectorFloat {
        public const int SIZE = 4;

        public float X { get { return Data[0]; } set { Data[0] = value; } }

        public float Y { get { return Data[1]; } set { Data[1] = value; } }

        public float Z { get { return Data[2]; } set { Data[2] = value; } }

        public float W { get { return Data[3]; } set { Data[3] = value; } }

        public Vector4f ( float[] data ) : base( data, SIZE ) {
        }

        public Vector4f () : this( new float[SIZE] ) {
        }

        public Vector4f ( float x, float y, float z, float w ) : this( new float[]{ x, y, z, w } ) {
        }

        public Vector4f set ( float x, float y, float z, float w ) {
            X = x;
            Y = y;
            Z = z;
            W = w;
            return this;
        }
    }
}


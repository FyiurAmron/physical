using System;

namespace physical.math {
    public class Vector3f : VectorFloat {
        public const int SIZE = 3;

        public float X { get { return Data[0]; } set { Data[0] = value; } }

        public float Y { get { return Data[1]; } set { Data[1] = value; } }

        public float Z { get { return Data[2]; } set { Data[2] = value; } }

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
    }
}


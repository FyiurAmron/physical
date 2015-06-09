

namespace physical.math {
    public class Vector2f : VectorFloat {
        public const int SIZE = 2;

        public float X { get { return Data[0]; } set { Data[0] = value; } }

        public float Y { get { return Data[1]; } set { Data[1] = value; } }

        public Vector2f ( Vector2f vector ) : base( vector ) {
        }

        public Vector2f ( float[] data ) : base( data, SIZE ) {
        }

        public Vector2f () : base( new float[SIZE] ) {
        }

        public Vector2f ( float x, float y ) : base( new float[]{ x, y } ) {
        }

        public Vector2f set ( float x, float y ) {
            X = x;
            Y = y;
            return this;
        }
    }
}


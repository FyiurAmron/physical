using System;

using OpenTK;

namespace physical.math {
    public class Matrix4f : Vector<float> {
        public const int SIZE = 4 * 4;
        public static readonly Matrix4f IDENTITY = new Matrix4f( true );

        public Matrix4f () : base( new float[SIZE] ) {
        }

        public Matrix4f ( bool identity ) : this() {
            if ( identity ) {
                data[0] = 1;
                data[5] = 1;
                data[10] = 1;
                data[15] = 1;
            }
        }

        public Matrix4f ( float[] data ) : base( data, SIZE ) {
        }

        public void setIdentity () {
            data[0] = 1;
            data[1] = 0;
            data[2] = 0;
            data[3] = 0;

            data[4] = 0;
            data[5] = 1;
            data[6] = 0;
            data[7] = 0;

            data[8] = 0;
            data[9] = 0;
            data[10] = 1;
            data[11] = 0;

            data[12] = 0;
            data[13] = 0;
            data[14] = 0;
            data[15] = 1;
        }

        public void setTranslation ( float x, float y, float z ) {
            data[12] = x;
            data[13] = y;
            data[14] = z;
        }

        public void setTranslation ( Vector3f vector3f ) {
            setTranslation( vector3f.X, vector3f.Y, vector3f.Z );
        }

        public void setTranslationX ( float x ) {
            data[12] = x;
        }

        public void setTranslationY ( float y ) {
            data[13] = y;
        }

        public void setTranslationZ ( float z ) {
            data[14] = z;
        }

        public void setValue ( float value ) {
            for ( int i = data.Length - 1; i >= 0; i-- )
                data[i] = value;
        }

        public void setZero () {
            setValue( 0 );
        }


        public void set ( Matrix4 matrix4 ) {
            data[0] = matrix4.M11;
            data[1] = matrix4.M12;
            data[2] = matrix4.M13;
            data[3] = matrix4.M14;

            data[4] = matrix4.M21;
            data[5] = matrix4.M22;
            data[6] = matrix4.M23;
            data[7] = matrix4.M24;

            data[8] = matrix4.M31;
            data[9] = matrix4.M32;
            data[10] = matrix4.M33;
            data[11] = matrix4.M34;

            data[12] = matrix4.M41;
            data[13] = matrix4.M42;
            data[14] = matrix4.M43;
            data[15] = matrix4.M44;
        }

        public Matrix4 toMatrix4 () {
            return new Matrix4(
                data[0], data[1], data[2], data[3],
                data[4], data[5], data[6], data[7],
                data[8], data[9], data[10], data[11],
                data[12], data[13], data[14], data[15]
            );
        }
    }
}


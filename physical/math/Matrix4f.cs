using System;

using OpenTK;

namespace physical.math {
    public class Matrix4f {
        float[] data;

        public float[] Data { get { return data; } }

        public Matrix4f () : this( new float[4 * 4] ) {
        }

        public Matrix4f ( float[] data ) {
            this.data = data;
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


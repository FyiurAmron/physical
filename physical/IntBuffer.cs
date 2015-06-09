using System;

namespace physical {
        public class FloatBuffer {
            int position;
            float[] array;

            public FloatBuffer ( float[] array ) {
                this.array = array;
            }

            public int put ( float value ) {
                array[position] = value;
                position++; // return counter++;
                return position;
            }

            public float get () {
                float f = array[position]; // return array[counter++];
                position++;
                return f;
            }

            public void rewind () {
                position = 0;
            }

            public void offset ( int positionOffset ) {
                position += positionOffset;
            }

            public int available () {
                return array.Length - position;
            }
        }

}


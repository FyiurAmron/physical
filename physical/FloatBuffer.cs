using System;

namespace physical {
    public class FloatBuffer {
        int counter;
        float[] array;

        public FloatBuffer ( float[] array ) {
            this.array = array;
        }

        public float get () {
            float f = array[counter]; // return array[counter++];
            counter++;
            return f;
        }

        public void rewind () {
            counter = 0;
        }

        public int available () {
            return array.Length - counter;
        }
    }
}


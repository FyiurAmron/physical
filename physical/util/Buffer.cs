using System;

namespace physical.util {
    public class Buffer<T> {
        int position;
        T[] array;

        public int Size{ get; set; }

        public T[] Array { get { return array; } }

        public Buffer ( params T[] array ) {
            this.array = array;
            Size = array.Length;
        }

        public Buffer ( int size ) {
            this.array = new T[size];
            Size = size;
        }

        void checkPosition () {
            if ( position >= Size )
                throw new InvalidOperationException( "position >= Size" );
        }

        public int put ( T value ) {
            checkPosition();
            array[position] = value;
            position++; // return counter++;
            return position;
        }

        public T get () {
            T f = array[position]; // return array[counter++];
            position++;
            return f;
        }

        public void rewind () {
            position = 0;
        }

        public void offset ( int positionOffset ) {
            position += positionOffset;
            checkPosition();
        }

        public int available () {
            return array.Length - position;
        }
    }
}


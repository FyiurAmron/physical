using System;

namespace physical.math {
    public class Vector<T> {
        protected readonly T[] data;

        public T[] Data { get { return data; } }

        public Vector ( int requiredSize ) : this( new T[requiredSize] ) {
        }

        public Vector ( T[] data ) {
            this.data = data;
        }

        public Vector ( T[] data, int requiredSize ) {
            if ( data.Length < requiredSize )
                throw new InvalidOperationException();
            this.data = data;
        }

        public Vector ( Vector<T> vector ) : this( vector.Data.Length ) {
            set( vector );
        }

        public void set ( T[] data ) {
            for ( int i = Math.Min( this.data.Length, data.Length ) - 1; i >= 0; i-- )
                this.data[i] = data[i];
        }

        public void set ( Vector<T> vector ) {
            T[] src = vector.data;
            for ( int i = Math.Min( data.Length, vector.data.Length ) - 1; i >= 0; i-- )
                data[i] = src[i];
        }

        public void rotate ( int offset ) {
            if ( offset == 0 )
                return;
            int absOffset = Math.Abs( offset ), len2 = data.Length - absOffset;
            T[] tmp = new T[absOffset];
            if ( offset > 0 ) {
                System.Array.Copy( data, len2, tmp, 0, absOffset );
                System.Array.Copy( data, 0, data, absOffset, len2 );
                System.Array.Copy( tmp, 0, data, 0, absOffset );
            } else {
                System.Array.Copy( data, 0, tmp, 0, absOffset );
                System.Array.Copy( data, absOffset, data, 0, len2 );
                System.Array.Copy( tmp, 0, data, len2, absOffset );
            }
            foreach ( var item in data )
                Console.Write( item.ToString() + " " );
            Console.WriteLine();
        }
    }
}


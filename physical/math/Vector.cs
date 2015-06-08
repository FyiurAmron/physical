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
    }
}


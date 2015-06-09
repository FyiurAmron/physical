/**

 @author poponuro
 */
using System;
using biz.ritter.javapi.lang;

namespace physical.util {
    abstract public class ArrayCompiler {
        protected int pos, len;

        protected ArrayCompiler ( int len ) {
            this.len = len;
        }

        public int getPos () {
            return pos;
        }

        protected void checkPos () {
            if ( pos < 0 || pos >= len )
                throw new IndexOutOfRangeException( ""+pos );
        }

        public void setPos ( int pos ) {
            this.pos = pos;
            checkPos();
        }

        public void movePos ( int offset ) {
            pos += offset;
            checkPos();
        }

        public class bytes : ArrayCompiler {
            protected byte[] arr;
            protected bool littleEndian;

            public bytes ( int len ) : base(len) {
                arr = new byte[len];
            }

            public void setEndianess ( bool is_little_endian ) {
                littleEndian = is_little_endian;
            }

            public void put ( byte b ) {
                arr[pos] = b;
                pos++;
            }

            public void put ( byte b, params byte[] bs ) {
                arr[pos] = b;
                pos++;
                put( bs );
            }

            public void put ( byte[] v ) {
//      if ( pos + v.length > len )
//        throw new IllegalStateException( "array buffer overflow; v.length == " + v.length + " ; pos == " + pos + " ; len == " + len );
                System.Array.Copy( v, 0, arr, pos, v.Length );
                pos += v.Length;
            }

            public void put ( long l ) {
                put( l, 8 );
            }

            public void put ( int i ) {
                put( i, 4 );
            }

            public void put ( short s ) {
                put( s, 2 );
            }

            public void put ( float f ) {
                put( Float.floatToIntBits( f ) );
            }

            public void put ( double d ) {
                put( biz.ritter.javapi.lang.Double.doubleToLongBits( d ) );
            }

            public void put ( long l, int bytes ) { // bytes - significant bytes (counted from right)
                int ppos = littleEndian ? pos + bytes : pos;
                for ( int i = 0; i < bytes; i++, l >>= 8 ) {
                    arr[ppos] = (byte) l; // cast implies & 0xFF
                    if ( littleEndian )
                        ppos--;
                    else
                        ppos++;
                }
                pos += bytes;
            }

            public byte[] get_array () {
                return arr;
            }

            public byte[] compile () {
                if ( pos != len )
                    throw new InvalidOperationException( "the array hasn't been filled with data yet; pos == " + pos + " ; len == " + len );
                return arr;
            }
        }

        public class floats : ArrayCompiler {
            protected float[] arr;

            public floats ( int len ) :base(len){
                arr = new float[len];
            }

            public void put ( float f ) {
                arr[pos] = f;
                pos++;
            }

            public void put ( float f, params float[] fs ) {
                arr[pos] = f;
                pos++;
                put( fs );
            }

            public void put ( float[] fs ) {
//      if ( pos + fs.length > len )
//        throw new IllegalStateException( "array buffer overflow; fs.length == " + fs.length + " ; pos == " + pos + " ; len == " + len );
                System.Array.Copy( fs, 0, arr, pos, fs.Length );
                pos += fs.Length;
            }

            public float[] get_array () {
                return arr;
            }

            public float[] compile () {
                if ( pos != len )// {
                    throw new InvalidOperationException( "the array hasn't been filled with data yet; pos == " + pos + " ; len == " + len );
                //float[] ret = new float[pos];
                //System.arraycopy( arr, 0, ret, 0, pos );
                //return ret;
                //}
                return arr;
            }
        }
    }
}
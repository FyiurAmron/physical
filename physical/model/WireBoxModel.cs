using System;
using OpenTK.Graphics.OpenGL;
using physical.util;

namespace physical.model {
    public class WireBoxModel : Model {
        protected static readonly Buffer<int> box_indices_buf
            = new Buffer<int>( 0, 1, 0, 2, 0, 4, 1, 3, 1, 5, 2, 3, 2, 6, 3, 7, 4, 5, 4, 6, 5, 7, 6, 7 );
        protected const int OPT_X = 2, OPT_Y = 2, OPT_Z = 2;

        static protected ModelData buildWireBox ( float x, float y, float z ) {
            Buffer<float> fb = new Buffer<float>( OPT_X * OPT_Y * OPT_Z * VERTEX_COUNT );
            for ( int i = 0; i < OPT_X; i++ )
                for ( int j = 0; j < OPT_Y; j++ )
                    for ( int k = 0; k < OPT_Z; k++ ) {
                        fb.put( i * x );
                        fb.put( j * y );
                        fb.put( k * z );
                    }
            fb.rewind();
            return new ModelData( fb, new Buffer<float>( fb.Size ), new Buffer<float>( fb.Size ), box_indices_buf );
        }

        public WireBoxModel ( float size_x, float size_y, float size_z ) : base( buildWireBox( size_x, size_y, size_z ) ) {
            primitiveType = PrimitiveType.Lines;
        }

        public WireBoxModel ( float[] sizes )
            : this( sizes[0], sizes[1], sizes[2] ) {
        }
    }
}


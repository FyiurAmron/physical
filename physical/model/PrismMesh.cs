
using physical.math;
using physical.util;

namespace physical.model {
    public class PrismMesh : Mesh {
        const int PRISM_VERTEX_COUNT = ( 2 + 3 * 2 ) * Mesh.VERTEX_COUNT;
        // top/bottom + 3 sides (2 tri each)

        public PrismMesh ( float[][] ps, float height ) : base( buildPrism( ps, height ) ) {
        }

        public PrismMesh ( float[] p1, float[] p2, float[] p3, float height ) :
            base( buildPrism( new float[][] { p1, p2, p3 }, height ) ) {
        }

        public static MeshData buildPrism ( float[][] p, float height ) { // p = float[3][3]; // three vert., three coords
            float[][] r = new float[Mesh.VERTEX_COUNT][];
            for ( int i = 0; i < Mesh.VERTEX_COUNT; i++ )
                r[i] = new float[Mesh.V_DIMS];

            float[] base_norm = Vector3f.getNormal( p[0], p[1], p[2] ),
            rev_norm = { -base_norm[0], -base_norm[1], -base_norm[2] };

            for ( int i = 0; i < Mesh.VERTEX_COUNT; i++ )
                for ( int j = 0; j < Mesh.V_DIMS; j++ )
                    r[i][j] = p[i][j] + rev_norm[j] * height; // since rev normal is set to 'inside', it goes up from the base

            ArrayCompiler.Floats vx = new ArrayCompiler.Floats( Mesh.V_DIMS * PRISM_VERTEX_COUNT ),
            vn = new ArrayCompiler.Floats( Mesh.VN_DIMS * PRISM_VERTEX_COUNT ),
            vt = new ArrayCompiler.Floats( Mesh.VT_DIMS * PRISM_VERTEX_COUNT );

            for ( int i = 0; i < Mesh.VERTEX_COUNT; i++ ) {
                vx.put( p[i] );
                vn.put( base_norm );
            }
            vt.put( Mesh.TRI_VT_PROTO );

            float[] n;
            for ( int i = 0, j = 1; i < 3; i++, j++ ) {
                if ( j == 3 )
                    j = 0;

                vx.put( p[i] );
                vx.put( r[i] );
                vx.put( p[j] );

                vx.put( p[j] );
                vx.put( r[i] );
                vx.put( r[j] );

                n = Vector3f.getNormal( p[i], r[i], p[j] );
                for ( int k = 0; k < 2 * Mesh.VERTEX_COUNT; k++ )
                    vn.put( n );

                vt.put( RECT_VT_PROTO );
            }

            for ( int i = Mesh.VERTEX_COUNT - 1; i >= 0; i-- ) { // reverses winding
                vx.put( r[i] );
                vn.put( rev_norm );
            }
            vt.put( Mesh.TRI_VT_PROTO );
            return new MeshData( vx.compile(), vn.compile(), vt.compile() );
        }
    }
}


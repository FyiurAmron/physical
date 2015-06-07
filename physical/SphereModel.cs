using System;

namespace Physical {
    public class SphereModel {
        protected int 
            VERTEX_COUNT = 3, // because we use triangles
            V_DIMS = 3, VN_DIMS = 3, VT_DIMS = 2, // for reference for other implementing classes
            VS_COUNT = 3;
        // v, vn, vt; 3 total.
        /** &lt; 0.5 creates distortion on last valid stack, &gt; 0.5 generates unnecessary 'sewing' on it */
        static protected float CAP_RATIO = 0.5f,
            PI = (float) Math.PI, TWO_PI = 2 * PI;

        protected float radius, ds;
        protected int slices, stacks, nsign;
        protected float[] m_s_theta, c_theta;
        protected float rho, t;
        protected float[][][][] v_cache;
        protected ArrayCompiler.floats[] fac;

        protected void swap_cache () {
            float[][][] tmp = v_cache[0];
            v_cache[0] = v_cache[1];
            v_cache[1] = tmp;
        }

        protected void fac_put ( params float[][][] vs ) {
            if ( vs != null )
                foreach ( float[][] fv in vs )
                    for ( int i = 0; i < VS_COUNT; i++ )
                        fac[i].put( fv[i] );
        }

        protected void set_cache () {
            float s_rho = (float) Math.Sin( rho ), c_rho = (float) Math.Cos( rho ),
            s = 0,
            x, y, z;
            for ( int j = 0; j <= slices; j++, s += ds ) {
                z = m_s_theta[j] * s_rho;
                y = c_rho * nsign;
                x = c_theta[j] * s_rho;
                float[][] fArr = new float[3][] {
                    new float[]{ x * radius, y * radius, z * radius },
                    new float[]{ x * nsign, y * nsign, z * nsign },
                    new float[]{ s, t }
                };
                v_cache[1][j] = fArr;
            }
        }

        public SphereModel ( float radius, int slices, int stacks, bool outside ) {
            if ( stacks < 2 || slices < 2 )
                throw new InvalidOperationException( "stacks < 2 || slices < 2" );
            this.radius = radius;
            this.slices = slices;
            this.stacks = stacks;

            int ops = ( stacks + 1 ) * slices * 6; // stacks +1 for 2 'caps' with half tris at each sphere's 'end'
            fac = new ArrayCompiler.floats[] {
                new ArrayCompiler.floats( ops * V_DIMS ),
                new ArrayCompiler.floats( ops * VN_DIMS ),
                new ArrayCompiler.floats( ops * VT_DIMS )
            };
            nsign = outside ? 1 : -1;

            m_s_theta = new float[slices + 1];
            c_theta = new float[slices + 1];
            float theta = 0, dtheta = TWO_PI / slices;
            for ( int j = 0; j <= slices; j++, theta += dtheta ) {
                m_s_theta[j] = -(float) Math.Sin( theta );
                c_theta[j] = (float) Math.Cos( theta );
            }
            v_cache = new float[2][][][];
            for ( int i = 0; i < 2; i++ )
                v_cache[i] = new float[slices + 1][][];
            ds = 1.0f / slices;
        }

        public float[][] prepare () { // note: the caps are constructed differently vs GLU (due to CAP_RATIO)
            float drho = PI / stacks, dt = 1.0f / stacks, s, half_ds = ds / 2;
            rho = drho * CAP_RATIO;
            t = dt * CAP_RATIO; // this is reversed vs GLU, since we draw top-to-bottom (correct alignment)

            set_cache(); // cap 1
            s = half_ds; // pushed to the middle to make the trifan smoother; use s = 0 for legacy (GLU) rendering
            for ( int j = 0; j < slices; j++, s += ds )
                fac_put(
                    v_cache[1][j + 1],
                    new float[][] {
                        new float[]{ 0, radius * nsign, 0 },
                        new float[] { 0, nsign, 0 },
                        new float[]{ s, 0 }
                    },
                    v_cache[1][j] );
            swap_cache();
            rho = drho;
            t = dt;

            for ( int i = 1; i < stacks; i++, rho += drho, t += dt ) {
                set_cache();
                for ( int j = 0; j < slices; j++ )
                    fac_put( v_cache[0][j], v_cache[1][j], v_cache[0][j + 1],
                        v_cache[1][j + 1], v_cache[0][j + 1], v_cache[1][j] );
                swap_cache();
            }

            rho = PI - drho * CAP_RATIO;
            t = 1 - dt * CAP_RATIO;
            set_cache();
            for ( int j = 0; j < slices; j++ )
                fac_put( v_cache[0][j], v_cache[1][j], v_cache[0][j + 1],
                    v_cache[1][j + 1], v_cache[0][j + 1], v_cache[1][j] );
            swap_cache();

            s = half_ds; // cap 2
            for ( int j = 0; j < slices; j++, s += ds ) // last slice with tip
                    fac_put(
                    v_cache[0][j],
                    new float[][] {
                        new float[]{ 0, -radius * nsign, 0 },
                        new float[]{ 0, -nsign, 0 },
                        new float[]{ s, 1 }
                    },
                    v_cache[0][j + 1] );

            return new float[][]{ fac[0].compile(), fac[1].compile(), fac[2].compile() };
        }
    }
}


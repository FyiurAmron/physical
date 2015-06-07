using System;

namespace physical {
    public class SphereModel : Model {
        public SphereModel ( float radius, int slices, int stacks, bool outside ) : base( SphereBuilder.build( radius, slices, stacks, outside ) ) {
        }
    }

    public class SphereBuilder {
        /** &lt; 0.5 creates distortion on last valid stack, &gt; 0.5 generates unnecessary 'sewing' on it */
        protected const float CAP_RATIO = 0.5f;

        float ds;
        int nsign;
        float[] msTheta, cTheta;
        float rho, t;
        float[][][][] vCache;

        float radius;
        int slices;
        //int stacks;
        //bool outside;
        ArrayCompiler.floats[] fac;

        protected void swapCache () {
            float[][][] tmp = vCache[0];
            vCache[0] = vCache[1];
            vCache[1] = tmp;
        }

        protected void putFAC ( params float[][][] vs ) {
            if ( vs != null )
                foreach ( float[][] fv in vs )
                    for ( int i = 0; i < Model.VS_COUNT; i++ )
                        fac[i].put( fv[i] );
        }

        protected void setCache () {
            float s_rho = (float) Math.Sin( rho ), c_rho = (float) Math.Cos( rho ),
            s = 0,
            x, y, z;
            for ( int j = 0; j <= slices; j++, s += ds ) {
                z = msTheta[j] * s_rho;
                y = c_rho * nsign;
                x = cTheta[j] * s_rho;
                float[][] fArr = new float[3][] {
                    new float[]{ x * radius, y * radius, z * radius },
                    new float[]{ x * nsign, y * nsign, z * nsign },
                    new float[]{ s, t }
                };
                vCache[1][j] = fArr;
            }
        }

        protected ModelData buildFloats ( float radius, int slices, int stacks, bool outside ) {
            this.radius = radius;
            this.slices = slices;
            if ( stacks < 2 || slices < 2 )
                throw new InvalidOperationException( "stacks < 2 || slices < 2" );

            nsign = outside ? 1 : -1;
            // note: the caps are constructed differently vs GLU (due to CAP_RATIO)

            msTheta = new float[slices + 1];
            cTheta = new float[slices + 1];
            float theta = 0, dtheta = Model.TWO_PI / slices;
            for ( int j = 0; j <= slices; j++, theta += dtheta ) {
                msTheta[j] = -(float) Math.Sin( theta );
                cTheta[j] = (float) Math.Cos( theta );
            }
            vCache = new float[2][][][];
            for ( int i = 0; i < 2; i++ )
                vCache[i] = new float[slices + 1][][];
            ds = 1.0f / slices;

            int ops = ( stacks + 1 ) * slices * 6; // stacks +1 for 2 'caps' with half tris at each sphere's 'end'
            fac = new ArrayCompiler.floats[] {
                new ArrayCompiler.floats( ops * Model.V_DIMS ),
                new ArrayCompiler.floats( ops * Model.VN_DIMS ),
                new ArrayCompiler.floats( ops * Model.VT_DIMS )
            };

            float drho = Model.PI / stacks, dt = 1.0f / stacks, s, half_ds = ds / 2;
            rho = drho * CAP_RATIO;
            t = dt * CAP_RATIO; // this is reversed vs GLU, since we draw top-to-bottom (correct alignment)

            setCache(); // cap 1
            s = half_ds; // pushed to the middle to make the trifan smoother; use s = 0 for legacy (GLU) rendering
            for ( int j = 0; j < slices; j++, s += ds )
                putFAC( 
                    vCache[1][j + 1],
                    new float[][] {
                        new float[]{ 0, radius * nsign, 0 },
                        new float[] { 0, nsign, 0 },
                        new float[]{ s, 0 }
                    },
                    vCache[1][j] );
            swapCache();
            rho = drho;
            t = dt;

            for ( int i = 1; i < stacks; i++, rho += drho, t += dt ) {
                setCache();
                for ( int j = 0; j < slices; j++ )
                    putFAC(
                        vCache[0][j], vCache[1][j], vCache[0][j + 1],
                        vCache[1][j + 1], vCache[0][j + 1], vCache[1][j] );
                swapCache();
            }

            rho = Model.PI - drho * CAP_RATIO;
            t = 1 - dt * CAP_RATIO;
            setCache();
            for ( int j = 0; j < slices; j++ )
                putFAC( 
                    vCache[0][j], vCache[1][j], vCache[0][j + 1],
                    vCache[1][j + 1], vCache[0][j + 1], vCache[1][j] );
            swapCache();

            s = half_ds; // cap 2
            for ( int j = 0; j < slices; j++, s += ds ) // last slice with tip
                putFAC(
                    vCache[0][j],
                    new float[][] {
                        new float[]{ 0, -radius * nsign, 0 },
                        new float[]{ 0, -nsign, 0 },
                        new float[]{ s, 1 }
                    },
                    vCache[0][j + 1] );
            return new ModelData( fac[0].compile(), fac[1].compile(), fac[2].compile() );
            // TODO optimize indices
        }

        public static ModelData build ( float radius, int slices, int stacks, bool outside ) {
            return new SphereBuilder().buildFloats( radius, slices, stacks, outside );
        }
    }
}


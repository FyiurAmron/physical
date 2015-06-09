

namespace physical.physics {
    public class SphereBody : Body {
        readonly float radius;

        public SphereBody ( float mass, float radius ) : base( mass ) {
            this.radius = radius;
        }
    }
}


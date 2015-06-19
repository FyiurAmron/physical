using physical.math;

namespace physical.physics {
    public class PlaneBody : Body {
        public Plane3f Plane3f { get; set; }

        public PlaneBody ( Plane3f plane3f ) {
            this.Plane3f = plane3f;
        }

        public PlaneBody () : this( new Plane3f() ) {
        }

        public PlaneBody ( Vector3f normal, float distanceToOrigin ) : this( new Plane3f( normal, distanceToOrigin ) ) {
        }
    }
}


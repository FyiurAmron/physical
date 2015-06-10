using physical.math;

namespace physical.physics {
    public class PlaneBody : Body {
        public Plane3f Plane3f { get; set; }

        public PlaneBody ( Plane3f plane3f ) {
            this.Plane3f = plane3f;
            FixedPosition = true;
        }

        public PlaneBody () : this( new Plane3f() ) {
        }

        public PlaneBody ( Vector3f normal, float distanceToOrigin ) : this( new Plane3f( normal, distanceToOrigin ) ) {
        }

        override public void checkCollision ( Body body ) {
            if ( body.FixedPosition )
                return;
            body.checkCollision( this ); // no sensible code available here
        }
    }
}


using System;

namespace physical.physics {
    public class SphereBody : Body {
        readonly float radius;

        public float Restitution { set; get; }

        public SphereBody ( float mass, float radius ) : base( mass ) {
            this.radius = radius;
            Restitution = 0.5f;
        }

        override public void checkCollision ( Body body ) {
            if ( body is SphereBody ) {
                SphereBody sb = (SphereBody) body;
            } else if ( body is PlaneBody ) {
                PlaneBody pb = (PlaneBody) body;
                if ( Position.Y < radius ) { // temporary; TODO: implement point-to-plane here
                    Position.Y = radius;
                    Velocity.Y *= -Restitution; // TODO: apply restitution factor
                }
            } else
                throw new ArgumentException();
        }
    }
}


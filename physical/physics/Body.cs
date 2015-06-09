using System;
using physical.math;

namespace physical.physics {
    public class Body {
        public float Mass { get; set; }

        Vector3f
            position,
            velocity = new Vector3f(),
            acceleration = new Vector3f();

        protected Body () : this( float.PositiveInfinity ) {
        }

        public Body ( float mass ) {
            Mass = mass;
        }

        public Body ( float mass, Vector3f initialPosition ) : this( mass ) {
            position = new Vector3f( initialPosition );
        }

        public void applyForce ( Vector3f force ) {
            acceleration.add( force.getScaled( 1 / Mass ) );
        }

        public void applyImpulse ( Vector3f impulse ) {
            velocity.add( impulse.getScaled( 1 / Mass ) );
        }

        public void timeStep ( float deltaT ) {
            velocity.add( acceleration.getScaled( 1 / deltaT ) );
            position.add( velocity.getScaled( 1 / deltaT ) );
        }
    }
}


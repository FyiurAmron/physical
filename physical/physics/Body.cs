using System;
using System.Collections.Generic;

using physical.math;

namespace physical.physics {
    public abstract class Body {
        public float Mass { get; set; }

        Vector3f
            position,
            velocity = new Vector3f(),
            acceleration = new Vector3f(),
            oldPosition = new Vector3f();

        public Vector3f Position { get { return position; } set { position.set( value ); } }

        public Vector3f Velocity { get { return velocity; } set { velocity.set( value ); } }

        public Vector3f Acceleration { get { return acceleration; } set { acceleration.set( value ); } }

        public bool FixedPosition { get; set; }
        /*
        List<Action<Vector3f,Vector3f>> positionConstraints = new List<Action<Vector3f,Vector3f>>();

        public List<Action<Vector3f,Vector3f>> PositionConstraints { get { return positionConstraints; } }
*/
        protected Body () : this( float.PositiveInfinity ) {
        }

        public Body ( float mass ) {
            Mass = mass;
            position = new Vector3f();
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
            oldPosition.set( position );
            velocity.add( acceleration.getScaled( deltaT ) );
            position.add( velocity.getScaled( deltaT ) );
            acceleration.setZero();
            /*
            foreach ( Action<Vector3f,Vector3f> positionConstraint in positionConstraints )
                positionConstraint( oldPosition, position );
                */
        }

        abstract public void checkCollision ( Body body );
    }
}


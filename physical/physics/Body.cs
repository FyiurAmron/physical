using System;
using System.Collections.Generic;

using physical.math;

namespace physical.physics {
    public abstract class Body {
        protected const float KINEMATIC_EPSILON = 1E-3f;

        public float Mass { get; set; }

        //Matrix4f transform;

        Vector3f
            //position,
            //oldPosition = new Vector3f(),
            velocity = new Vector3f(),
            acceleration = new Vector3f();

        public float Restitution { get; set; }

        //public Vector3f Position { get { return position; } set { position.set( value ); } }

        public Matrix4f Transform { get; set; }

        public Vector3f Velocity { get { return velocity; } set { velocity.set( value ); } }

        public Vector3f Acceleration { get { return acceleration; } set { acceleration.set( value ); } }

        public bool FixedPosition { get; set; }
        /*
        List<Action<Vector3f,Vector3f>> positionConstraints = new List<Action<Vector3f,Vector3f>>();

        public List<Action<Vector3f,Vector3f>> PositionConstraints { get { return positionConstraints; } }
*/

        public Body ( float mass ) {
            Mass = mass;
            Transform = new Matrix4f( true );
            Restitution = 1.0f;
        }

        protected Body () : this( float.PositiveInfinity ) {
        }
        /*
        public Body ( float mass, Vector3f initialPosition ) : this( mass ) {
            position = new Vector3f( initialPosition );
        }*/

        public Body ( float mass, Matrix4f initialTransform ) : this( mass ) {
            Transform = new Matrix4f( initialTransform );
        }

        // instance methods

        public void applyForce ( Vector3f force ) {
            acceleration.add( force.getScaled( 1 / Mass ) );
        }

        public void applyImpulse ( Vector3f impulse ) {
            velocity.add( impulse.getScaled( 1 / Mass ) );
        }

        virtual public void timeStep ( float deltaT ) {
            //oldPosition.set( position );
            velocity.add( acceleration.getScaled( deltaT ) );
            //position.add( velocity.getScaled( deltaT ) );
            Transform.addTranslation( velocity.getScaled( deltaT ) );
            acceleration.setZero();
            if ( velocity.lengthSq() < KINEMATIC_EPSILON )
                velocity.setZero();
            /*
            foreach ( Action<Vector3f,Vector3f> positionConstraint in positionConstraints )
                positionConstraint( oldPosition, position );
                */
        }

        abstract public void checkCollision ( Body body );
    }
}


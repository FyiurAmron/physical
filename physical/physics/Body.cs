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

        //public bool FixedPosition { get; set; } // use mass = float.PositiveInfinity instead

        List<Action<Matrix4f,Vector3f,Vector3f>> constraints = new List<Action<Matrix4f,Vector3f,Vector3f>>();

        public List<Action<Matrix4f,Vector3f,Vector3f>> Constraints { get { return constraints; } }

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

        public void applyForce ( float forceX, float forceY, float forceZ ) {
            float scale = 1f / Mass;
            acceleration.add( forceX * scale, forceY * scale, forceZ * scale );
        }

        public void applyForce ( Vector3f force ) {
            acceleration.add( force.getScaled( 1 / Mass ) );
        }

        public void applyImpulse ( float velocityX, float velocityY, float velocityZ ) {
            float scale = 1f / Mass;
            velocity.add( velocityX * scale, velocityY * scale, velocityZ * scale );
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
            
            foreach ( Action<Matrix4f,Vector3f,Vector3f> constraint in constraints )
                constraint( Transform, velocity, acceleration );
        }
    }
}


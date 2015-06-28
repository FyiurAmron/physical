using System;
using System.Collections.Generic;

using physical.math;

namespace physical.physics {
    public abstract class Body {
        public const float KINEMATIC_EPSILON_SQ = 1E-2f;

        public float Mass { get; set; }

        //Matrix4f transform;

        Vector3f
            //position,
            //oldPosition = new Vector3f(),
            velocity = new Vector3f(),
            acceleration = new Vector3f();

        public float Restitution { get; set; }

        public float Friction { get; set; }

        //public Vector3f Position { get { return position; } set { position.set( value ); } }

        public Matrix4f Transform { get; set; }

        public Vector3f Velocity { get { return velocity; } set { velocity.set( value ); } }

        protected internal Vector3f Acceleration { get { return acceleration; } set { acceleration.set( value ); } }

        //public bool FixedPosition { get; set; } // use mass = float.PositiveInfinity instead

        List<Action<Body>> constraints = new List<Action<Body>>();
        List<Action<Body>> forces = new List<Action<Body>>();

        public List<Action<Body>> Forces { get { return forces; } }

        public List<Action<Body>> Constraints { get { return constraints; } }

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

        protected internal Vector3f getForce () {
            Vector3f v3f = new Vector3f( Acceleration );
            v3f.scale( Mass );
            return v3f;
        }

        virtual public void timeStep ( float deltaT ) {
            foreach ( Action<Body> force in forces )
                force( this );
            
            //oldPosition.set( position );
            velocity.add( acceleration.getScaled( deltaT ) );
            //position.add( velocity.getScaled( deltaT ) );
            if ( velocity.lengthSq() < KINEMATIC_EPSILON_SQ )
                velocity.setZero();
            else
                Transform.addTranslation( velocity.getScaled( deltaT ) );
            acceleration.setZero();

            foreach ( Action<Body> constraint in constraints )
                constraint( this );
        }
    }
}


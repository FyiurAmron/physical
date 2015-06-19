
using System.Collections.Generic;
using physical.math;
using physical.model;
using System;

namespace physical.physics {
    public class BodyManager {
        List<Body> bodies = new List<Body>();
        HashSet<Body> bodySet = new HashSet<Body>();
        Dictionary<Body,Mesh> bodyMeshMap = new Dictionary<Body, Mesh>();
        Vector3f gravity = new Vector3f( 0, -9.81f, 0 );
        Dictionary<ColliderDescriptor,Collider/*<Body,Body>*/> colliderMap = new Dictionary<ColliderDescriptor, Collider/*<Body,Body>*/>();

        public Vector3f Gravity { get { return gravity; } set { gravity.set( value ); } }

        public BodyManager () {
            addCollider( new SphereSphereCollider() );
            addCollider( new SpherePlaneCollider() );
        }

        void addCollider ( Collider/*<Body,Body>*/ collider ) {
            colliderMap.Add( collider.getDescriptor(), collider );
        }

        public void addBody ( Body body ) {
            if ( bodySet.Contains( body ) )
                throw new ArgumentException( "body already added" );
            bodies.Add( body );
            bodySet.Add( body );
        }

        public void addBody ( Body body, Mesh mesh ) {
            addBody( body );
            mesh.Transform.set( body.Transform );
            bodyMeshMap.Add( body, mesh );
        }

        public void update ( float deltaT ) {
            for ( int i = bodies.Count - 1; i >= 0; i-- ) {
                Body body1 = bodies[i];
                for ( int j = i - 1; j >= 0; j-- ) {
                    Body body2 = bodies[j];
                    Type t1 = body1.GetType(), t2 = body2.GetType();
                    Collider /*<Body,Body>*/ collider;

                    ColliderDescriptor cd = new ColliderDescriptor( t1, t2 );
                    colliderMap.TryGetValue( cd, out collider );
                    if ( collider != null ) {
                        if ( collider.collide( body1, body2 ) ) {
                            Console.WriteLine( "collision: " + t1 + " [" + i + "] vs " + t2 + " [" + j + "]" );
                        }
                    } else {
                        cd = new ColliderDescriptor( t2, t1 );
                        colliderMap.TryGetValue( cd, out collider );
                        if ( collider == null ) {
                            //throw new InvalidOperationException( "unsupported collision: " + t1 + " vs " + t2 );
                        } else if ( collider.collide( body2, body1 ) ) {
                            Console.WriteLine( "collision: " + t2 + " [" + j + "] vs " + t1 + " [" + i + "]" );
                        }
                    }
                }
                if ( body1.Mass == float.PositiveInfinity )
                    continue;
                body1.Acceleration.add( gravity );
                body1.timeStep( deltaT );

                if ( bodyMeshMap.ContainsKey( body1 ) )
                    bodyMeshMap[body1].Transform.set( body1.Transform );
            }
        }
    }
}


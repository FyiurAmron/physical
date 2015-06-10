
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

        public Vector3f Gravity { get { return gravity; } set { gravity.set( value ); } }


        public BodyManager () {
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
                    body1.checkCollision( bodies[j] );
                }
                if ( body1.FixedPosition )
                    continue;
                body1.Acceleration.add( gravity );
                body1.timeStep( deltaT );

                if ( bodyMeshMap.ContainsKey( body1 ) )
                    bodyMeshMap[body1].Transform.set( body1.Transform );
            }
        }
    }
}



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
        Dictionary<Body,HashSet<Body>> contactMap = new Dictionary<Body, HashSet<Body>>();

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

        void addContact ( Body b1, Body b2 ) {
            HashSet<Body> contacts;
            contactMap.TryGetValue( b1, out contacts );
            if ( contacts == null ) {
                contacts = new HashSet<Body>();
                contactMap.Add( b1, contacts );
            }
            contacts.Add( b2 );
            contactMap.TryGetValue( b2, out contacts );
            if ( contacts == null ) {
                contacts = new HashSet<Body>();
                contactMap.Add( b2, contacts );
            }
            contacts.Add( b1 );
        }

        void removeContact ( Body b1, Body b2 ) {
            HashSet<Body> contacts;
            contactMap.TryGetValue( b1, out contacts );
            if ( contacts != null ) {
                contacts.Remove( b2 );
            }
            contactMap.TryGetValue( b2, out contacts );
            if ( contacts != null ) {
                contacts.Remove( b1 );
            }
        }

        bool hasContact ( Body b1, Body b2 ) {
            HashSet<Body> contacts;
            contactMap.TryGetValue( b1, out contacts );
            if ( contacts != null && contacts.Contains( b2 ) )
                return true;
            contactMap.TryGetValue( b2, out contacts );
            return contacts != null && contacts.Contains( b1 );
        }

        void collide ( Collider collider, bool contact, Body body1, Body body2, int i, int j ) {
            if ( collider.collide( body1, body2 ) ) {
                if ( contact ) {
                    //Console.WriteLine( "contact continued: " + body1 + " [" + i + "] vs " + body2 + " [" + j + "]" );
                } else {
                    //Console.WriteLine( "contact started: " + body1 + " [" + i + "] vs " + body2 + " [" + j + "]" );
                    addContact( body1, body2 );
                }
            } else if ( contact ) {
                //Console.WriteLine( "contact ended: " + body1 + " [" + i + "] vs " + body2 + " [" + j + "]" );
                removeContact( body1, body2 );
            }
        }

        public void update ( float deltaT ) {
            //Console.WriteLine( "update; deltaT = " + deltaT );
            for ( int i = bodies.Count - 1; i >= 0; i-- ) {
                Body body1 = bodies[i];
                if ( body1.Mass != float.PositiveInfinity ) {
                    body1.Acceleration.add( gravity );
                }
                for ( int j = i - 1; j >= 0; j-- ) {
                    Body body2 = bodies[j];
                    bool contact = hasContact( body1, body2 );
                    Type t1 = body1.GetType(), t2 = body2.GetType();
                    Collider /*<Body,Body>*/ collider;

                    ColliderDescriptor cd = new ColliderDescriptor( t1, t2 );
                    colliderMap.TryGetValue( cd, out collider );
                    if ( collider != null ) {
                        collide( collider, contact, body1, body2, i, j );
                        continue;
                    } 
                    cd = new ColliderDescriptor( t2, t1 );
                    colliderMap.TryGetValue( cd, out collider );
                    if ( collider == null ) {
                        //throw new InvalidOperationException( "unsupported collision: " + t1 + " vs " + t2 );
                    } else {
                        collide( collider, contact, body2, body1, i, j );
                    }
                }
                if ( body1.Mass != float.PositiveInfinity ) {
                    body1.timeStep( deltaT );
                    //Console.WriteLine( "position: " + body1.Transform.TranslationX + "," + body1.Transform.TranslationY + "," + body1.Transform.TranslationZ );

                    if ( bodyMeshMap.ContainsKey( body1 ) )
                        bodyMeshMap[body1].Transform.set( body1.Transform );
                }
            }
            //Console.WriteLine( "updated" );
        }

        // end of class BodyManager
    }
}


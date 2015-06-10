

namespace physical.physics {
    public class PlaneBody : Body {
        public PlaneBody () {
            // temporarily assume it's a n=(0,1,0), o=(0,0,0) plane
            FixedPosition = true;
        }

        override public void checkCollision ( Body body ) {
            body.checkCollision( this ); // no sensible code available here
        }
    }
}


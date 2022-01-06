using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

namespace Frontend.Scripts
{
    public class RWheelCollider : MonoBehaviour
    {
        #region REGION - Private variables

        private GameObject wheel;
        private Rigidbody rigidBody;
        private float wheelMass = 1f;
        private float wheelRadius = 0.5f;
        private float suspensionLength = 1f;
        private float suspensionSpring = 10f;
        private float suspensionSpringExp = 0f;
        private float suspensionDamper = 2f;
        private float suspensionForceOffset = 0f;
        private float currentFwdFrictionCoef = 1f;
        private float currentSideFrictionCoef = 1f;
        private float currentSurfaceFrictionCoef = 1f;
        private float currentSteeringAngle = 0f;
        private float currentMotorTorque = 0f;
        private float currentBrakeTorque = 0f;
        private float currentMomentOfInertia = 1.0f * 0.5f * 0.5f * 0.5f;//moment of inertia of wheel; used for mass in acceleration calculations regarding wheel angular velocity.  MOI of a solid cylinder = ((m*r*r)/2)
        private int currentRaycastMask = ~(1 << 26);
        private WheelFrictionCurve fwdFrictionCurve = new WheelFrictionCurve(0.06f, 1.2f, 0.065f, 1.25f, 0.7f);
        private WheelFrictionCurve sideFrictionCurve = new WheelFrictionCurve(0.03f, 1.0f, 0.04f, 1.05f, 0.7f);
        private bool suspensionNormalForce = false;

        private Vector3 gravity = new Vector3(0, -9.81f, 0);

        private Vector3 gNorm = new Vector3(0, -1, 0);
        private Action<Vector3> onImpactCallback;

        private float extSpringForce = 0f;
        private Vector3 extHitPoint = Vector3.zero;
        private Vector3 extHitNorm = Vector3.up;


        private float rollingResistanceCoefficient = 0.005f;//tire-deformation based rolling-resistance; scaled by spring force, is a flat force that will be subtracted from wheel velocity every tick
        private float rotationalResistanceCoefficient = 0f;//bearing/friction based resistance; scaled by wheel rpm and 1/10 spring force


        private bool grounded = false;

        private float inertiaInverse;
        private float radiusInverse;

        private float prevFLong = 0f;
        private float prevFLat = 0f;
        private float prevFSpring;
        private float currentSuspensionCompression = 0f;
        private float prevSuspensionCompression = 0f;
        private float currentAngularVelocity = 0f;
        private float vSpring;
        private float fDamp;

        private Vector3 wF, wR;
        private Vector3 wheelUp;
        private Vector3 wheelForward;
        private Vector3 wheelRight;
        private Vector3 localVelocity;
        private Vector3 localForce;
        private float vWheel;
        private float vWheelDelta;
        private float sLong;
        private float sLat;
        private Vector3 hitPoint;
        private Vector3 hitNormal;
        private Collider hitCollider;

        #endregion ENDREGION - Private variables

        #region REGION - Public accessible API get/set methods


        public Rigidbody Rigidbody
        {
            get { return rigidBody; }
            set { rigidBody = value; }
        }

     
        public float spring
        {
            get { return suspensionSpring; }
            set { suspensionSpring = value; }
        }

        public float springCurve
        {
            get { return suspensionSpringExp; }
            set { suspensionSpringExp = value; }
        }

        public float damper
        {
            get { return suspensionDamper; }
            set { suspensionDamper = value; }
        }

        public float length
        {
            get { return suspensionLength; }
            set { suspensionLength = value; }
        }

        public float mass
        {
            get { return wheelMass; }
            set
            {
                wheelMass = value;
                currentMomentOfInertia = wheelMass * wheelRadius * wheelRadius * 0.5f;
                inertiaInverse = 1.0f / currentMomentOfInertia;
            }
        }
        public float radius
        {
            get { return wheelRadius; }
            set
            {
                wheelRadius = value;
                currentMomentOfInertia = wheelMass * wheelRadius * wheelRadius * 0.5f;
                inertiaInverse = 1.0f / currentMomentOfInertia;
                radiusInverse = 1.0f / wheelRadius;
            }
        }

        public float forceApplicationOffset
        {
            get { return suspensionForceOffset; }
            set { suspensionForceOffset = Mathf.Clamp01(value); }
        }

        public WheelFrictionCurve forwardFrictionCurve
        {
            get { return fwdFrictionCurve; }
            set { if (value != null) { fwdFrictionCurve = value; } }
        }
        public WheelFrictionCurve sidewaysFrictionCurve
        {
            get { return sideFrictionCurve; }
            set { if (value != null) { sideFrictionCurve = value; } }
        }

        public float forwardFrictionCoefficient
        {
            get { return currentFwdFrictionCoef; }
            set { currentFwdFrictionCoef = value; }
        }

        public float sideFrictionCoefficient
        {
            get { return currentSideFrictionCoef; }
            set { currentSideFrictionCoef = value; }
        }

        public float surfaceFrictionCoefficient
        {
            get { return currentSurfaceFrictionCoef; }
            set { currentSurfaceFrictionCoef = value; }
        }

        public float rollingResistance
        {
            get { return rollingResistanceCoefficient; }
            set { rollingResistanceCoefficient = value; }
        }

        public float rotationalResistance
        {
            get { return rotationalResistanceCoefficient; }
            set { rotationalResistanceCoefficient = value; }
        }

        public float brakeTorque
        {
            get { return currentBrakeTorque; }
            set { currentBrakeTorque = Mathf.Abs(value); }
        }

        public float motorTorque
        {
            get { return currentMotorTorque; }
            set { currentMotorTorque = value; }
        }

        public float steeringAngle
        {
            get { return currentSteeringAngle; }
            set { currentSteeringAngle = value; }
        }

        public Vector3 gravityVector
        {
            get { return gravity; }
            set { gravity = value; gNorm = gravity.normalized; }
        }


        public bool useSuspensionNormal
        {
            get { return suspensionNormalForce; }
            set { suspensionNormalForce = value; }
        }

        public void setImpactCallback(Action<Vector3> callback)
        {
            onImpactCallback = callback;
        }

        public bool isGrounded
        {
            get { return grounded; }
        }

        public float rpm
        {
            // wWheel / (pi*2) * 60f
            // all values converted to combined constants
            get { return currentAngularVelocity * 9.549296585f; }
            set { currentAngularVelocity = value * 0.104719755f; }
        }

        public float angularVelocity
        {
            get { return currentAngularVelocity; }
            set { currentAngularVelocity = value; }
        }

        public float linearVelocity
        {
            get { return currentAngularVelocity * wheelRadius; }
        }

        public float compressionDistance
        {
            get { return currentSuspensionCompression; }
        }

        public int raycastMask
        {
            get { return currentRaycastMask; }
            set { currentRaycastMask = value; }
        }

        public float perFrameRotation
        {
            // returns rpm * 0.16666_ * 360f * secondsPerFrame
            // degrees per frame = (rpm / 60) * 360 * secondsPerFrame
            get { return rpm * 6 * Time.deltaTime; }
        }

        public float externalSpringForce
        {
            get { return extSpringForce; }
            set { extSpringForce = value; }
        }

        public Vector3 externalHitPoint
        {
            get { return extHitPoint; }
            set { extHitPoint = value; }
        }

        public Vector3 externalHitNormal
        {
            get { return extHitNorm; }
            set { extHitNorm = value; }
        }

        public float momentOfInertia
        {
            get { return currentMomentOfInertia; }
        }

        public float springForce
        {
            get { return localForce.y + extSpringForce; }
        }
        public float dampForce
        {
            get { return fDamp; }
        }

        public float longitudinalForce
        {
            get { return localForce.z; }
        }
        public float lateralForce
        {
            get { return localForce.x; }
        }
        public float longitudinalSlip
        {
            get { return sLong; }
        }
        public float lateralSlip
        {
            get { return sLat; }
        }
        public Vector3 wheelLocalVelocity
        {
            get { return localVelocity; }
        }
        public Collider contactColliderHit
        {
            get { return hitCollider; }
        }

        public Vector3 contactNormal
        {
            get { return hitNormal; }
        }
        public Vector3 worldHitPos
        {
            get { return wheel.transform.position - wheel.transform.up * (suspensionLength - currentSuspensionCompression + wheelRadius); }
        }

        #endregion ENDREGION - Public accessible methods, API get/set methods

        #region REGION - Update methods -- internal, external


        public void UpdateWheel()
        {
            if (rigidBody == null)
            {
                return;
            }
            if (this.wheel == null) { this.wheel = this.gameObject; }
            wheelForward = Quaternion.AngleAxis(currentSteeringAngle, wheel.transform.up) * wheel.transform.forward;
            wheelUp = wheel.transform.up;
            wheelRight = -Vector3.Cross(wheelForward, wheelUp);
            prevSuspensionCompression = currentSuspensionCompression;
            prevFSpring = localForce.y;
            bool prevGrounded = grounded;
            if (CheckSuspensionCompression())//suspension compression is calculated in the suspension contact check
            {
                //surprisingly, this seems to work extremely well...
                //there will be the 'undefined' case where hitNormal==wheelForward (hitting a vertical wall)
                //but that collision would never be detected anyway, as well as the suspension force would be undefined/uncalculated
                wR = Vector3.Cross(hitNormal, wheelForward);
                wF = -Vector3.Cross(hitNormal, wR);

                wF = wheelForward - hitNormal * Vector3.Dot(wheelForward, hitNormal);
                wR = Vector3.Cross(hitNormal, wF);
                //wR = wheelRight - hitNormal * Vector3.Dot(wheelRight, hitNormal);


                //no idea if this is 'proper' for transforming velocity from world-space to wheel-space; but it seems to return the right results
                //the 'other' way to do it would be to construct a quaternion for the wheel-space rotation transform and multiple
                // vqLocal = qRotation * vqWorld * qRotationInverse;
                // where vqWorld is a quaternion with a vector component of the world velocity and w==0
                // the output being a quaternion with vector component of the local velocity and w==0
                Vector3 worldVelocityAtHit = rigidBody.GetPointVelocity(hitPoint);
                if (hitCollider != null && hitCollider.attachedRigidbody != null)
                {
                    worldVelocityAtHit -= hitCollider.attachedRigidbody.GetPointVelocity(hitPoint);
                }
                float mag = worldVelocityAtHit.magnitude;
                localVelocity.z = Vector3.Dot(worldVelocityAtHit.normalized, wF) * mag;
                localVelocity.x = Vector3.Dot(worldVelocityAtHit.normalized, wR) * mag;
                localVelocity.y = Vector3.Dot(worldVelocityAtHit.normalized, hitNormal) * mag;

                calcSpring();
                integrateForces();
                if (!prevGrounded && onImpactCallback != null)//if was not previously grounded, call-back with impact data; we really only know the impact velocity
                {
                    onImpactCallback.Invoke(localVelocity);
                }
            }
            else
            {
                integrateUngroundedTorques();
                grounded = false;
                vSpring = prevFSpring = fDamp = prevSuspensionCompression = currentSuspensionCompression = 0;
                localForce = Vector3.zero;
                hitNormal = Vector3.zero;
                hitPoint = Vector3.zero;
                hitCollider = null;
                localVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Should be called whenever the wheel collider is disabled -- clears out internal state data from the previous wheel hit
        /// </summary>
        public void clearGroundedState()
        {
            grounded = false;
            vSpring = prevFSpring = fDamp = prevSuspensionCompression = currentSuspensionCompression = 0;
            localForce = Vector3.zero;
            hitNormal = Vector3.up;
            hitPoint = Vector3.zero;
            localVelocity = Vector3.zero;
            hitCollider = null;
        }

        #endregion ENDREGION - Update methods -- internal, external

        #region REGION - Private/internal update methods

        /// <summary>
        /// Integrate the torques and forces for a grounded wheel, using the pre-calculated fSpring downforce value.
        /// </summary>
        private void integrateForces()
        {
            calcFrictionStandard();
            //anti-jitter handling code; if lateral or long forces are oscillating, damp them on the rebound
            //could possibly even zero them out for the rebound, but this method allows for some force
            float fMult = 0.1f;
            if ((prevFLong < 0 && localForce.z > 0) || (prevFLong > 0 && localForce.z < 0))
            {
                localForce.z *= fMult;
            }
            if ((prevFLat < 0 && localForce.x > 0) || (prevFLat > 0 && localForce.x < 0))
            {
                localForce.x *= fMult;
            }
            Vector3 calculatedForces;
            if (suspensionNormalForce)
            {
                calculatedForces = wheel.transform.up * localForce.y;
            }
            else
            {
                calculatedForces = hitNormal * localForce.y;
                calculatedForces += calcAG(hitNormal, localForce.y);
            }
            calculatedForces += localForce.z * wF;
            calculatedForces += localForce.x * wR;
            Vector3 forcePoint = hitPoint;
            if (suspensionForceOffset > 0)
            {
                float offsetDist = suspensionLength - compressionDistance + wheelRadius;
                forcePoint = hitPoint + wheel.transform.up * (suspensionForceOffset * offsetDist);
            }
            rigidBody.AddForceAtPosition(calculatedForces, forcePoint, ForceMode.Force);
            if (hitCollider != null && hitCollider.attachedRigidbody != null && !hitCollider.attachedRigidbody.isKinematic)
            {
                hitCollider.attachedRigidbody.AddForceAtPosition(-calculatedForces, forcePoint, ForceMode.Force);
            }
            prevFLong = localForce.z;
            prevFLat = localForce.x;
        }

        /// <summary>
        /// Calculate an offset to the spring force that will negate the tendency to slide down hill caused by suspension forces.
        ///   Seems to mostly work and brings drift down to sub-milimeter-per-second.
        ///   Should be combined with some sort of spring/joint/constraint to complete the sticky-friction implementation.  
        /// </summary>
        /// <param name="hitNormal"></param>
        /// <param name="springForce"></param>
        /// <returns></returns>
        private Vector3 calcAG(Vector3 hitNormal, float springForce)
        {
            Vector3 agFix = new Vector3(0, 0, 0);
            // this is the amount of suspension force that is misaligning the vehicle
            // need to push uphill by this amount to keep the rigidbody centered along suspension axis
            float gravNormDot = Vector3.Dot(hitNormal, gNorm);
            //this force should be applied in the 'uphill' direction
            float agForce = gravNormDot * springForce;
            //calculate uphill direction from hitNorm and gNorm
            // cross of the two gives the left/right of the hill
            Vector3 hitGravCross = Vector3.Cross(hitNormal, gNorm);
            // cross the left/right with the hitNorm to derive the up/down-hill direction
            Vector3 upDown = Vector3.Cross(hitGravCross, hitNormal);
            // and pray that all the rhs/lhs coordinates are correct...
            float slopeLatDot = Vector3.Dot(upDown, wR);
            agFix = agForce * slopeLatDot * wR * Mathf.Clamp(currentSideFrictionCoef, 0, 1);
            float vel = Mathf.Abs(localVelocity.z);
            if (brakeTorque > 0 && Mathf.Abs(motorTorque) < brakeTorque && vel < 4)
            {
                float mult = 1f;
                if (vel > 2)
                {
                    //if between 2m/s and 4/ms, lerp output force between them
                    //zero ouput at or above 4m/s, max output at or below 2m/s, intermediate force output inbetween those values
                    vel -= 2;//clamp to range 0-2
                    vel *= 0.5f;//clamp to range 0-1
                    mult = 1 - vel;//invert to range 1-0; with 0 being for input velocity of 4
                }
                float slopeLongDot = Vector3.Dot(upDown, wF);
                agFix += agForce * slopeLongDot * wF * Mathf.Clamp(currentFwdFrictionCoef, 0, 1) * mult;
            }
            return agFix;
        }

        /// <summary>
        /// Integrate drive and brake torques into wheel velocity for when -not- grounded.
        /// This allows for wheels to change velocity from user input while the vehicle is not in contact with the surface.
        /// Not-yet-implemented are torques on the rigidbody due to wheel accelerations.
        /// </summary>
        private void integrateUngroundedTorques()
        {
            //velocity change due to motor; if brakes are engaged they can cancel this out the same tick
            //acceleration is in radians/second; only operating on fixedDeltaTime seconds, so only update for that length of time
            currentAngularVelocity += currentMotorTorque * inertiaInverse * Time.fixedDeltaTime;
            if (currentAngularVelocity != 0)
            {
                float rotationalDrag = rotationalResistanceCoefficient * currentAngularVelocity * inertiaInverse * Time.fixedDeltaTime;
                rotationalDrag = Mathf.Min(Mathf.Abs(rotationalDrag), Mathf.Abs(currentAngularVelocity)) * Mathf.Sign(currentAngularVelocity);
                currentAngularVelocity -= rotationalDrag;
            }
            if (currentAngularVelocity != 0)
            {
                // maximum torque exerted by brakes onto wheel this frame
                float wBrake = currentBrakeTorque * inertiaInverse * Time.fixedDeltaTime;
                // clamp the max brake angular change to the current angular velocity
                wBrake = Mathf.Min(Mathf.Abs(currentAngularVelocity), wBrake) * Mathf.Sign(currentAngularVelocity);
                // and finally, integrate it into wheel angular velocity
                currentAngularVelocity -= wBrake;
            }
        }

        private bool CheckSuspensionCompression()
        {
            RaycastHit hit;
            if (Physics.Raycast(wheel.transform.position, -wheel.transform.up, out hit, suspensionLength + wheelRadius, currentRaycastMask, QueryTriggerInteraction.Ignore))
            {
                currentSuspensionCompression = suspensionLength + wheelRadius - hit.distance;
                hitNormal = hit.normal;
                hitCollider = hit.collider;
                hitPoint = hit.point;
                grounded = true;
                return true;
            }
            grounded = false;
            return false;
        }



        //TODO config specified 'wheel width'
        //TODO config specified number of capsules
        /// <summary>
        /// less efficient and less optimal solution for skinny wheels, but avoids the edge cases caused by sphere colliders<para/>
        /// uses 2 capsule-casts in a V shape downward for the wheel instead of a sphere; 
        /// for some collisions the wheel may push into the surface slightly, up to about 1/3 radius.  
        /// Could be expanded to use more capsules at the cost of performance, but at increased collision fidelity, by simulating more 'edges' of a n-gon circle.  
        /// Sadly, unity lacks a collider-sweep function, or this could be a bit more efficient.
        /// </summary>
        /// <returns></returns>
       

        #region REGION - Friction model shared functions

        private void calcSpring()
        {
            //calculate damper force from the current compression velocity of the spring; damp force can be negative
            vSpring = (currentSuspensionCompression - prevSuspensionCompression) / Time.fixedDeltaTime;//per second velocity
            fDamp = suspensionDamper * vSpring;
            //calculate spring force basically from displacement * spring along with a secondary exponential term
            // k = xy + axy^2
            float fSpring = (suspensionSpring * currentSuspensionCompression) + (suspensionSpringExp * suspensionSpring * currentSuspensionCompression * currentSuspensionCompression);
            //integrate damper value into spring force
            fSpring += fDamp;
            //if final spring value is negative, zero it out; negative springs are not possible without attachment to the ground; gravity is our negative spring :)
            if (fSpring < 0) { fSpring = 0; }
            localForce.y = fSpring;
        }

        /// <summary>
        /// Returns a slip ratio between 0 and 1, 0 being no slip, 1 being lots of slip
        /// </summary>
        /// <param name="vLong"></param>
        /// <param name="vWheel"></param>
        /// <returns></returns>
        private float calcLongSlip(float vLong, float vWheel)
        {
            float sLong = 0;
            if (vLong == 0 && vWheel == 0) { return 0f; }//no slip present
            float a = Mathf.Max(vLong, vWheel);
            float b = Mathf.Min(vLong, vWheel);
            sLong = (a - b) / Mathf.Abs(a);
            sLong = Mathf.Clamp(sLong, 0, 1);
            return sLong;
        }

        /// <summary>
        /// Returns a slip ratio between 0 and 1, 0 being no slip, 1 being lots of slip
        /// </summary>
        /// <param name="vLong"></param>
        /// <param name="vLat"></param>
        /// <returns></returns>
        private float calcLatSlip(float vLong, float vLat)
        {
            float sLat = 0;
            if (vLat == 0)//vLat = 0, so there can be no sideways slip
            {
                return 0f;
            }
            else if (vLong == 0)//vLat!=0, but vLong==0, so all slip is sideways
            {
                return 1f;
            }
            sLat = Mathf.Abs(Mathf.Atan(vLat / vLong));//radians
            sLat = sLat * Mathf.Rad2Deg;//degrees
            sLat = sLat / 90f;//percentage (0 - 1)
            return sLat;
        }

        #endregion ENDREGION - Friction calculations methods based on alternate

        #region REGION - Standard Friction Model
        // based on : http://www.asawicki.info/Mirror/Car%20Physics%20for%20Games/Car%20Physics%20for%20Games.html

        public void calcFrictionStandard()
        {
            //initial motor/brake torque integration, brakes integrated further after friction applied
            //motor torque applied directly
            currentAngularVelocity += currentMotorTorque * inertiaInverse * Time.fixedDeltaTime;//acceleration is in radians/second; only operating on 1 * fixedDeltaTime seconds, so only update for that length of time

            //rolling resistance integration
            if (currentAngularVelocity != 0)
            {
                float fRollResist = localForce.y * rollingResistanceCoefficient;//rolling resistance force in newtons
                float tRollResist = fRollResist * wheelRadius;//rolling resistance as a torque
                float wRollResist = tRollResist * inertiaInverse * Time.fixedDeltaTime;//rolling resistance angular velocity change
                wRollResist = Mathf.Min(wRollResist, Mathf.Abs(currentAngularVelocity)) * Mathf.Sign(currentAngularVelocity);
                currentAngularVelocity -= wRollResist;
            }

            //rotational resistance integration
            if (currentAngularVelocity != 0)
            {
                //float fRotResist = currentAngularVelocity * rotationalResistanceCoefficient;
                //float tRotResist = fRotResist * radiusInverse;
                //float wRotResist = tRotResist * inertiaInverse * Time.fixedDeltaTime;
                //currentAngularVelocity -= wRotResist;
                currentAngularVelocity -= currentAngularVelocity * rotationalResistanceCoefficient * radiusInverse * inertiaInverse * Time.fixedDeltaTime;
            }

            // maximum torque exerted by brakes onto wheel this frame as a change in angular velocity
            float wBrakeMax = currentBrakeTorque * inertiaInverse * Time.fixedDeltaTime;
            // clamp the max brake angular change to the current angular velocity
            float wBrake = Mathf.Min(Mathf.Abs(currentAngularVelocity), wBrakeMax);
            // sign it opposite of current wheel spin direction
            // and finally, integrate it into wheel angular velocity
            currentAngularVelocity += wBrake * -Mathf.Sign(currentAngularVelocity);
            // this is the remaining brake angular acceleration/torque that can be used to counteract wheel acceleration caused by traction friction
            float wBrakeDelta = wBrakeMax - wBrake;

            vWheel = currentAngularVelocity * wheelRadius;
            sLong = calcLongSlip(localVelocity.z, vWheel);
            sLat = calcLatSlip(localVelocity.z, localVelocity.x);
            vWheelDelta = vWheel - localVelocity.z;

            float downforce = localForce.y + extSpringForce;
            float fLongMax = fwdFrictionCurve.evaluate(sLong) * downforce * currentFwdFrictionCoef * currentSurfaceFrictionCoef;
            float fLatMax = sideFrictionCurve.evaluate(sLat) * downforce * currentSideFrictionCoef * currentSurfaceFrictionCoef;
            // TODO - this should actually be limited by the amount of force necessary to arrest the velocity of this wheel in this frame
            // so limit max should be (abs(vLat) * sprungMass) / Time.fixedDeltaTime  (in newtons)
            localForce.x = fLatMax;
            // using current down-force as a 'sprung-mass' to attempt to limit overshoot when bringing the velocity to zero
            if (localForce.x > Mathf.Abs(localVelocity.x) * downforce) { localForce.x = Mathf.Abs(localVelocity.x) * downforce; }
            // if (fLat > sprungMass * Mathf.Abs(vLat) / Time.fixedDeltaTime) { fLat = sprungMass * Mathf.Abs(vLat) * Time.fixedDeltaTime; }
            localForce.x *= -Mathf.Sign(localVelocity.x);// sign it opposite to the current vLat

            //angular velocity delta between wheel and surface in radians per second; radius inverse used to avoid div operations
            float wDelta = vWheelDelta * radiusInverse;
            //amount of torque needed to bring wheel to surface speed over one second
            float tDelta = wDelta * currentMomentOfInertia;
            //newtons of force needed to bring wheel to surface speed over one second; radius inverse used to avoid div operations
            // float fDelta = tDelta * radiusInverse; // unused
            //absolute value of the torque needed to bring the wheel to road speed instantaneously/this frame
            float tTractMax = Mathf.Abs(tDelta) / Time.fixedDeltaTime;
            //newtons needed to bring wheel to ground velocity this frame; radius inverse used to avoid div operations
            float fTractMax = tTractMax * radiusInverse;
            //final maximum force value is the smallest of the two force values;
            // if fTractMax is used the wheel will be brought to surface velocity,
            // otherwise fLongMax is used and the wheel is still slipping but maximum traction force will be exerted
            fTractMax = Mathf.Min(fTractMax, fLongMax);
            // convert the clamped traction value into a torque value and apply to the wheel
            float tractionTorque = fTractMax * wheelRadius * -Mathf.Sign(vWheelDelta);
            // and set the longitudinal force to the force calculated for the wheel/surface torque
            localForce.z = fTractMax * Mathf.Sign(vWheelDelta);
            //use wheel inertia to determine final wheel acceleration from torques; inertia inverse used to avoid div operations; convert to delta-time, as accel is normally radians/s
            float angularAcceleration = tractionTorque * inertiaInverse * Time.fixedDeltaTime;
            //apply acceleration to wheel angular velocity
            currentAngularVelocity += angularAcceleration;
            //second integration pass of brakes, to allow for locked-wheels after friction calculation
            if (Mathf.Abs(currentAngularVelocity) < wBrakeDelta)
            {
                currentAngularVelocity = 0;
                wBrakeDelta -= Mathf.Abs(currentAngularVelocity);
                float fMax = Mathf.Max(0, Mathf.Abs(fLongMax) - Mathf.Abs(localForce.z));//remaining 'max' traction left
                float fMax2 = Mathf.Max(0, downforce * Mathf.Abs(localVelocity.z) - Mathf.Abs(localForce.z));
                float fBrakeMax = Mathf.Min(fMax, fMax2);
                localForce.z += fBrakeMax * -Mathf.Sign(localVelocity.z);
            }
            else
            {
                currentAngularVelocity += -Mathf.Sign(currentAngularVelocity) * wBrakeDelta;//traction from this will be applied next frame from wheel slip, but we're integrating here basically for rendering purposes
            }

            combinatorialFriction(fLatMax, fLongMax, localForce.x, localForce.z, out localForce.x, out localForce.z);
            //TODO technically wheel angular velocity integration should not occur until after the force is capped here, otherwise things will get out of synch
        }

        /// <summary>
        /// Simple and effective; limit their sum to the absolute maximum friction that the tire 
        /// can ever produce, as calculated by the (averaged=/) peak points of the friction curve. 
        /// This keeps the total friction output below the max of the tire while allowing the greatest range of optimal output for both lat and long friction.
        /// -Ideally- slip ratio would be brought into the calculation somewhere, but not sure how it should be used.
        /// </summary>
        private void combinatorialFriction(float latMax, float longMax, float fLat, float fLong, out float combLat, out float combLong)
        {
            float max = (fwdFrictionCurve.max + sideFrictionCurve.max) * 0.5f * (localForce.y + extSpringForce);
            float len = Mathf.Sqrt(fLat * fLat + fLong * fLong);
            if (len > max)
            {
                fLong /= len;
                fLat /= len;
                fLong *= max;
                fLat *= max;
            }
            combLat = fLat;
            combLong = fLong;
        }

        #endregion ENDREGION - Standard Friction Model


        public void drawDebug()
        {
            if (!grounded) { return; }

            Vector3 rayStart, rayEnd;
            Vector3 vOffset = rigidBody.velocity * Time.fixedDeltaTime;

            //draw the force-vector line
            rayStart = hitPoint;
            //because localForce isn't really a vector... its more 3 separate force-axis combinations...
            rayEnd = hitNormal * localForce.y;
            rayEnd += wR * localForce.x;
            rayEnd += wF * localForce.z;
            rayEnd += rayStart;

            //rayEnd = rayStart + wheel.transform.TransformVector(localForce.normalized) * 2f;
            Debug.DrawLine(rayStart + vOffset, rayEnd + vOffset, Color.magenta);

            rayStart += wheel.transform.up * 0.1f;
            rayEnd = rayStart + wF * 10f;
            Debug.DrawLine(rayStart + vOffset, rayEnd + vOffset, Color.blue);

            rayEnd = rayStart + wR * 10f;
            Debug.DrawLine(rayStart + vOffset, rayEnd + vOffset, Color.red);
        }

        #endregion ENDREGION - Private/internal update methods

    }
}

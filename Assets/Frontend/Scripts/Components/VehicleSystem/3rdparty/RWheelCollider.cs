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
        private Rigidbody rig;
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
        private WheelFrictionCurve fwdFrictionCurve = new WheelFrictionCurve(0.06f, 1.2f, 0.065f, 1.25f, 0.7f);
        private WheelFrictionCurve sideFrictionCurve = new WheelFrictionCurve(0.03f, 1.0f, 0.04f, 1.05f, 0.7f);


        private Vector3 gNorm = new Vector3(0, -1, 0);
        private float extSpringForce = 0f;

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

        #endregion ENDREGION - Private variables

        public Rigidbody Rigidbody
        {
            get { return rig; }
            set { rig = value; }
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

        public float compressionDistance
        {
            get { return currentSuspensionCompression; }
        }

        public float perFrameRotation
        {
            // returns rpm * 0.16666_ * 360f * secondsPerFrame
            // degrees per frame = (rpm / 60) * 360 * secondsPerFrame
            get { return rpm * 6 * Time.deltaTime; }
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
        public Vector3 contactNormal
        {
            get { return hitNormal; }
        }
        public Vector3 worldHitPos
        {
            get { return wheel.transform.position - wheel.transform.up * (suspensionLength - currentSuspensionCompression + wheelRadius); }
        }

        private void Start()
        {
            currentSuspensionCompression = suspensionLength + wheelRadius; 
        }
        private void FixedUpdate()
        {
            DrawDebug();
        }

        public void UpdateWheel()
        {
            if (rig == null) return;

            if (this.wheel == null) { this.wheel = this.gameObject; }

            wheelForward = Quaternion.AngleAxis(currentSteeringAngle, wheel.transform.up) * wheel.transform.forward;
            wheelUp = wheel.transform.up;
            wheelRight = -Vector3.Cross(wheelForward, wheelUp);
            
            prevSuspensionCompression = currentSuspensionCompression;
            prevFSpring = localForce.y;
            bool prevGrounded = grounded;
            if (CheckSuspensionCompression())
            {
                wR = Vector3.Cross(hitNormal, wheelForward);
                wF = -Vector3.Cross(hitNormal, wR);

                wF = wheelForward - hitNormal * Vector3.Dot(wheelForward, hitNormal);
                wR = Vector3.Cross(hitNormal, wF);

                Vector3 worldVelocityAtHit = rig.GetPointVelocity(hitPoint);
               
                float mag = worldVelocityAtHit.magnitude;
                localVelocity.z = Vector3.Dot(worldVelocityAtHit.normalized, wF) * mag;
                localVelocity.x = Vector3.Dot(worldVelocityAtHit.normalized, wR) * mag;
                localVelocity.y = Vector3.Dot(worldVelocityAtHit.normalized, hitNormal) * mag;

                CalculateSpring();
                IntegrateForces();
            }
            else
            {
                IntegrateUngroundedTorques();
                grounded = false;
                vSpring = prevFSpring = fDamp = prevSuspensionCompression = currentSuspensionCompression = 0;
                localForce = Vector3.zero;
                hitNormal = Vector3.zero;
                hitPoint = Vector3.zero;
                localVelocity = Vector3.zero;
            }
        }

        public void ClearGroundedState()
        {
            grounded = false;
            vSpring = prevFSpring = fDamp = prevSuspensionCompression = currentSuspensionCompression = 0;
            localForce = Vector3.zero;
            hitNormal = Vector3.up;
            hitPoint = Vector3.zero;
            localVelocity = Vector3.zero;
        }


        private void IntegrateForces()
        {
            CalcFrictionStandard();
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
            
            calculatedForces = hitNormal * localForce.y;
            calculatedForces += CalculateAG(hitNormal, localForce.y);
            
            
            calculatedForces += localForce.z * wF;
            calculatedForces += localForce.x * wR;
            Vector3 forcePoint = hitPoint;
            if (suspensionForceOffset > 0)
            {
                float offsetDist = suspensionLength - compressionDistance + wheelRadius;
                forcePoint = hitPoint + wheel.transform.up * (suspensionForceOffset * offsetDist);
            }
            rig.AddForceAtPosition(calculatedForces, forcePoint, ForceMode.Force);
           

            prevFLong = localForce.z;
            prevFLat = localForce.x;
           
            
                

        }
        private Vector3 CalculateAG(Vector3 hitNormal, float springForce)
        {
            Vector3 agFix = new Vector3(0f, 0f, 0f);
            float gravNormDot = Vector3.Dot(hitNormal, gNorm);
            float agForce = gravNormDot * springForce;
            Vector3 hitGravCross = Vector3.Cross(hitNormal, gNorm);

            Vector3 upDown = Vector3.Cross(hitGravCross, hitNormal);
            float slopeLatDot = Vector3.Dot(upDown, wR);
            agFix = agForce * Mathf.Clamp(currentSideFrictionCoef, 0f, 1f) * slopeLatDot * wR;
            float vel = Mathf.Abs(localVelocity.z);
            if (brakeTorque > 0f && Mathf.Abs(motorTorque) < brakeTorque && vel < 4f)
            {
                float mult = 1f;
                if (vel > 2f)
                {
                    vel -= 2f;
                    vel *= 0.5f;
                    mult = 1f - vel;
                }
                float slopeLongDot = Vector3.Dot(upDown, wF);
                agFix += agForce * Mathf.Clamp(currentFwdFrictionCoef, 0f, 1f) * mult * slopeLongDot * wF;
            }
            return agFix;
        }

        private void IntegrateUngroundedTorques()
        {
            currentAngularVelocity += currentMotorTorque * inertiaInverse * Time.fixedDeltaTime;
            if (currentAngularVelocity != 0)
            {
                float rotationalDrag = rotationalResistanceCoefficient * currentAngularVelocity * inertiaInverse * Time.fixedDeltaTime;
                rotationalDrag = Mathf.Min(Mathf.Abs(rotationalDrag), Mathf.Abs(currentAngularVelocity)) * Mathf.Sign(currentAngularVelocity);
                currentAngularVelocity -= rotationalDrag;
            }
            if (currentAngularVelocity != 0)
            {
                float wBrake = currentBrakeTorque * inertiaInverse * Time.fixedDeltaTime;
                wBrake = Mathf.Min(Mathf.Abs(currentAngularVelocity), wBrake) * Mathf.Sign(currentAngularVelocity);
                currentAngularVelocity -= wBrake;
            }
        }

        private bool CheckSuspensionCompression()
        {
            if (Physics.Raycast(wheel.transform.position, -wheel.transform.up, out RaycastHit hit, suspensionLength + wheelRadius))
            {
                currentSuspensionCompression = suspensionLength + wheelRadius - hit.distance;
                hitNormal = hit.normal;
                hitPoint = hit.point;
                grounded = true;
                return true;
            }
            grounded = false;
            return false;
        }



        private void CalculateSpring()
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

        private float CalcLongSlip(float vLong, float vWheel)
        {
            float sLong = 0;
            if (vLong == 0 && vWheel == 0) { return 0f; }//no slip present
            float a = Mathf.Max(vLong, vWheel);
            float b = Mathf.Min(vLong, vWheel);
            sLong = (a - b) / Mathf.Abs(a);
            sLong = Mathf.Clamp(sLong, 0, 1);
            return sLong;
        }

        private float CalcLatSlip(float vLong, float vLat)
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


        public void CalcFrictionStandard()
        {
            currentAngularVelocity += currentMotorTorque * inertiaInverse * Time.fixedDeltaTime;

            //rolling resistance integration
            if (currentAngularVelocity != 0)
            {
                float fRollResist = localForce.y * rollingResistanceCoefficient;
                float tRollResist = fRollResist * wheelRadius;
                float wRollResist = tRollResist * inertiaInverse * Time.fixedDeltaTime;
                wRollResist = Mathf.Min(wRollResist, Mathf.Abs(currentAngularVelocity)) * Mathf.Sign(currentAngularVelocity);
                currentAngularVelocity -= wRollResist;
            }

            if (currentAngularVelocity != 0)
            {
                currentAngularVelocity -= currentAngularVelocity * rotationalResistanceCoefficient * radiusInverse * inertiaInverse * Time.fixedDeltaTime;
            }

            float wBrakeMax = currentBrakeTorque * inertiaInverse * Time.fixedDeltaTime;
            float wBrake = Mathf.Min(Mathf.Abs(currentAngularVelocity), wBrakeMax);
            currentAngularVelocity += wBrake * -Mathf.Sign(currentAngularVelocity);
            float wBrakeDelta = wBrakeMax - wBrake;

            vWheel = currentAngularVelocity * wheelRadius;
            sLong = CalcLongSlip(localVelocity.z, vWheel);
            sLat = CalcLatSlip(localVelocity.z, localVelocity.x);
            vWheelDelta = vWheel - localVelocity.z;

            float downforce = localForce.y + extSpringForce;
            float fLongMax = fwdFrictionCurve.evaluate(sLong) * downforce * currentFwdFrictionCoef * currentSurfaceFrictionCoef;
            float fLatMax = sideFrictionCurve.evaluate(sLat) * downforce * currentSideFrictionCoef * currentSurfaceFrictionCoef;

            // TODO - this should actually be limited by the amount of force necessary to arrest the velocity of this wheel in this frame
            // so limit max should be (abs(vLat) * sprungMass) / Time.fixedDeltaTime  (in newtons)
            localForce.x = fLatMax;
            if (localForce.x > Mathf.Abs(localVelocity.x) * downforce) { localForce.x = Mathf.Abs(localVelocity.x) * downforce; }
            localForce.x *= -Mathf.Sign(localVelocity.x);

            float wDelta = vWheelDelta * radiusInverse;
            float tDelta = wDelta * currentMomentOfInertia;
            float tTractMax = Mathf.Abs(tDelta) / Time.fixedDeltaTime;
            float fTractMax = tTractMax * radiusInverse;


            fTractMax = Mathf.Min(fTractMax, fLongMax);
            float tractionTorque = fTractMax * wheelRadius * -Mathf.Sign(vWheelDelta);
            localForce.z = fTractMax * Mathf.Sign(vWheelDelta);
            float angularAcceleration = tractionTorque * inertiaInverse * Time.fixedDeltaTime;
            currentAngularVelocity += angularAcceleration;
            if (Mathf.Abs(currentAngularVelocity) < wBrakeDelta)
            {
                currentAngularVelocity = 0;
                wBrakeDelta -= Mathf.Abs(currentAngularVelocity);
                float fMax = Mathf.Max(0, Mathf.Abs(fLongMax) - Mathf.Abs(localForce.z));
                float fMax2 = Mathf.Max(0, downforce * Mathf.Abs(localVelocity.z) - Mathf.Abs(localForce.z));
                float fBrakeMax = Mathf.Min(fMax, fMax2);
                localForce.z += fBrakeMax * -Mathf.Sign(localVelocity.z);
            }
            else
            {
                currentAngularVelocity += -Mathf.Sign(currentAngularVelocity) * wBrakeDelta;
            }

            FinalFriction(fLatMax, fLongMax, localForce.x, localForce.z, out localForce.x, out localForce.z);
            //TODO technically wheel angular velocity integration should not occur until after the force is capped here, otherwise things will get out of synch
        }

        private void FinalFriction(float latMax, float longMax, float fLat, float fLong, out float combLat, out float combLong)
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


        public void DrawDebug()
        {
            if (!grounded) { return; }

            Vector3 rayStart, rayEnd;
            Vector3 vOffset = rig.velocity * Time.fixedDeltaTime;

            rayStart = hitPoint;
            rayEnd = hitNormal * localForce.y;
            rayEnd += wR * localForce.x;
            rayEnd += wF * localForce.z;
            rayEnd += rayStart;

            Debug.DrawLine(rayStart + vOffset, rayEnd + vOffset, Color.magenta);

            rayStart += wheel.transform.up * 0.1f;
            rayEnd = rayStart + wF * 10f;
            Debug.DrawLine(rayStart + vOffset, rayEnd + vOffset, Color.blue);

            rayEnd = rayStart + wR * 10f;
            Debug.DrawLine(rayStart + vOffset, rayEnd + vOffset, Color.red);
        }

    }
}

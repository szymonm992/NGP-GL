using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public struct VolumetricHitResult
{
    public Vector3 hit;
    public Vector3 normal;
    public float resolveDistance;

    public VolumetricHitResult(Vector3 hit, Vector3 normal, float resolveDistance){
        this.hit = hit;
        this.normal = normal;
        this.resolveDistance = resolveDistance;
    }
}


//[RequireComponent(typeof(Rigidbody))]
public class VolumetricCylinderCollider : MonoBehaviour
{
    public float radius = 1;
    public float height = 2;

    public int resolution = 4;

    private bool isCollision = false;


    private VolumetricHitResult[] hitPoints;



    private void FixedUpdate()
    {

        hitPoints = Check(transform.position, radius, height, transform.rotation, resolution);
        /*
        Vector3 offsetSum = Vector3.zero;
        foreach (VolumetricHitResult hitResult in hitResults) {
            Debug.DrawLine(hitResult.hit, hitResult.hit + hitResult.normal * hitResult.resolveDistance, new Color(255, 128, 0), Time.fixedDeltaTime);
            offsetSum += hitResult.normal * hitResult.resolveDistance;
        }
        offsetSum /= hitResults.Length;
        transform.position += offsetSum;*/
    }


    // handles physics detection with intersecting colliders
    public static VolumetricHitResult[] Check(Vector3 origin, float radius, float height, Quaternion rotation, int resolution = 4) {
        List<VolumetricHitResult> hitResults = new List<VolumetricHitResult>();

        Collider[] checkColliders = Physics.OverlapBox(origin, new Vector3(radius, height * 0.5f, radius), rotation);

        // heat map of collision, saving distances from each collider and then using it to determine hit point with it.
        // coords are polar + y axis for height. [theta, r, y]
        int res = Mathf.Clamp(resolution, 1, 32);
        int resTheta = 3 + res;
        Vector3[,,] colCheckPoints = new Vector3[resTheta, res+1, res+1];
        Vector3[,,] colHitVecs = new Vector3[resTheta, res+1, res+1];
        bool[,,] colIsHit = new bool[resTheta, res + 1, res + 1];

        float thetaStep = 360.0f / resTheta;
        float linearStep = 1.0f / res;

        // for each collider, a separate hit map and set of hit points are generated
        foreach (Collider col in checkColliders) {
            // generate first heat map
            for(int i_theta = 0; i_theta < resTheta; i_theta++) {
                float theta = thetaStep * i_theta;
                Vector3 thetaDir = Quaternion.AngleAxis(theta, rotation * Vector3.up) * (rotation * Vector3.forward);
                for (int i_r = 0; i_r <= res; i_r++) {
                    float r2 = Mathf.Pow(i_r * linearStep, 0.5f) * radius;
                    for (int i_h = 0; i_h <= res; i_h++) {
                        float h = (i_h * linearStep - 0.5f) * height;

                        Vector3 checkPoint = origin + thetaDir * r2 + (rotation * Vector3.up) * h;

                        Vector3 hitPoint = col.ClosestPoint(checkPoint);

                        // storing both normal and distance to the closest surface.
                        colCheckPoints[i_theta, i_r, i_h] = checkPoint;
                        colHitVecs[i_theta,i_r,i_h] = checkPoint - hitPoint; 
                    }
                }
            }

            bool hasHit = false;

            // find hit vectors
            for (int i_theta = 0; i_theta < resTheta; i_theta++) {
                for (int i_r = 0; i_r <= res; i_r++) {
                    for (int i_h = 0; i_h <= res; i_h++) {
                        colIsHit[i_theta, i_r, i_h] = false;
                        Vector3 colHit = colHitVecs[i_theta, i_r, i_h];
                        if (colHit.magnitude == 0) {
                            colIsHit[i_theta, i_r, i_h] = true;
                            hasHit = true;
                        } else {
                            for (int j = 0; j < 27; j++) {
                                if (j == 14) continue;
                                int x = i_theta - 1 + j % 3;
                                int y = i_r - 1 + (j / 3) % 3;
                                int z = i_h - 1 + (j / 9) % 3;
                                if (x < 0 || y < 0 || z < 0 || x >= resTheta || y > res || z > res) continue;
                                Vector3 secondColHit = colHitVecs[x,y,z];
                                if (Vector3.Dot(colHit, secondColHit) < 0) {
                                    colIsHit[i_theta, i_r, i_h] = true;
                                    hasHit = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (hasHit) {
                int totalVecCount = 0;
                Vector3 totalNormal = Vector3.zero;
                Vector3 totalHitPoint = Vector3.zero;

                // find hit point vectors
                for (int i_theta = 0; i_theta < resTheta; i_theta++) {
                    for (int i_r = 0; i_r <= res; i_r++) {
                        for (int i_h = 0; i_h <= res; i_h++) {
                            if (colIsHit[i_theta, i_r, i_h]) {

                                int nCount = 0;
                                Vector3 nNormal = Vector3.zero;

                                for (int j = 0; j < 27; j++) {
                                    if (j == 14) continue;
                                    int x = i_theta - 1 + j % 3;
                                    int y = i_r - 1 + (j / 3) % 3;
                                    int z = i_h - 1 + (j / 9) % 3;
                                    if (x < 0 || y < 0 || z < 0 || x >= resTheta || y > res || z > res) continue;

                                    nNormal += colHitVecs[x, y, z];
                                    nCount++;
                                }

                                totalNormal += nNormal / nCount;
                                totalHitPoint += colCheckPoints[i_theta, i_r, i_h];
                                totalVecCount++;
                            }
                        }
                    }
                }

                totalNormal /= totalVecCount;
                totalHitPoint /= totalVecCount;

                /*
                float resolveDistance = IntersectRay(new Ray(totalHitPoint, -totalNormal.normalized), origin, radius, height, rotation);
                if (resolveDistance == Mathf.Infinity) resolveDistance = 0;

                hitResults.Add(new VolumetricHitResult(totalHitPoint, totalNormal.normalized, resolveDistance));*/

                hitResults.Add(new VolumetricHitResult(totalHitPoint, totalNormal.normalized, 0));
            }
        }
        return hitResults.ToArray();
    }


    // finds an intersection point of the ray with the surface of a cylinder, regardless of whether a ray started in or out of it.
    // returns a floating point representing how far on a ray intersection happens. Return Mathf.Infinity if hit failed.
    private static float IntersectRay(Ray ray, Vector3 origin, float radius, float height, Quaternion rotation) {

        float halfHeight = height * 0.5f;

        // transforming ray into a local space of the cylinder (no rotation, world origin)
        Quaternion rotInverse = Quaternion.Inverse(rotation);
        Ray tRay = new Ray(rotInverse * (ray.origin - origin), rotInverse * ray.direction);

        // checking if ray would hit cylinder vertically at all
        if((tRay.origin.y > halfHeight && tRay.direction.y >= 0) || (tRay.origin.y < -halfHeight && tRay.direction.y <= 0)) {
            return Mathf.Infinity;
        }

        // finding intersection point with cylinder walls.
        Vector2 tRay2D = new Vector2(tRay.origin.x, tRay.origin.z);
        Vector2 tRayDir2D = new Vector2(tRay.direction.x, tRay.direction.z);
        Vector2 tRayDir2DNorm = tRayDir2D.normalized;
        Vector2 spherePos2D = tRay2D - tRayDir2DNorm * Vector2.Dot(tRay2D, tRayDir2DNorm);
        if (spherePos2D.magnitude > radius) return Mathf.Infinity; // line doesn't cross the circle
        float sphereOff = Mathf.Sin(Mathf.Acos(spherePos2D.magnitude/radius));
        Vector2 vec1 = (spherePos2D + tRayDir2DNorm * sphereOff * radius) - tRay2D;
        Vector2 vec2 = (spherePos2D - tRayDir2DNorm * sphereOff * radius) - tRay2D;

        Vector2 vec2D = Vector2.zero;
        if (Vector2.Dot(tRayDir2DNorm, vec1) > 0) {
            vec2D = vec1;
        }
        if (Vector2.Dot(tRayDir2DNorm, vec2) > 0 && (vec2D == Vector2.zero || vec2.magnitude < vec1.magnitude)) {
            vec2D = vec2;
        }
        if (vec2D.magnitude == 0) return Mathf.Infinity; // line points away from the circle

        Vector3 vec = new Vector3(vec2D.x, tRay.direction.y * vec2D.magnitude * (tRayDir2DNorm.magnitude / tRayDir2D.magnitude), vec2D.y);
        // extend/limit found ray point to circle faces of the cylinder
        if(Mathf.Abs(tRay.origin.y+vec.y) > halfHeight || (tRay2D.magnitude <= radius && Mathf.Abs(tRay.origin.y) > halfHeight)) {
            float yPlane = -halfHeight;
            if(tRay.origin.y > halfHeight || (tRay.origin.y > -halfHeight && tRay.direction.y > 0)) {
                yPlane = halfHeight;
            }
            vec *= (yPlane - tRay.origin.y) / vec.y;

            // checking hit point to make sure it's within the cylinder after this transformation.
            vec2D = new Vector2(tRay.origin.x+vec.x,tRay.origin.z+vec.z);
            if (vec2D.magnitude > radius) return Mathf.Infinity;
        }

        return vec.magnitude;
    }

    // drawing a wireframe of a cylinder when object is selected
    private void OnDrawGizmos() {
        if (!this.enabled || !gameObject.activeInHierarchy) return;

        Gizmos.color = isCollision ? Color.red : Color.green;

        var halfHeight = transform.up * height * 0.5f;

        UnityEditor.Handles.color = isCollision ? Color.red : Color.green;
        UnityEditor.Handles.DrawWireDisc(transform.position - halfHeight, transform.up, radius);
        UnityEditor.Handles.DrawWireDisc(transform.position + halfHeight, transform.up, radius);

       

        if (hitPoints == null || hitPoints.Length == 0) return;
        foreach (VolumetricHitResult hitResult in hitPoints)
        {
            //Debug.DrawLine(hitResult.hit, hitResult.hit + hitResult.normal * hitResult.resolveDistance, new Color(255, 128, 0), Time.fixedDeltaTime);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hitResult.hit, .1f);
            //offsetSum += hitResult.normal * hitResult.resolveDistance;
        }
        /*
        for (int i = 0; i < 4; i++) {
            Vector3 offset = (i < 2 ? transform.forward : transform.right) * (i % 2 > 0 ? -1 : 1) * radius;
            Gizmos.DrawLine(transform.position + offset - halfHeight, transform.position + offset + halfHeight);
        }*/
    }
}

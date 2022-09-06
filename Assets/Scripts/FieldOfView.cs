using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [Range (0, 10)]
    public float viewRadius;
    [Range (0, 360)]
    public float viewAngle;
    public LayerMask obstacleMask;

    public float meshResolution;
    public int edgeResolution;
    public float edgeDstThreshold;
    public MeshFilter viewMeshFilter;
    [HideInInspector]public Mesh viewMesh;

    private void Start() {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
    }

    private void Update() {
        // DrawFieldOfView();
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal){
        if(!angleIsGlobal) {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public Vector3 FindBestDirectionToAvoidObstacle(float obsatacleDistance, LayerMask obsatacleMask){
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        
        Vector3 selectedDirection = Vector3.zero;
        float maxDistance = int.MinValue;

        for(int i = 0 ; i <= stepCount ; i++){
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle, obsatacleDistance, obsatacleMask);

            if(newViewCast.hit){
                if(maxDistance > newViewCast.dst){
                    maxDistance = newViewCast.dst;
                    selectedDirection = newViewCast.point;
                }
            }
            else{
                selectedDirection = newViewCast.point;
                return selectedDirection.normalized;
            }
        }
        return selectedDirection.normalized;
    }

    private void DrawFieldOfView(){
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;

        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for(int i = 0 ; i <= stepCount ; i++){
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if(i > 0){
                bool edgeDstThresholdExcceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if(oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExcceeded)){
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if(edge.pointA != Vector3.zero){
                        viewPoints.Add(edge.pointA);
                    }
                    if(edge.pointB != Vector3.zero){
                        viewPoints.Add(edge.pointB);
                    }
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }


        // Draw Mesh
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for(int i = 0 ; i < vertexCount - 1 ; i++){
            vertices[i+1] = transform.InverseTransformPoint(viewPoints[i]);
            if(i < vertexCount - 2){
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }
        
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    public ViewCastInfo ViewCast(float globalAngle){
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if(Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask)){
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else{
            return new ViewCastInfo(false, transform.position + dir * viewRadius, hit.distance, globalAngle);
        }
    }

    public ViewCastInfo ViewCast(float globalAngle, float obstacleDistance, LayerMask obsatacleMask){
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if(Physics.Raycast(transform.position, dir, out hit, obstacleDistance, obsatacleMask)){
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else{
            return new ViewCastInfo(false, transform.position + dir * obstacleDistance, hit.distance, globalAngle);
        }
    }

    public EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast){
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for(int i = 0 ; i < edgeResolution ; i++){
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);
            
            bool edgeDstThresholdExcceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if(newViewCast.hit == minViewCast.hit && !edgeDstThresholdExcceeded){
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else{
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    public struct ViewCastInfo {
        public bool hit;
        public Vector3 point;
        public float dst;
        public  float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle){
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }
    
    public struct EdgeInfo{
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB){
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}

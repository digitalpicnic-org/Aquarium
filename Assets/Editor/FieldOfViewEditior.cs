using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(FieldOfView))]
public class FieldOfViewEditior : Editor
{
    private void OnSceneGUI() {
        // FieldOfView fov = (FieldOfView)target;
        // Handles.color = Color.white;
        // Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.viewRadius);
        // Vector3 viewAngleA = fov.DirFromAngle(-fov.viewAngle / 2, false);
        // Vector3 viewAngleB = fov.DirFromAngle(fov.viewAngle / 2, false);
        
        // Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius);
        // Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius);

        // DrawFieldOfView(fov);
    }

    private void DrawFieldOfView(FieldOfView fov){
        int stepCount = Mathf.RoundToInt(fov.viewAngle * fov.meshResolution);
        float stepAngleSize = fov.viewAngle / stepCount;
        // List<Vector3> viewPoints = new List<Vector3>();
        // FieldOfView.ViewCastInfo oldViewCast = new FieldOfView.ViewCastInfo();
        for(int i = 0 ; i <= stepCount ; i++){
            float angle = fov.transform.eulerAngles.y - fov.viewAngle / 2 + stepAngleSize * i;
            Handles.color = Color.red;
            Handles.DrawLine(fov.transform.position, fov.transform.position + fov.DirFromAngle(angle, true) * fov.viewRadius);
        }
        
    }
    private void DrawFieldOfViewa(FieldOfView fov){
        int stepCount = Mathf.RoundToInt(fov.viewAngle * fov.meshResolution);
        float stepAngleSize = fov.viewAngle / stepCount;

        List<Vector3> viewPoints = new List<Vector3>();
        FieldOfView.ViewCastInfo oldViewCast = new FieldOfView.ViewCastInfo();
        for(int i = 0 ; i <= stepCount ; i++){
            float angle = fov.transform.eulerAngles.y - fov.viewAngle / 2 + stepAngleSize * i;
            Handles.color = Color.red;
            Handles.DrawLine(fov.transform.position, fov.transform.position + fov.DirFromAngle(angle, true) * fov.viewRadius);

            FieldOfView.ViewCastInfo newViewCast = fov.ViewCast(angle);
            if(i > 0){
                bool edgeDstThresholdExcceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > fov.edgeDstThreshold;
                if(oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExcceeded)){
                    FieldOfView.EdgeInfo edge = fov.FindEdge(oldViewCast, newViewCast);
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

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = fov.transform.position;
        for(int i = 0 ; i < vertexCount - 1 ; i++){
            vertices[i+1] = fov.transform.InverseTransformPoint(viewPoints[i] + fov.transform.position);
            if(i < vertexCount - 2){
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }
        
        Handles.color = Color.yellow;
        Handles.DrawAAConvexPolygon(vertices);
        // fov.viewMesh.Clear();
        // fov.viewMesh.vertices = vertices;
        // fov.viewMesh.triangles = triangles;
        // fov.viewMesh.RecalculateNormals();
    }
}

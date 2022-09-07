using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(Koi))]
public class KoiEditor : Editor
{
    private void OnSceneGUI() {
        Koi koi = (Koi)target;
        
        Handles.color = Color.yellow;
        // head
        Handles.DrawWireDisc(koi.headTranform.position, koi.transform.forward, koi.headSize);
        Handles.DrawWireDisc(koi.headTranform.position, koi.transform.right, koi.headSize);
        Handles.DrawWireDisc(koi.headTranform.position, koi.transform.up, koi.headSize);
        

        // sprint
        Handles.DrawWireDisc(koi.headTranform.position + koi.transform.forward * koi.sprintDistance, koi.transform.forward, koi.headSize);
        Handles.DrawWireDisc(koi.headTranform.position + koi.transform.forward * koi.sprintDistance, koi.transform.right, koi.headSize);
        Handles.DrawWireDisc(koi.headTranform.position + koi.transform.forward * koi.sprintDistance, koi.transform.up, koi.headSize);

        Handles.color = Color.red;
        
        Handles.DrawWireDisc(koi.headTranform.position + koi.transform.forward * koi.obsatacleDistance, koi.transform.forward, koi.headSize);
        
        // if(koi.reflexVector != Vector3.zero){
        //     Handles.color = Color.red;
        //     Handles.DrawLine(koi.hitPoint, koi.hitPoint + koi.reflexVector);
        //     Handles.color = Color.yellow;
        //     Handles.DrawWireDisc(koi.hitPoint, koi.reflexVector, 0.05f);
        // }

        Handles.color = Color.white;
        Handles.DrawLine(koi.headTranform.position, koi.headTranform.position + koi.moveVector * 2);
        Handles.DrawWireDisc(koi.headTranform.position + koi.moveVector * 2, Vector3.up, 0.01f);
        Handles.DrawWireDisc(koi.headTranform.position + koi.moveVector * 2, Vector3.forward, 0.01f);
        Handles.DrawWireDisc(koi.headTranform.position + koi.moveVector * 2, Vector3.right, 0.01f);
    }
}

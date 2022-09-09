using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Koi : MonoBehaviour
{
    public float defaultSpeed;
    public float maxSpeed;
    public float minSpeed;
    public float currentSpeed;
    public LayerMask obsatacleMask;
    public float obsatacleDistance;
    public Vector3 reflexVector;
    public float maxStamina;
    public float currentStamina;
    public float sprintDistance;
    public float headSize;
    public Transform headTranform;
    public Vector3 currentVerocity;
    public Vector3 hitPoint;
    public Vector3 moveVector;

    [Range(0, 360)]
    public float maxDegree;
    [Range(0,5)]
    public float time;
    public bool nearObsatacle;
    public float reduceSpeed;
    public MeshRenderer meshRenderer;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // var b= CalculateObstacleReflexVector(out bool a);
        // b *= time;
        // moveVector = Vector3.RotateTowards(transform.forward, b, Mathf.Deg2Rad * maxDegree, time);
        // transform.forward = Vector3.SmoothDamp(transform.forward, moveVector, ref currentVerocity, 1);
        MoveKoi();
    }


    public void MoveKoi(){
        // bool nearObsatacle;
        var obsatacleVector = CalculateObstacleReflexVector(out nearObsatacle);
        Debug.Log(obsatacleVector);
        CalculateSprint(nearObsatacle);
        if(hitPoint != Vector3.zero)
        time = (obsatacleDistance - (Vector3.Distance(headTranform.position, hitPoint)))/obsatacleDistance * 3;
        if(hitPoint != Vector3.zero){
            obsatacleVector *= time;
        }
        // moveVector = transform.TransformDirection(obsatacleVector);
        moveVector = Vector3.RotateTowards(transform.forward, obsatacleVector, Mathf.Deg2Rad * 70, time);
        moveVector = Vector3.SmoothDamp(transform.forward, moveVector, ref currentVerocity, 1);
        moveVector = moveVector.normalized * currentSpeed;
        transform.forward = moveVector;
        transform.position += moveVector * Time.deltaTime;

        meshRenderer.material.SetFloat("_FrequencyZ", currentSpeed);
    }

    private void CalculateSprint(bool nearObsatacle){
        
        if(Physics.CheckSphere(headTranform.position + transform.forward * sprintDistance, headSize, obsatacleMask) || nearObsatacle){
            if(currentStamina < maxStamina){
                currentStamina = Mathf.Lerp(currentStamina, maxStamina, Time.deltaTime / 2);
            }
            if(!CheckObstacle()){
                currentSpeed = Mathf.Lerp(currentSpeed, defaultSpeed, Time.deltaTime * 2);
            }
            else{
                reduceSpeed = 1;
                if(hitPoint != Vector3.zero){
                    reduceSpeed = Vector3.Distance(headTranform.position, hitPoint)/obsatacleDistance;
                }
                currentSpeed = Mathf.Lerp(currentSpeed, reduceSpeed * defaultSpeed, Time.deltaTime * 2);
            }
        
        }
        else{
            if(currentStamina > 0){
                currentStamina = Mathf.Lerp(currentStamina, 0, Time.deltaTime);
            }
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.deltaTime);
            Debug.Log("sprint");
        }
    }

    public Vector3 CalculateObstacleReflexVector(out bool hitObstacle){
        var obsatacleVector = Vector3.zero;
        int minHit = int.MaxValue;
        RaycastHit[] hits = Physics.BoxCastAll(headTranform.position, Vector3.one * headSize/2, headTranform.forward, transform.rotation, obsatacleDistance, obsatacleMask);
        foreach(var hit in hits){
            if(minHit > hit.distance && hit.point != Vector3.zero){
                reflexVector = hit.normal;
                hitPoint = hit.point;
                Debug.Log(hit.point);
            }
        }
        if(hits.Length == 0){
            reflexVector = Vector3.zero;
            hitPoint = Vector3.zero;
            hitObstacle = false;
        }
        else    hitObstacle = true;
        obsatacleVector = reflexVector.normalized;
        // RaycastHit hit;
        // if(Physics.SphereCast(headTranform.position, headSize, transform.forward, out hit, obsatacleDistance, obsatacleMask)){
        //     // reflexVector = Vector3.Reflect(transform.forward, hit.normal).normalized;
        //     reflexVector = hit.normal;
        //     hitPoint = hit.point;
        //     obsatacleVector = reflexVector;
        //     obsatacleVector *= (obsatacleDistance - hit.distance) * 5;
        //     hitObstacle = true;
        //     Debug.Log("Hit");
        // }
        // else{
        //     reflexVector = Vector3.zero;
        //     hitPoint = Vector3.zero;
        //     obsatacleVector = Vector3.zero;
        //     hitObstacle = false;
        // }

        // obsatacleVector = reflexVector;
        return obsatacleVector;
    }

    public bool CheckObstacle(){
        if(Physics.SphereCast(headTranform.position, headSize, transform.forward, out RaycastHit hit, obsatacleDistance)){
            return true;
        }
        else{
            return false;
        }
    }



    // public Vector3 FindBestExit(){
    //     int maxDis = int.MaxValue;
    //     RaycastHit[] hits = Physics.SphereCastAll(headTranform.position, headSize, )
    // }





    private void OnDrawGizmosSelected() {

        Gizmos.color = Color.red;
        int minHit = int.MaxValue;
        RaycastHit[] hits = Physics.BoxCastAll(headTranform.position, Vector3.one * headSize/2, headTranform.forward, transform.rotation, obsatacleDistance, obsatacleMask);
        foreach(var hit in hits){
            
            if(minHit > hit.distance && hit.point != Vector3.zero){
                Gizmos.DrawLine(headTranform.position, hit.point);
            }
        }

        
        
    }
}

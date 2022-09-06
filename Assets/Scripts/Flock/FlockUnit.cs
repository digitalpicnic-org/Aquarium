using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockUnit : MonoBehaviour
{
    [SerializeField]private float FOVAngle;
    [SerializeField]private float smotthDamp;
    [SerializeField]private LayerMask obstacleLayer;
    [SerializeField]private Vector3[] directionsToCheckWhenAvoidingObstacles;

    private List<FlockUnit> cohesionNeighbours = new List<FlockUnit>();
    private List<FlockUnit> aligementNeighbours = new List<FlockUnit>();
    private List<FlockUnit> avoidanceNeighbours = new List<FlockUnit>();
    private Flock assignedFlock;
    [SerializeField]private Vector3 currenVelocity;
    private Vector3 currentObstacleAvoidanceVector;
    [SerializeField]private float speed;
    private Vector3 targetMoveVector;
    // private FieldOfView fov;

    private void Start() {
        // fov = GetComponent<FieldOfView>();
        // fov.viewAngle = FOVAngle;
        // fov.obstacleMask = obstacleLayer;
        // fov.viewRadius = assignedFlock.obstacleDistance;
        GenerateDir();
    }

    public void AssignFlock(Flock flock){
        assignedFlock = flock;
    }

    public void InitialzeSpeed(float speed){
        this.speed = speed; 
    }

    public void MoveUnit(){
        FindNeighbours();
        CalculateSpeed();
        var cohesionVector = CalculateCohesionVector() * assignedFlock.cohesionWeight;
        var avoidanceVector = CalculateAvoidanceVector() * assignedFlock.avoidanceWeight;
        var aligementVector = CalculateAligementVector() * assignedFlock.aligementWeight;
        var boundsVector = CalculateBoundsVector() * assignedFlock.boundsWeight;
        var obstacleVector = CalculateObstacleVector() * assignedFlock.obstacleWeight;
        // var obstacleReflec = CalculateObstacleReflexVector() * assignedFlock.obstacleWeight;
        
        var moveVector = cohesionVector  + avoidanceVector + aligementVector + boundsVector + obstacleVector;
        moveVector = Vector3.SmoothDamp(transform.forward, moveVector, ref currenVelocity, smotthDamp);
        moveVector = moveVector.normalized * speed;
        targetMoveVector = moveVector;
        transform.forward = moveVector;
        transform.position += moveVector * Time.deltaTime;
    }

    

    private void FindNeighbours(){
        cohesionNeighbours.Clear();
        aligementNeighbours.Clear();
        avoidanceNeighbours.Clear();
        List<FlockUnit> allUnits = new List<FlockUnit>();
        foreach (var unit in assignedFlock.allUnits){
            allUnits.Add(unit.unit);
        }

        foreach(var unit in allUnits){
            if(unit != this){
                float currentNeighbourDistanceSqr = Vector3.SqrMagnitude(unit.transform.position - transform.position);
                if(currentNeighbourDistanceSqr <= assignedFlock.cohesionDistance * assignedFlock.cohesionDistance)
                {
                    cohesionNeighbours.Add(unit);
                }
                if(currentNeighbourDistanceSqr <= assignedFlock.aligementDistance * assignedFlock.aligementDistance)
                {
                    aligementNeighbours.Add(unit);
                }
                if(currentNeighbourDistanceSqr <= assignedFlock.avoidanceDistance * assignedFlock.avoidanceDistance)
                {
                    avoidanceNeighbours.Add(unit);
                }
            }
        }
    }

    private void CalculateSpeed(){
        if(cohesionNeighbours.Count == 0) return; 
        foreach(var unit in cohesionNeighbours){
            speed += unit.speed;
        }
        speed /= cohesionNeighbours.Count;
        speed = Mathf.Clamp(speed, assignedFlock.minSpeed, assignedFlock.maxSpeed);
    }

    public Vector3 CalculateCohesionVector(){
        var cohesionVector = Vector3.zero;
        if(cohesionNeighbours.Count == 0)
            return cohesionVector;
        int neighboursInFOV = 0;
        foreach(var unit in cohesionNeighbours){
            if(IsInFOV(unit.transform.position)){
                neighboursInFOV++;
                cohesionVector += unit.transform.position;
            }
        }
        if(neighboursInFOV == 0)
            return cohesionVector;
        cohesionVector /= neighboursInFOV;
        cohesionVector -= transform.position;
        cohesionVector = Vector3.Normalize(cohesionVector);
        return cohesionVector;
    }

    private Vector3 CalculateAligementVector()
    {
        var aligementVector = transform.forward;
        if(aligementNeighbours.Count == 0)
            return aligementVector;
        int neighboursInFOV = 0;
        foreach(var unit in aligementNeighbours){
            if(IsInFOV(unit.transform.position)){
                neighboursInFOV++;
                aligementVector += unit.transform.forward;
            }
        }
        if(neighboursInFOV == 0)
            return transform.forward;
        aligementVector /= neighboursInFOV;
        aligementVector = Vector3.Normalize(aligementVector);
        return aligementVector;
    }

    private Vector3 CalculateAvoidanceVector()
    {
        var avoidanceVector = Vector3.zero ;
        if(avoidanceNeighbours.Count == 0)
            return avoidanceVector;
        int neighboursInFOV = 0;
        foreach(var unit in avoidanceNeighbours){
            if(IsInFOV(unit.transform.position)){
                neighboursInFOV++;
                avoidanceVector += transform.position - unit.transform.position;
            }
        }
        if(neighboursInFOV == 0)
            return Vector3.zero;
        avoidanceVector /= neighboursInFOV;
        avoidanceVector = Vector3.Normalize(avoidanceVector);
        return avoidanceVector;
    }

    private Vector3 CalculateBoundsVector(){
        var offsetToCenter = assignedFlock.transform.position - transform.position;
        bool isNearCenter = (offsetToCenter.magnitude >= assignedFlock.boundsDistance * 0.9f);
        return isNearCenter ? offsetToCenter.normalized : Vector3.zero;
    }

    private Vector3 CalculateObstacleVector(){
        var obstacleVector = Vector3.zero;
        RaycastHit hit;
        if(Physics.SphereCast(transform.position, 0.25f, transform.forward, out hit, assignedFlock.obstacleDistance, obstacleLayer)){
            obstacleVector = FindBestDirectionToAvoidObstacle();
            speed = Mathf.Lerp(speed, 0.2f, Time.deltaTime * 3);
            // Debug.Log(speed);
        }
        else{
            currentObstacleAvoidanceVector = Vector3.zero;
            speed = Mathf.Lerp(speed, assignedFlock.minSpeed, Time.deltaTime);
        }
        return obstacleVector;
    }
    
    public Vector3 CalculateObstacleReflexVector(){
        var obsatacleVector = Vector3.zero;
        RaycastHit hit;
        if(Physics.SphereCast(transform.position, 0.25f, transform.forward, out hit, assignedFlock.obstacleDistance, obstacleLayer)){
            var reflexVector = Vector3.Reflect(transform.forward, hit.normal).normalized;
            // var reflexVector = hit.normal;
            // hitPoint = hit.point;
            obsatacleVector = reflexVector;
            // obsatacleVector *= (obsatacleDistance - hit.distance) * 5;
            // hitObstacle = true;
            Debug.Log("Hit");
        }
        else{
            // reflexVector = Vector3.zero;
            // hitPoint = Vector3.zero;
            obsatacleVector = Vector3.zero;
            // hitObstacle = false;
        }

        // obsatacleVector = reflexVector;
        return obsatacleVector;
    }
    
    private Vector3 FindBestDirectionToAvoidObstacle(){
        // new 
        // var fov = GetComponent<FieldOfView>();
        // return fov.FindBestDirectionToAvoidObstacle(assignedFlock.obstacleDistance, obstacleLayer);


        // old
        RaycastHit hit;
        if(currentObstacleAvoidanceVector != Vector3.zero){
            if(Physics.Raycast(transform.position, transform.forward, out hit, assignedFlock.obstacleDistance, obstacleLayer)){
                return currentObstacleAvoidanceVector;
            }
        }
        float maxDistance = int.MinValue;
        Vector3 selectedDirection = Vector3.zero;
        Vector3 currentDirection;
        for (int i = 0 ; i < directionsToCheckWhenAvoidingObstacles.Length; i++){
            // RaycastHit hit;
            currentDirection = transform.TransformDirection(directionsToCheckWhenAvoidingObstacles[i].normalized);
            if(Physics.SphereCast(transform.position, 0.25f, currentDirection, out hit, assignedFlock.obstacleDistance, obstacleLayer)){
                float currentDistance = (hit.point - transform.position).sqrMagnitude;
                if(currentDistance > maxDistance){
                    maxDistance = currentDistance;
                    selectedDirection = currentDirection;
                }
            }
            else{
                selectedDirection = currentDirection;
                currentObstacleAvoidanceVector = currentDirection.normalized;
                return selectedDirection.normalized;
            }
        }

        var reflexVector = CalculateObstacleReflexVector();
        currentDirection = transform.TransformDirection(reflexVector.normalized);
        if(Physics.SphereCast(transform.position, 0.25f, currentDirection, out hit, assignedFlock.obstacleDistance, obstacleLayer)){
            float currentDistance = (hit.point - transform.position).sqrMagnitude;
            if(currentDistance > maxDistance){
                maxDistance = currentDistance;
                selectedDirection = currentDirection;
            }
        }
        else{
            selectedDirection = currentDirection;
            currentObstacleAvoidanceVector = currentDirection.normalized;
            return selectedDirection.normalized;
        }
        return selectedDirection.normalized;

    }

    public void GenerateDir(){
        List<Vector3> dirs = new List<Vector3>();
        int dividedSize = directionsToCheckWhenAvoidingObstacles.Length / 4;
        dirs.Add(DirFromAngle(0, Vector3.up));
        for(int i = 1 ; i <= dividedSize ; i++){
            float angle = FOVAngle / 2 / dividedSize * i;
            dirs.Add(DirFromAngle(angle, Vector3.up));
            dirs.Add(DirFromAngle(-angle, Vector3.up));
            dirs.Add(DirFromAngle(angle, Vector3.right));
            dirs.Add(DirFromAngle(-angle, Vector3.right));
        }
        directionsToCheckWhenAvoidingObstacles = dirs.ToArray();
    }

    public Vector3 DirFromAngle(float angleInDegrees, Vector3 normal){
        if(normal == Vector3.up)
            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        else if(normal == Vector3.right)
            return new Vector3(0, Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        else
            return Vector3.zero;
    }

    private bool IsInFOV(Vector3 position)
    {
        return Vector3.Angle(transform.forward, position - transform.position) <= FOVAngle ;
    }

    private void OnDrawGizmosSelected(){
        
        // Draw line cohesion
        foreach(var nieghbour in cohesionNeighbours){
            // var nieghbourDirection = nieghbour.transform.position - transform.position;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, nieghbour.transform.position);

        }

        // Draw line found Obstacle
        foreach(var line in directionsToCheckWhenAvoidingObstacles){
            var currentDirection = transform.TransformDirection(line.normalized);
            RaycastHit hit;
            if(assignedFlock != null){
                if(Physics.SphereCast(transform.position, 0.25f, currentDirection, out hit, assignedFlock.obstacleDistance, obstacleLayer)){
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, hit.point);
                    // Gizmos.DrawWireSphere(hit.point, 0.25f);
                }
                else{
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(transform.position, transform.position + currentDirection * assignedFlock.obstacleDistance);
                }
            }
        }
        
        // Draw line target move
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + targetMoveVector);
        Gizmos.DrawWireSphere(transform.position + targetMoveVector , 0.25f);

        Vector3 avoidDir = FindBestDirectionToAvoidObstacle();
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + avoidDir);
        Gizmos.DrawWireSphere(transform.position + currentObstacleAvoidanceVector , 0.25f);

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishUnit : MonoBehaviour
{
    [Header ("Setup Unit")]
    [SerializeField]private UnitType _type;
    public UnitType type {
                        get => _type;
                    }
    private Spawner spawner;

    [Header ("Setup Target Spawn")]
    [SerializeField]
    [Tooltip ("Distance between the nearest and the farthest on z Axis")]private float maxDepth;
    [SerializeField]
    [Tooltip ("The Minimize of distance between an unit and the camera")]private float minZPos;
    [SerializeField][Range (0, 1)]
    [Tooltip ("The Ratio to random the next destination")]private float nextTargetRatio;

    [Header ("Target Setup")]
    [HideInInspector]
    [SerializeField]
    [Tooltip ("The destination position")]private Vector3 destination;
    [SerializeField]
    [Tooltip ("The Minimize of distance between an unit and destination")]private float distanceThreshold;
    [SerializeField]
    [Tooltip ("Time remain to decide to change direction to destination")]private float maxRedirectTime;
    [HideInInspector][SerializeField]private float redirectTime;

    [Header ("Movement")]
    [HideInInspector][SerializeField]private Vector3 currentVelocity;
    [SerializeField]
    [Tooltip ("How smooth to rotate when moving")]private float smoothDamp;
    [SerializeField]private float speed;
    [HideInInspector][SerializeField]private float Cspeed;
    [SerializeField]
    [Tooltip ("Rotation speed when rotate move than 170 degrees")]private float rotateSpeed;

    [Header ("Shader")]
    [SerializeField]private MeshRenderer _renderer;
    [HideInInspector][SerializeField]private bool isSpin = false;
    [HideInInspector][SerializeField]private bool isSet = false;
    [SerializeField][Range (0, 1)]
    [Tooltip ("Prob about Event spin when reach destination")]private float probSpin;

    // properties
    [Header ("Properties")]
    [HideInInspector]public List<UnitType> fishType;
    [HideInInspector]public List<UnitType> mammalType;
    private bool isRotate = false;
    private Vector3 rotatePoint;
    public float aliveDuration;
    private bool isDead = false;
    [Range (0, 1)]public float speedRatio;
    private Vector3 initDest;
    

    // Set by spawner
    // Set first destination
    public void SetInitDestination(Vector3 dest){
        // destination = dest;
        initDest = dest;
    }

    // Set spawner
    public void AssignSpawner(Spawner spawner){
        this.spawner = spawner;
    }

    // Set baseMap fish
    public void SetBaseMap(Texture baseMap){
        _renderer.material.SetTexture("_BaseMap", baseMap);
    }


    void Start()
    {
        // When spawned by spawner
        if(spawner){
            fishType = spawner.fishType;
            mammalType = spawner.mammalType;
        }
        SpawnMe();
    }

    // Setup start fish
    public void SpawnMe(){
        if(!isSet)
            StartCoroutine(SpawnFish());

        // if(mammalType.Exists(t => t == _type) || _type == UnitType.Shark)
            destination = RandomFirstDestination(true);
        // else
        //     destination = RandomNewDestination();
        transform.forward = new Vector3(destination.x - initDest.x, 0, destination.z - initDest.z);

        // Shader animation
        // disable first
        _renderer.material.SetFloat("_RandomValue", UnityEngine.Random.Range(-3.14f, 3.14f));
        /*if(fishType.Exists(t => t == _type) || mammalType.Exists(t => t == _type))
            _renderer.material.SetFloat("_DistTail", 0);
        if(_type == UnitType.Turtle)
            _renderer.material.SetFloat("_DistFin", 0);*/
    }

    IEnumerator SpawnFish(){
        // Set drop fish in pool
        while(Mathf.Abs(transform.position.y - initDest.y) > 0.05f){
            transform.position = Vector3.Lerp(transform.position, initDest, Time.deltaTime);
            yield return null;
        }
        isSet = true;
            
        


        // Shader animation
        // Start swim
        /*if(fishType.Exists(t => t == _type) || mammalType.Exists(t => t == _type))
            _renderer.material.SetFloat("_DistTail", 0.1f);
        if(_type == UnitType.Turtle)
            _renderer.material.SetFloat("_DistFin", 0.1f);*/
    }


    void Update()
    {
        // When SpawnFish done
        if(isSet){
            // Movement
            CheckDestination();

            // Check alive
            aliveDuration -= Time.deltaTime;
            if(aliveDuration <= 0 && !isDead ){
                // exit event
                ExitPool();
            }
        }
            
    }

    private void ExitPool(){
        isDead = true;
        destination = new Vector3(0, 0, 25);
        distanceThreshold = 1;
        spawner.allFish.Remove(this);

        StartCoroutine(LeavePool());
    }

    IEnumerator LeavePool(){
        while(transform.position.z < destination.z){
            var scale = Mathf.Abs(transform.position.z - destination.z);
            if(scale < 8){
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, (1 - scale / 8) * Time.deltaTime);
            }
            yield return null;
        }
        Destroy(this.gameObject);
    }


    private void CheckDestination(){
        // Set the unit types can spin 
        UnitType[] arrayCanSpin = {UnitType.Tuna, UnitType.Parrotfish, UnitType.Turtle, UnitType.Dolphin};
        List<UnitType> canSpin = new List<UnitType>();
        canSpin.AddRange(arrayCanSpin);

        // check destination
        if(Vector3.Distance(transform.position, destination) < distanceThreshold){
            // random new destination and reset redirectTime
            if(canSpin.Exists(t => t == _type))
                destination = RandomNewDestination();
            else
                destination = RandomFirstDestination(false);
            redirectTime = 0;

            var moveVector = destination - transform.position;
            if(Mathf.Abs(Vector3.SignedAngle(transform.forward, moveVector, Vector3.forward)) > 170){
                // only fish and turtle can spin
                // Check method to redirect to destination
                if(canSpin.Exists(t => t == _type) && UnityEngine.Random.value > probSpin){
                    Debug.Log("Spin");
                    SpinRightRound();
                    
                }
                else{
                    Debug.Log("Rotate");
                    isRotate = true;
                    rotatePoint = transform.position - transform.right;
                }
            }
        }
        else{
            // unit doesn't reach the destination
            redirectTime += Time.deltaTime;

            
            if(redirectTime >= maxRedirectTime && maxRedirectTime != 0 && !isDead){
                // Timeout 
                // check direction is normal or not
                if(Vector3.Angle(transform.forward, destination - transform.position) > 25){
                    if(canSpin.Exists(t => t == _type)){
                        SpinRightRound();
                        MoveUnit(false);
                    }
                    redirectTime = 0;
                    
                }
                else{
                    redirectTime = 0;
                    MoveUnit(true);
                }
            }
            else{
                MoveUnit(true);
            }
        }
    }

    private Vector3 RandomNewDestination(){
        var scale = UnityEngine.Random.insideUnitSphere;
        Vector3 boxSize;
        Vector3 centerOfBox = CalculateBoundTarget(transform.position, out boxSize);
        var newDest = Vector3.Scale(boxSize, scale) + centerOfBox;
        if(ValidateNewDestination(newDest))
            return newDest;
        else{
            
            var newScale = UnityEngine.Random.insideUnitSphere;
            Debug.Log($"old scale {scale} new scale {newScale}");
            Vector3 newBoxSize;
            Vector3 newCenterOfBox = CalculateBoundTarget(transform.position, out newBoxSize);
            var newSecondDest = Vector3.Scale(newBoxSize, newScale) + newCenterOfBox;
            if(ValidateNewDestination(newSecondDest)){
                Debug.Log("Use Second");
                return newSecondDest;
            }
            else{
                Debug.Log("Use Thrid");
                return RandomFirstDestination(false);
            }
        }
        
    }

    private bool ValidateNewDestination(Vector3 newDest){
        if(Vector3.Angle(Vector3.forward, newDest - transform.position) < 30 || Vector3.Angle(-Vector3.forward, newDest - transform.position) < 30){
            return false;
        }
        else{
            return true;
        }
    }


    private Vector3 RandomFirstDestination(bool isStart){
        var scale = UnityEngine.Random.insideUnitSphere;
        var x = UnityEngine.Random.value >= 0.5f ? 1 : -1;
        // var z = UnityEngine.Random.value >= 0.5f ? 1 : 0;
        Vector3 boxSize;
        Vector3 centerOfBox = CalculateBoundTarget(transform.position, out boxSize);
        if(isStart){
            return Vector3.Scale(boxSize, new Vector3(x, 0, scale.z)) + new Vector3(centerOfBox.x, initDest.y, centerOfBox.z);
        }
        else{
            return Vector3.Scale(boxSize, new Vector3(x, scale.y, scale.z)) + centerOfBox;
        }
        
    }

    private void MoveUnit(bool isNormal){
        
        var moveVector = destination - transform.position;

        Cspeed = currentVelocity.magnitude;

        if(!isRotate){
            // Dynamic speed
            if(fishType.Exists(t => t == _type)){
                if(!isSpin){
                    var Tspeed = Cspeed / 2.5f;
                    
                    if(Tspeed < 0.9f)
                        speed = Mathf.Lerp(speed, 0.9f, Time.deltaTime);
                    else if(Tspeed > 3)
                        speed = Mathf.Lerp(speed, 2f, Time.deltaTime);
                    else
                        speed = Mathf.Lerp(speed, Tspeed, Time.deltaTime);
                }
                else{
                    speed = Mathf.Lerp(speed, 1f, Time.deltaTime * 2);
                }
            }
            // else static speed random when reach to the destination

            // Prevent infinity rotate around destination
            if(isNormal)
                moveVector = Vector3.SmoothDamp(transform.forward, moveVector, ref currentVelocity, smoothDamp);
            else
                moveVector = Vector3.RotateTowards(transform.forward, moveVector, 200, 5);
            
            // movement
            // cohesion
            if(isDead)
                speed = 5;

            
            moveVector = moveVector.normalized * speed * speedRatio;
            transform.forward = moveVector;
            transform.position += moveVector * Time.deltaTime;
        }
        else{
            transform.RotateAround(rotatePoint, Vector3.up, -rotateSpeed * Time.deltaTime);
            transform.position += transform.forward * Time.deltaTime;
            transform.right = transform.position - rotatePoint;
            if(Mathf.Abs(Vector3.SignedAngle(transform.forward, moveVector, Vector3.up)) < 40)
                isRotate = false;
        }
    }

    [ContextMenu("TestSpin")]
    public void SpinRightRound(){
        if(!isSpin){
            StartCoroutine(Spin());
        }
    }

    // Shader animation spin
    IEnumerator Spin(){
        Debug.Log("Star Spin");
        isSpin = true;
        var spin = _renderer.material.GetFloat("_Spin");
        _renderer.material.SetInt("_IsFlip", 1);
        while(spin <= 6.28){
            spin += Time.deltaTime * 5;
            _renderer.material.SetFloat("_Spin", spin);
            yield return null;
        }
        _renderer.material.SetFloat("_Spin", 0);
        isSpin = false;
        _renderer.material.SetInt("_IsFlip", 0);
    }


    private void OnDrawGizmos() {
        
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, destination);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(destination, distanceThreshold);


        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 2, 0.1f);
    }

    private void OnDrawGizmosSelected() {
        // bound next target position
        
        Vector3 boxSize;
        Vector3 centerOfBox = CalculateBoundTarget(destination, out boxSize);;
        Gizmos.color = Color.grey;
        Gizmos.DrawWireCube(centerOfBox, boxSize * 2);


        // rotate point
        if(isRotate){
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(rotatePoint - Vector3.up * 2, rotatePoint + Vector3.up * 2);
            Gizmos.DrawWireSphere(rotatePoint, 0.4f);
        }
    }

    // Utillity
    private Vector3 CalculateBoundTarget(Vector3 oldDest, out Vector3 boxSize){
        Vector3 centerOfBox;
        var maxYpos = oldDest.z * 0.45f;
        var y = Mathf.Clamp(oldDest.y, -maxYpos + maxYpos * nextTargetRatio, maxYpos - maxYpos * nextTargetRatio);
        var z = Mathf.Clamp(oldDest.z, minZPos + maxDepth * nextTargetRatio, minZPos + maxDepth - maxDepth * nextTargetRatio);
        // ในกรอบ 2 ตรงแกน x
        boxSize = new Vector3(oldDest.z * Mathf.Tan(spawner.horizontalCamAngle/2 * Mathf.Deg2Rad) * 1.4f, maxYpos * nextTargetRatio, maxDepth * nextTargetRatio) ;

        centerOfBox = new Vector3(0, y, z);
        return centerOfBox;
    }
}

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
    [SerializeField]private float maxDepth;
    [SerializeField]private float minZPos;
    [SerializeField][Range (0, 1)]private float nextTargetRatio;

    [Header ("Target Setup")]
    [HideInInspector]
    [SerializeField]private Vector3 destination;
    [SerializeField]private float distanceThreshold;
    [SerializeField]private float maxRedirectTime;
    [HideInInspector][SerializeField]private float redirectTime;

    [Header ("Movement")]
    [HideInInspector][SerializeField]private Vector3 currentVelocity;
    [SerializeField]private float smoothDamp;
    [SerializeField]private float speed;
    [HideInInspector][SerializeField]private float Cspeed;
    [SerializeField]private float rotateSpeed;

    [Header ("Shader")]
    [SerializeField]private MeshRenderer _renderer;
    [SerializeField]private bool isSpin = false;
    [SerializeField]private bool isSet = false;
    [SerializeField][Range (0, 1)]private float probSpin;

    // properties
    [Header ("Properties")]
    private bool isRotate = false;
    private Vector3 rotatePoint;
    [HideInInspector]public List<UnitType> fishType;
    [HideInInspector]public List<UnitType> mammalType;
    public float aliveDuration;
    private bool isDead = false;
    [Range (0, 1)]public float speedRatio;
    
    void Start()
    {
        if(spawner){
            fishType = spawner.fishType;
            mammalType = spawner.mammalType;
        }
        SpawnMe();
    }

    public void SetInitDestination(Vector3 dest){
        destination = dest;
    }

    public void SetBaseMap(Texture baseMap){
        _renderer.material.SetTexture("_BaseMap", baseMap);
    }

    public void SpawnMe(){
        if(!isSet)
            StartCoroutine(SpawnFish());

        // Fish & Mammal

        if(fishType.Exists(t => t == _type) || mammalType.Exists(t => t == _type))
            _renderer.material.SetFloat("_DistTail", 0);
        if(_type == UnitType.Turtle)
            _renderer.material.SetFloat("_DistFin", 0);
    }

    IEnumerator SpawnFish(){
        // Set drop fish in pool
        while(Mathf.Abs(transform.position.y - destination.y) > 0.05f){
            transform.position = Vector3.Lerp(transform.position, destination, Time.deltaTime);
            yield return null;
        }
        isSet = true;
        // Start swim
        if(fishType.Exists(t => t == _type) || mammalType.Exists(t => t == _type))
            _renderer.material.SetFloat("_DistTail", 0.1f);
        if(_type == UnitType.Turtle)
            _renderer.material.SetFloat("_DistFin", 0.1f);
        _renderer.material.SetFloat("_RandomValue", UnityEngine.Random.Range(-3.14f, 3.14f));
    }

    void Update()
    {
        if(isSet){
            CheckDestination();
            aliveDuration -= Time.deltaTime;
            if(aliveDuration <= 0 && !isDead ){
                // exit events
                ExitPool();
            }
        }
            
    }

    private void ExitPool(){
        isDead = true;
        destination = new Vector3(0, 0, 20);
        spawner.allFish.Remove(this);
        StartCoroutine(LeavePool());
    }

    IEnumerator LeavePool(){
        while(transform.position.z < 17){
            yield return null;
        }
        Destroy(this.gameObject, 2);
    }

    private void CheckDestination(){

        UnitType[] arrayCanSpin = {UnitType.Tuna, UnitType.Parrotfish, UnitType.Turtle, UnitType.Dolphin};
        List<UnitType> canSpin = new List<UnitType>();
        canSpin.AddRange(arrayCanSpin);

        // check destination
        if(Vector3.Distance(transform.position, destination) < distanceThreshold){
            // random new destination
            destination = RandomNewDestination();
            redirectTime = 0;
            var moveVector = destination - transform.position;
            if(Mathf.Abs(Vector3.SignedAngle(transform.forward, moveVector, Vector3.forward)) > 170){
                // only fish and turtle can spin

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
            
            redirectTime += Time.deltaTime;
            if(redirectTime >= maxRedirectTime && maxRedirectTime != 0){
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
        
        return Vector3.Scale(boxSize, UnityEngine.Random.insideUnitSphere) + centerOfBox;
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


            
            moveVector = moveVector.normalized * speed * speedRatio;
            transform.forward = moveVector;
            transform.position += moveVector * Time.deltaTime;
        }
        else{
            transform.RotateAround(rotatePoint, Vector3.up, -rotateSpeed * Time.deltaTime);
            // transform.position += moveVector * Time.deltaTime;
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


    public void AssignSpawner(Spawner spawner){
        this.spawner = spawner;
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

    private Vector3 CalculateBoundTarget(Vector3 oldDest, out Vector3 boxSize){
        Vector3 centerOfBox;
        var maxYpos = oldDest.z * 0.45f;
        var y = Mathf.Clamp(oldDest.y, -maxYpos + maxYpos * nextTargetRatio, maxYpos - maxYpos * nextTargetRatio);
        var z = Mathf.Clamp(oldDest.z, minZPos + maxDepth * nextTargetRatio, minZPos + maxDepth - maxDepth * nextTargetRatio);
        // ในกรอบ 2 ตรงแกน x
        boxSize = new Vector3(oldDest.z * 3f, maxYpos * nextTargetRatio, maxDepth * nextTargetRatio) ;

        centerOfBox = new Vector3(0, y, z);
        return centerOfBox;
    }
}

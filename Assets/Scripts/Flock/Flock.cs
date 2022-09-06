using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    [Header ("Speed Setup")]
    [Range (1, 5)]
    [SerializeField]private float _minSpeed;
    public float minSpeed { get { return _minSpeed;}}
    [Range (1, 5)]
    [SerializeField]private float _maxSpeed;
    public float maxSpeed { get { return _maxSpeed;}}

    [Header ("Spawn Setup")]
    [SerializeField]private FlockUnit flockUnitPrefab;
    [SerializeField]private int flockSize;
    [SerializeField]private float timeLimit;
    public KeyCode keySpawn;
    [SerializeField]private bool autoGen;
    [SerializeField]private Vector3 spawnBounds;

    [Header ("Dectection Distance")]
    [Range (0, 10)]
    [SerializeField]private float _cohesionDistance;
    public float cohesionDistance { get { return _cohesionDistance; }}
    [Range (0, 10)]
    [SerializeField]private float _avoidanceDistance;
    public float avoidanceDistance { get { return _avoidanceDistance; }}
    [Range (0, 10)]
    [SerializeField]private float _aligementDistance;
    public float aligementDistance { get { return _aligementDistance; }}
    [Range (0, 10)]
    [SerializeField]private float _boundsDistance;
    public float boundsDistance { get { return _boundsDistance; }}
    [Range (0, 10)]
    [SerializeField]private float _obstacleDistance;
    public float obstacleDistance { get { return _obstacleDistance; }}

    [Header ("Behavior Weights")]
    [Range (0, 10)]
    [SerializeField]private float _cohesionWeight;
    public float cohesionWeight { get { return _cohesionWeight; }}
    [Range (0, 10)]
    [SerializeField]private float _avoidanceWeight;
    public float avoidanceWeight { get { return _avoidanceWeight; }}
    [Range (0, 10)]
    [SerializeField]private float _aligementWeight;
    public float aligementWeight { get { return _aligementWeight; }}
    [Range (0, 10)]
    [SerializeField]private float _boundsWeight;
    public float boundsWeight { get { return _boundsWeight; }}
    [Range (0, 10)]
    [SerializeField]private float _obstacleWeight;
    public float obstacleWeight { get { return _obstacleWeight; }}
    
    public List<Unit> allUnits;

    private void Start() {
        if(autoGen)
            GenerateUnits();
    }

    private void Update() {
        if(Input.GetKeyDown(keySpawn)){
            if(allUnits.Count >= flockSize){
                StartCoroutine(RemoveOldFlock(allUnits[0]));
            }
            
                GenerateUnit();
            
        }
        if(allUnits.Count > 0){
            for(int i = 0 ; i < allUnits.Count ; i++){
                if(allUnits[i].duration < 0){
                    StartCoroutine(RemoveOldFlock(allUnits[i]));   
                }
            }
            foreach(var unit in allUnits){
                if(unit.duration > 0 || timeLimit == 0){
                    unit.unit.MoveUnit();
                    unit.unit.GenerateDir();
                    if(timeLimit > 0){
                        unit.duration -= Time.deltaTime;
                    }
                }
                // else{
                //     allUnits.Remove(unit);
                //     StartCoroutine(RemoveOldFlock(unit));                       
                // }   
            }
        }
        for (int i = 0 ; i < allUnits.Count ; i++){
            
        }
    }

    IEnumerator RemoveOldFlock(Unit unit){
        allUnits.Remove(unit);
        Destroy(unit.unit.gameObject);
        while(unit.unit != null || allUnits.Count >= flockSize){
            yield return null;
        }
        // GenerateUnit();
    }

    private void GenerateUnits(){
        // allUnits = new FlockUnit[flockSize];
        for (int i = 0; i < flockSize ; i++){
            allUnits.Add(CreateUnit());
        }        
    }

    private void GenerateUnit(){

        allUnits.Add(CreateUnit());
    }

    private Unit CreateUnit(){
        var randomVector = UnityEngine.Random.insideUnitSphere;
        randomVector = new Vector3(randomVector.x * spawnBounds.x, randomVector.y * spawnBounds.y, randomVector.z * spawnBounds.z);
        var spawnPosition = transform.position + randomVector;
        var rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);

        FlockUnit unit = Instantiate(flockUnitPrefab, spawnPosition, rotation);
        unit.AssignFlock(this);
        unit.InitialzeSpeed(UnityEngine.Random.Range(minSpeed, maxSpeed));
        return new Unit(unit, timeLimit);
    }


    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, spawnBounds);
    }


    [System.Serializable]
    public class Unit{
        public FlockUnit unit;
        public float duration;
        public Unit(FlockUnit _unit, float _duration){
            unit = _unit;
            duration = _duration;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header ("Unit Setup")]
    [SerializeField]private FishUnit fishUnitPrefab;
    [Header ("Spawn Zone")]
    [SerializeField]private Vector3 spawnBounds;
    [SerializeField]private Vector3 offset;
    [SerializeField]private KeyCode keyCode;
    [Header ("Spawn Setup")]    
    [SerializeField]public List<FishUnit> allFish = new List<FishUnit>();
    [SerializeField]private bool autoSetup;
    [SerializeField]private int maxUnit;
    [SerializeField]public float maxDuration;
    [SerializeField]private List<NewFishUnit> newFishs = new List<NewFishUnit>();


    // Type Properties
    private List<UnitType> _fishType = new List<UnitType>();
    public List<UnitType> fishType {get {return _fishType;}}
    private List<UnitType> _mammalType = new List<UnitType>();
    public List<UnitType> mammalType {get {return _mammalType;}}

    void Start()
    {
        SetupUnitType();
        if(autoSetup){
            for(int i = 0 ; i < maxUnit ; i++){
                GenerateUnit();
            }
        }
    }

    private void SetupUnitType(){
        UnitType[] fishs = {UnitType.Tuna, UnitType.Parrotfish, UnitType.Shark};
        UnitType[] mammals =  {UnitType.Whale, UnitType.Dolphin, UnitType.Dugong};
        _fishType.AddRange(fishs);
        _mammalType.AddRange(mammals);
    }

    // Generate new Unit -> Check fishs in pool -> spawn



    void Update()
    {
        if(Input.GetKeyDown(keyCode)){
            // SpawnFish();
            GenerateUnit();
        }

        if(newFishs.Count > 0){
            if(allFish.Count < maxUnit){
                var newUnit = newFishs[0];
                newFishs.Remove(newUnit);
                SpawnFish(newUnit);
            }
            else{
                if(allFish[0].aliveDuration > maxDuration)
                {
                    allFish[0].aliveDuration = maxDuration;
                }
            }
        }
    }

    private void GenerateUnit(){
        Vector3 destination;
        Vector3 spawnPosition = RandomSpawnPoint(out destination);
        newFishs.Add(new NewFishUnit(destination, spawnPosition));
    }

    private Vector3 RandomSpawnPoint(out Vector3 destination){
        var scale = UnityEngine.Random.insideUnitCircle;
        var spawnOffset = transform.position + offset;
        Vector3 spawnPosition = new Vector3 (spawnBounds.x * scale.x + spawnOffset.x, 
                                        (spawnBounds.z * scale.y + spawnOffset.z) * 1.732f / 2 , 
                                        (spawnBounds.z * scale.y + spawnOffset.z));

        destination = new Vector3(scale.x * spawnBounds.x + spawnOffset.x,
                                            scale.y * spawnBounds.y + spawnOffset.y,
                                            spawnBounds.z * scale.y + spawnOffset.z);
        return spawnPosition;
    }

    private void SpawnFish(){

        var scale = UnityEngine.Random.insideUnitCircle;
        var spawnOffset = transform.position + offset;
        var spawnPosition = new Vector3 (spawnBounds.x * scale.x + spawnOffset.x, 
                                        (spawnBounds.z * scale.y + spawnOffset.z) * 1.732f / 2 , 
                                        (spawnBounds.z * scale.y + spawnOffset.z));
        
        var unit = Instantiate( fishUnitPrefab, 
                                spawnPosition,
                                Quaternion.Euler(0, 90, 0) );

        unit.SetInitDestination(new Vector3(scale.x * spawnBounds.x + spawnOffset.x,
                                            scale.y * spawnBounds.y + spawnOffset.y,
                                            spawnBounds.z * scale.y + spawnOffset.z));
        unit.AssignSpawner(this);
        allFish.Add(unit);
    }

    private void SpawnFish(NewFishUnit newfish){
        var unit = Instantiate( fishUnitPrefab, 
                                newfish.spawnPosition,
                                Quaternion.Euler(0, 90, 0) );

        unit.SetInitDestination(newfish.destination);
        unit.AssignSpawner(this);
        unit.aliveDuration = int.MaxValue;
        allFish.Add(unit);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        var centerOfBox = transform.position + offset;
        Gizmos.DrawWireCube(centerOfBox, spawnBounds * 2);
    }

    [System.Serializable]
    struct NewFishUnit{
        public Vector3 spawnPosition;
        public Vector3 destination;
        public NewFishUnit(Vector3 setDestination, Vector3 setPosition){
            spawnPosition = setPosition;
            destination = setDestination;
        }
    }
}

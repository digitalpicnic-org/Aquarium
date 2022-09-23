using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header ("Unit Setup")]
    [SerializeField]private FishUnit fishUnitPrefab;
    [Header ("Spawn Zone")]
    [SerializeField]private float spawnBound;
    [SerializeField]private float offset;
    [SerializeField]private KeyCode keyCode;
    [Header ("Spawn Setup")]    
    [SerializeField]public List<FishUnit> allFish = new List<FishUnit>();
    [SerializeField]private bool autoSetup;
    [SerializeField]private int maxUnit;
    [SerializeField]public float maxDuration;
    [SerializeField]private float fishSpeedRatio;
    [SerializeField]private List<NewFishUnit> newFishs = new List<NewFishUnit>();

    [Header ("Shader Fish")]
    [SerializeField]private Texture defaultTexture;
    [SerializeField]private Texture nextTexture;
    public string filename;

    // Type Properties
    private List<UnitType> _fishType = new List<UnitType>();
    public List<UnitType> fishType {get {return _fishType;}}
    private List<UnitType> _mammalType = new List<UnitType>();
    public List<UnitType> mammalType {get {return _mammalType;}}
    public UnitType spawnerType;
    private bool isStart = false;
    public int initUnit;
    public float horizontalCamAngle;
    public float weightSize;
    public bool isDummy;

    void Start()
    {
        if(isDummy)
            SetupUnitType();
    }

    public void SetupUnitType(){
        UnitType[] fishs = {UnitType.Tuna, UnitType.Parrotfish, UnitType.Shark};
        UnitType[] mammals =  {UnitType.Whale, UnitType.Dolphin, UnitType.Dugong};
        _fishType.AddRange(fishs);
        _mammalType.AddRange(mammals);
        
        if(autoSetup){
            for(int i = 0 ; i < initUnit ; i++){
                GenerateUnit();
            }
        }
        isStart = true;
    }

    // Generate new Unit -> Check fishs in pool -> spawn



    void Update()
    {
        if(!isStart) return;

        if(!Input.GetKey(KeyCode.Space) && Input.GetKeyDown(keyCode)){
            // SpawnFish();
            GenerateUnit();
        }

        // Fetch Texture from path
        if( Input.GetKey(KeyCode.Space) && Input.GetKeyDown(keyCode)){
            Debug.Log("owo");
            FetchNewUnit();
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

    public void FetchNewUnit(){
        if(filename != string.Empty){
            // string path = "../AquariumProject/SavedImages/" + filename;
            string path = @""+"/Users/dp-korn/Documents/Unity/Aquarium/Assets/Arts/Colored fish/remove noise/" + filename + ".png";
            var rawData = System.IO.File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2); // Create an empty Texture; size doesn't matter (she said)
            tex.LoadImage(rawData);
            if(tex){
                nextTexture = tex;
                GenerateUnit();
            }
                    
        }
    }

    public void FetchNewUnit(string path){
        if(path != string.Empty){
            // string path = "../AquariumProject/SavedImages/" + filename;
            // string path = @""+"/Users/dp-korn/Documents/Unity/Aquarium/Assets/Arts/Colored fish/remove noise/" + filename + ".png";
            var rawData = System.IO.File.ReadAllBytes(@""+path);
            Texture2D tex = new Texture2D(2, 2); // Create an empty Texture; size doesn't matter (she said)
            tex.LoadImage(rawData);
            if(tex){
                nextTexture = tex;
                GenerateUnit();
            }
                    
        }
    }

    private void GenerateUnit(){
        Vector3 destination;
        Vector3 spawnPosition = RandomSpawnPoint(out destination);
        if(nextTexture)
            newFishs.Add(new NewFishUnit(destination, spawnPosition, nextTexture));
        else
            newFishs.Add(new NewFishUnit(destination, spawnPosition, defaultTexture));
        nextTexture = null;
    }

    private Vector3 RandomSpawnPoint(out Vector3 destination){
        var scale = UnityEngine.Random.insideUnitSphere;
        var spawnOffset = transform.position + Vector3.forward * offset;

        var zPos = spawnBound/2 * scale.z + spawnOffset.z;
        // x = tan horizontal y = 60

        Vector3 spawnPosition = new Vector3 (zPos * Mathf.Tan(horizontalCamAngle/2 * Mathf.Deg2Rad) * 0.8f * scale.x + spawnOffset.x, 
                                        zPos / 1.732f + spawnOffset.y, 
                                        zPos );

        destination = new Vector3(  zPos * Mathf.Tan(horizontalCamAngle/2 * Mathf.Deg2Rad) * 0.8f * scale.x + spawnOffset.x, 
                                    zPos / 2.5f * scale.y + spawnOffset.y,
                                    zPos );
        return spawnPosition;
    }

    // private void SpawnFish(){

    //     var scale = UnityEngine.Random.insideUnitCircle;
    //     var spawnOffset = transform.position + Vector3.forward * offset;
    //     var spawnPosition = new Vector3 (spawnBound.x * scale.x + spawnOffset.x, 
    //                                     (spawnBounds.z * scale.y + spawnOffset.z) * 1.732f / 2 , 
    //                                     (spawnBounds.z * scale.y + spawnOffset.z));
        
    //     var unit = Instantiate( fishUnitPrefab, 
    //                             spawnPosition,
    //                             Quaternion.Euler(0, 90, 0) );

    //     unit.SetInitDestination(new Vector3(scale.x * spawnBounds.x + spawnOffset.x,
    //                                         scale.y * spawnBounds.y + spawnOffset.y,
    //                                         spawnBounds.z * scale.y + spawnOffset.z));
    //     unit.AssignSpawner(this);
    //     unit.speedRatio = fishSpeedRatio;
    //     allFish.Add(unit);
    // }

    private void SpawnFish(NewFishUnit newfish){
        var unit = Instantiate( fishUnitPrefab, 
                                newfish.spawnPosition,
                                Quaternion.Euler(0, 90, 0) );
        unit.transform.localScale *= weightSize;
        unit.SetInitDestination(newfish.destination);
        unit.AssignSpawner(this);
        unit.aliveDuration = int.MaxValue;
        unit.SetBaseMap(newfish.texture);
        unit.speedRatio = fishSpeedRatio;
        allFish.Add(unit);
    }

    public void SetConfig(SpawnSetUp setup){
        this.spawnBound = setup.spawnBound;
        this.offset = setup.zOffset;
        this.maxUnit = setup.maxUnit;
        if(initUnit >= maxUnit){
            this.initUnit = maxUnit;
        }
        else{
            this.initUnit = setup.initUnit;
        }
        this.maxDuration = setup.maxDuration;
        this.fishSpeedRatio = setup.fishSpeedRatio;
        this.autoSetup = setup.autoSetup;
        this.weightSize = setup.weightSize;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        var centerOfBox = transform.position + Vector3.forward * offset;
        Vector3 cubesize = new Vector3(centerOfBox.z * Mathf.Tan(horizontalCamAngle/2 * Mathf.Deg2Rad) * 0.8f, centerOfBox.z / 2.5f, spawnBound/2);
        Gizmos.DrawWireCube(centerOfBox, cubesize * 2);
    }

    [System.Serializable]
    struct NewFishUnit{
        public Vector3 spawnPosition;
        public Vector3 destination;
        public Texture texture;
        
        public NewFishUnit(Vector3 setDestination, Vector3 setPosition){
            spawnPosition = setPosition;
            destination = setDestination;
            texture = null;
        }
        public NewFishUnit(Vector3 setDestination, Vector3 setPosition, Texture setTexture){
            spawnPosition = setPosition;
            destination = setDestination;
            texture = setTexture;
        }
    }
}

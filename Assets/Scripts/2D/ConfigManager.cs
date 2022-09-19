using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConfigManager : MonoBehaviour
{
    public bool isEditor;
    [SerializeField]private List<Spawner> spawners;
    public SpawnList spawnList;
    
    private void Start() {
#if UNITY_EDITOR
        if(isEditor){
            foreach(var setup in spawnList.spawnSetUps){
                SetSpawner(setup);
            }
            foreach(var spawner in spawners){
                spawner.SetupUnitType();
            }
        }
        else{
            StartCoroutine(SetupSpawners());
        }
#else
        StartCoroutine(SetupSpawners());
#endif
        
    }

    IEnumerator SetupSpawners(){
        string path;
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
        path = @"" + Application.dataPath + "/Resources/Data/StreamingAssets" + "/config.json";
#elif UNITY_STANDALONE && !UNITY_EDITOR
        path = @"" + Application.dataPath + "/StreamingAssets" + "/config.json";
#else
        path = @"" + Application.dataPath + "/StreamingAssets/config.json";
#endif
        try{

            string strInput = File.ReadAllText(path);

            spawnList = JsonUtility.FromJson<SpawnList>(strInput);
            Debug.Log("Read file");
        }
        catch{
            string strOutput = JsonUtility.ToJson(spawnList);

            File.WriteAllText(path, strOutput);
            Debug.Log("Create new file");
        }
        yield return new WaitForSeconds(0.5f);
        foreach(var setup in spawnList.spawnSetUps){
            SetSpawner(setup);
        }
        foreach(var spawner in spawners){
            spawner.SetupUnitType();
        }
    }

    private void SetSpawner(SpawnSetUp setup){
        var spawner = spawners.Find(spawner => spawner.spawnerType == setup.unitType);
        spawner.SetConfig(setup);
    }

    [System.Serializable]
    public class SpawnList{
        public SpawnSetUp[] spawnSetUps;
    }

    // set from config file
    
}

[System.Serializable]
    public class SpawnSetUp{
        public UnitType unitType;
        public float spawnBound;
        public float zOffset;
        public int maxUnit;
        public int initUnit;
        public float maxDuration;
        public float fishSpeedRatio;
        public bool autoSetup;
        public float wieghtSize;
        public string name;
        public SpawnSetUp(string name,int type, float bound, float offset, int maxUnit, int initUnit, float maxDuration, float fishSpeedRatio, bool autoSetup, float wieghtSize){
            this.unitType = (UnitType)type;
            this.spawnBound = bound;
            this.zOffset = offset;
            this.maxUnit = maxUnit;
            this.initUnit = initUnit;
            this.maxDuration = maxDuration;
            this.fishSpeedRatio = fishSpeedRatio;
            this.autoSetup = autoSetup;
            this.wieghtSize = wieghtSize;
            this.name = name;
        }
    }
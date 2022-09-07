using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConfigManager : MonoBehaviour
{
    [SerializeField]private List<Spawner> spawners;
    public SpawnList spawnList;
    
    private void Start() {
        StartCoroutine(SetupSpawners());
    }

    IEnumerator SetupSpawners(){
        try{
            string strInput = File.ReadAllText(Application.dataPath + "/config.json");
            spawnList = JsonUtility.FromJson<SpawnList>(strInput);
            Debug.Log("Read file");
        }
        catch{
            string strOutput = JsonUtility.ToJson(spawnList);

            File.WriteAllText(Application.dataPath + "/config.json", strOutput);
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
        public float maxDuration;
        public float fishSpeedRatio;
        public bool autoSetup;

        public SpawnSetUp(int type, float bound, float offset, int maxUnit, float maxDuration, float fishSpeedRatio, bool autoSetup){
            this.unitType = (UnitType)type;
            this.spawnBound = bound;
            this.zOffset = offset;
            this.maxUnit = maxUnit;
            this.maxDuration = maxDuration;
            this.fishSpeedRatio = fishSpeedRatio;
            this.autoSetup = autoSetup;
        }
    }
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class IOConfigFile : MonoBehaviour
{
    [System.Serializable]
    public class Message{
        public int id;
        public string locate;
    }

    public  Message m;
    public TextMeshProUGUI text;
    
    public void WriteFile(){
        string strOutput = JsonUtility.ToJson(m);

        File.WriteAllText(Application.dataPath + "/config.txt", strOutput);
        text.text = Application.dataPath + "/config.txt";   

    }

    public void FetchFile(){
        try{
            string strInput = File.ReadAllText(Application.dataPath + "/config.txt");
            text.text = strInput;
        }
        catch{
            text.text = "didn't have file";
        }
    }
}

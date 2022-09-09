using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.Networking;

public class IOConfigFile : MonoBehaviour
{
    [System.Serializable]
    public class Message{
        public int id;
        public string locate;
    }

    public Texture a;
    private void Start() {
        // StartCoroutine(GetTexture());
        // string filename = @"/Users/dp-korn/Documents/Unity/AquariumProject/SavedImages/img_20220906-121600.png";
        // var rawData = System.IO.File.ReadAllBytes(filename);
        // Texture2D tex = new Texture2D(2, 2); // Create an empty Texture; size doesn't matter (she said)
        // tex.LoadImage(rawData);
        // if(tex)
        //     Debug.Log("owo");
        // a = tex;

        DirectoryInfo dir = new DirectoryInfo(@"/Users/dp-korn/Documents/Unity/AquariumProject/SavedImages");
        Debug.Log(dir.GetFiles().Length);
    }

    IEnumerator GetTexture(){
        UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("../AquariumProject/result.png");//file://Users/dp-korn/Documents/Unity/AquariumProject/result.png
        yield return uwr.SendWebRequest();

        if(uwr.result != UnityWebRequest.Result.Success){
            Debug.Log(uwr.error);
        }
        else{
            a = DownloadHandlerTexture.GetContent(uwr);
        }
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

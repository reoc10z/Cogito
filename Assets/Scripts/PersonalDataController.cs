using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class PersonalDataController : MonoBehaviour
{
    // general variables
    private string _logs = "";
    private string _pathLogFile;
    private string _pathTestVersionFile;
    public Text WarningText;
    public Button BtnStartGame;
    // variables for input data
    public GameObject TestVersion;
    public InputField Age;
    public InputField Work;
    public GameObject Sex;
    public GameObject Hand;

    // Start is called before the first frame update
    void Start()
    {
        _pathLogFile = GetPathFile("Log.txt");
        _pathTestVersionFile = GetPathFile("SettingsTestVersion.txt");
        BtnStartGame.onClick.AddListener(GoToNextScene);
    }
    
    // append new message to the general log message 
    private void ToLog(string msg)
    {
        msg = "" + 0 +" " + msg + "\n";
        _logs += msg;
        print(msg);
    }
    
    // writeLog assumes log file was already created under the path in _pathLogFile
    private void WriteLog()
    {
        WriteFile(_pathLogFile, _logs, "a");
        _logs = "";
    }
    
    // type = a, for appending
    // type = r, for replace
    private void WriteFile(string filePath, string msg, string type)
    {
        if (type == "a")
            File.AppendAllText(filePath, msg);
        else if (type=="r")
            File.WriteAllText(filePath, msg);
    }


    private string GetPathFile(string fileName)
    {
        string filePath;
        if ( SystemInfo.deviceModel == "PC")
        {
            filePath =  Application.dataPath + "/" + fileName;
        }
        else
        {
            filePath = Application.persistentDataPath + "/" + fileName;
        }

        return filePath;
    }
    
    private string GetSelectedToggle(GameObject tG)
    {
        Toggle[] toggles= tG.GetComponentsInChildren<Toggle>();
        foreach (var t in toggles)
        {
            //returns selected toggle
            if (t.isOn) return t.name;
        }
        return null;           // if nothing is selected return null
    }
    
    private void GoToNextScene()
    {
        //check if empty input
        if (
            GetSelectedToggle(TestVersion) is null || 
            Age.text=="" || 
            Work.text =="" || 
            GetSelectedToggle(Sex) is null ||
            GetSelectedToggle(Hand) is null
            )
        {
            WarningText.text = "Llena todos los campos";
            return;
        }
        // get data from input
        string testVersion = GetSelectedToggle(TestVersion); // a)base, b)H, c)A, d)HA
        string age = Age.text;
        string work = Work.text;
        string sex = GetSelectedToggle(Sex);
        string hand = GetSelectedToggle(Hand);
    
        // logs
        ToLog("_0_ test version _ " + testVersion);
        ToLog("_0_ age _ " + age);
        ToLog("_0_ work _ " + work);
        ToLog("_0_ sex _ " + sex);
        ToLog("_0_ hand dominant _ " + hand);
        WriteLog();
        
        // set file for game type setting: base, H, A, HA
        WriteFile(_pathTestVersionFile, testVersion,"r");
        
        // go to next scene
        Loader.Load(Loader.Scene.GameScene);
    }
    
}

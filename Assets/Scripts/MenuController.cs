using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MenuController : MonoBehaviour
{
    // general variables
    public Button BtnTraining;
    public Button BtnStartTest;
    private string _pathLogFile;
    public Text TextforPath;
    public Button BtnExit;
    public Dropdown DropBorrar; // TODO : remove this and related code
    private string _logs = "";
    void Awake()
    {
        BtnTraining.onClick.AddListener(GoToTraining);
        BtnStartTest.onClick.AddListener(GoToNextScene);
        BtnExit.onClick.AddListener(ExitGame);
    }
    // Start is called before the first frame update
    void Start()
    {
        //log file
        _pathLogFile  = CreateFile("Log.txt");
        TextforPath.text = _pathLogFile;
        
        // initial logs
        ToLog("");
        ToLog("");
        ToLog("");
        ToLog("_0_ app starts _ " + System.DateTime.Now );
        ToLog("_0_ screen size (w,h) _ " + Screen.width + " , " + Screen.height);
        ToLog("_0_ mobile type _ " + SystemInfo.deviceModel);
        ToLog("_0_ android version _ " + SystemInfo.operatingSystem);
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
    
    //t ype = 0 for a setting file.
    // type = 1 for log file.
    // if file already exists, it returns that path.
    private string CreateFile(string fileName)
    {
        string filePath = GetPathFile(fileName);
        
        if (!File.Exists(filePath)) {
            //Create File if it doesn't exist
            File.WriteAllText(filePath, "");
        }
        
        return filePath;
    }

    private void GoToTraining()
    {
        // setting file for level. Set level to -1
        WriteFile( CreateFile("SettingsLevel.txt") , "-1", "r");
        
        // set file for game type setting: base, H, A, HA
        WriteFile( CreateFile("SettingsTestVersion.txt") , "", "r");
        
        ToLog("_0_ testing starts");
        WriteLog();
        Loader.Load(Loader.Scene.GameScene);
    }
    
    private void GoToNextScene()
    {
        // ------- remove code from here --------
        int selectedLevel = DropBorrar.value;
        print(selectedLevel);
        if (selectedLevel > 0)
        {
            // setting file for level. Set level to 0
            WriteFile( CreateFile("SettingsLevel.txt") , ""+selectedLevel, "r");

            // create setting file for game type: base, haptic, auditory, haptic-auditory
            // this file will be modified in personalDataController
            WriteFile( CreateFile("SettingsTestVersion.txt") , "HA", "r");
            WriteLog();
            Loader.Load(Loader.Scene.GameScene);
            return;
        }
        // --------    up to here    --------- 
        
        
        // setting file for level. Set level to 0
        WriteFile( CreateFile("SettingsLevel.txt") , "0", "r");

        // create setting file for game type: base, haptic, auditory, haptic-auditory
        // this file will be modified in personalDataController
        WriteFile( CreateFile("SettingsTestVersion.txt") , "", "r");
        
        WriteLog();
        Loader.Load(Loader.Scene.PlayerDataScene);
    }

    private void ExitGame()
    {
        WriteLog();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
}

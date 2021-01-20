using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MenuController : MonoBehaviour
{
    // general variables
    public Button BtnNext;
    private string _pathLogFile;
    public Text TextforPath;
    public Button BtnExit;
    public Button BtnBorrar;
    private string _logs = "";
    void Awake()
    {
        BtnNext.onClick.AddListener(GoToNextScene);
        BtnExit.onClick.AddListener(ExitGame);
        BtnBorrar.onClick.AddListener(BorrarEsto);
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
        ToLog("-0- app starts: " + System.DateTime.Now );
        ToLog("-0- screen size (w,h): " + Screen.width + "," + Screen.height);
        ToLog("-0- mobile type: " + SystemInfo.deviceModel);
        ToLog("-0- android version: " + SystemInfo.operatingSystem);
        
        // setting file for level. Set level to 0
        WriteFile( CreateFile("SettingsLevel.txt") , "0", "r");

        // create setting file for game type: A-none, B-haptic, C-auditory, D-haptic-auditory
        // this file will be modified in personalDataController
        WriteFile( CreateFile("SettingsTestVersion.txt") , "", "r");
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
    
    private void GoToNextScene()
    {
        WriteLog();
        Loader.Load(Loader.Scene.PlayerDataScene);
    }


    private void BorrarEsto()
    {
        WriteLog();
        Loader.Load(Loader.Scene.QuestionnaireScene);
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

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
    public Button BtnShareLog;
    private string _pathLogFile;
    public Button BtnExit;
    private string _logs = "";
#if UNITY_ANDROID
    private AndroidNativeVolumeService sound = new AndroidNativeVolumeService();
#endif
    void Awake()
    {
        BtnTraining.onClick.AddListener(GoToTraining);
        BtnStartTest.onClick.AddListener(GoToGame);
        BtnShareLog.onClick.AddListener(GoToShare);
        BtnExit.onClick.AddListener(ExitGame);
    }
    // Start is called before the first frame update
    void Start()
    {
        //log file
        _pathLogFile  = CreateFile("Log.txt");

        // initial logs
        ToLog("_0_ app starts _ " + System.DateTime.Now + "_NA" );
        ToLog("_0_ screen size (w,h) _ " + Screen.width + " , " + Screen.height + "_NA");
        ToLog("_0_ mobile type _ " + SystemInfo.deviceModel + "_NA");
        ToLog("_0_ android version _ " + SystemInfo.operatingSystem + "_NA");
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
        
        ToLog("_0_ testing starts_NA_NA");
        WriteLog();
        Loader.Load(Loader.Scene.CalibrationScene);
    }
    
    private void GoToGame()
    {
#if UNITY_ANDROID
        // get volume intensity
        sound.GetSystemVolume();
        float vol = 100.0f * sound.GetSystemVolume();
        ToLog("_0_ volume (%) _ " + vol.ToString("0.00")+"_NA" );
#endif
        // setting file for level. Set level to 0
        WriteFile( CreateFile("SettingsLevel.txt") , "0", "r");

        // create setting file for game type: base, haptic, auditory, haptic-auditory
        // this file will be modified in personalDataController
        WriteFile( CreateFile("SettingsTestVersion.txt") , "", "r");
        
        WriteLog();
        Loader.Load(Loader.Scene.PlayerDataScene);
    }

    private void GoToShare()
    {
        //ToLog("_0_ testing starts");
        //WriteLog();
        
        Loader.Load(Loader.Scene.SharingScene);
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

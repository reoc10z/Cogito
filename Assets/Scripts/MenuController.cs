using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.CompilerServices;

public class MenuController : MonoBehaviour
{
    // general variables
    public Button btnCalibration;
    public Button btnTraining;
    public Button btnStartTest;
    public Button btnShareLog;
    public Button btnExit;
    public Text[] txtSteps = new Text[5];
    private string _pathLogFile;
    private string _pathSettingsHapticDelayFile;
    private string _logs = "";
#if UNITY_ANDROID
    private AndroidNativeVolumeService sound = new AndroidNativeVolumeService();
#endif
    void Awake()
    {
        btnCalibration.onClick.AddListener(GoToCalibration);
        btnTraining.onClick.AddListener(GoToTraining);
        btnStartTest.onClick.AddListener(GoToGame);
        btnShareLog.onClick.AddListener(GoToShare);
        btnExit.onClick.AddListener(ExitGame);
        ActivateButton(1);
    }
    // Start is called before the first frame update
    void Start()
    {
        // steps file
        var step =  ReadFile( CreateFile("SettingsStep.txt") );
        ActivateButton( int.Parse(step) );
        
        //log file
        _pathLogFile  = CreateFile("Log.txt");
        
        // create setting file for delay time for the haptic stimulus
        // this file will be modified in CalibrationController and read by CogitoController 
        _pathSettingsHapticDelayFile = CreateFile("SettingsHapticDelay.txt");

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
    
    private string ReadFile(string filePath)
    {
        // read filepath and return all text as one string
        return System.IO.File.ReadAllText(filePath);
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


    private void GoToCalibration()
    {
        Loader.Load(Loader.Scene.CalibrationScene);
    }
    
    
    private void GoToTraining()
    {
        // setting file for level. Set level to -1
        WriteFile( CreateFile("SettingsLevel.txt") , "-1", "r");
        //WriteFile( CreateFile("SettingsLevel.txt") , "2", "r"); // todo: remove this line
        
        // set file for game type setting: base, H, A, HA
        WriteFile( CreateFile("SettingsTestVersion.txt") , "", "r");
        //WriteFile( CreateFile("SettingsTestVersion.txt") , "HapticAuditory", "r"); // todo: remove this line
        //WriteFile( CreateFile("SettingsTestVersion.txt") , "Auditory", "r"); // todo: remove this line
        //WriteFile( CreateFile("SettingsTestVersion.txt") , "Haptic", "r"); // todo: remove this line
        
        ToLog("_0_ practice starts_NA_NA");
        WriteLog();

        Loader.Load(Loader.Scene.GameScene);
    }
    
    private void GoToGame()
    {
        ToLog("_0_ test starts_NA_NA");
        WriteLog();
        
#if !UNITY_EDITOR
        // get volume intensity
        float vol = 100.0f * sound.GetSystemVolume();
        ToLog("_0_ volume (%) _ " + vol.ToString("0.00")+"_NA" );
#endif
        // setting file for level. Set level to 0
        WriteFile( CreateFile("SettingsLevel.txt") , "0", "r");

        // create setting file for game type: base, haptic, auditory, haptic-auditory
        // this file will be modified in personalDataController
        WriteFile( CreateFile("SettingsTestVersion.txt") , "", "r");
        
        // set value to time delay for haptic stimulus
        string hapticsDelay = ReadFile(_pathSettingsHapticDelayFile);
        if (hapticsDelay == "")
        {
            hapticsDelay = "0";
            // if empty, write 0 as delay for haptics
            WriteFile(_pathSettingsHapticDelayFile, hapticsDelay, "r");
        }
        ToLog("_0_ delay for haptics (ms) _ " + hapticsDelay +"_NA" );

        WriteLog();
        Loader.Load(Loader.Scene.PlayerDataScene);
    }

    private void GoToShare()
    {
        Loader.Load(Loader.Scene.SharingScene);
    }
    
    private void ExitGame()
    {
        WriteLog();
        WriteFile( CreateFile("SettingsStep.txt"), "", "r");   
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ActivateButton(int idButton)
    {
        switch (idButton)
        {
            case 1:
                btnCalibration.interactable = true;
                btnTraining.interactable = false;
                btnStartTest.interactable = false;
                break;
            
            case 2:
                btnCalibration.interactable = false;
                btnTraining.interactable = true;
                btnStartTest.interactable = false;
                break;
            
            case 3:
                btnCalibration.interactable = false;
                btnTraining.interactable = false;
                btnStartTest.interactable = true;
                break;
            
            case 4:
            case 5:
                btnCalibration.interactable = false;
                btnTraining.interactable = false;
                btnStartTest.interactable = false;
                break;
        }

        // activate indicator for next step
        foreach (var txt in txtSteps)
            txt.gameObject.SetActive(false);
        txtSteps[idButton-1].gameObject.SetActive(true);
    }
    
}

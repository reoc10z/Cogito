using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class CalibrationController2 : MonoBehaviour
{
    public Button btnNext;
    public AudioSource backgroundSound;
    public Text textVol;
    public Text textWarning;

#if UNITY_ANDROID
    private AndroidNativeVolumeService sound = new AndroidNativeVolumeService();
#endif
    
    // Start is called before the first frame update
    void Start()
    {
        // keep screen always active
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        btnNext.onClick.AddListener(ClickOnNext);
        btnNext.interactable = false;
    }
    
        
    void Update()
    {
#if UNITY_ANDROID
        float vol = 100.0f * sound.GetSystemVolume();
        textVol.text = "Volúmen: " + vol.ToString("0.00") + " %";
        
        // test if headset is plugged
        // Plugin for headset detection was downloaded from: https://github.com/DaVikingCode/UnityDetectHeadset
        bool isHeadset = DetectHeadset.Detect();
        if (isHeadset)
        {
            btnNext.interactable = true;
            textWarning.text = "";
        }
        else
        {
            btnNext.interactable = false;
            textWarning.text = "Ahora, mantén conectados tus audífonos de cable";
        }
#endif
    }
    
    
    private void ClickOnNext()
    {
        backgroundSound.Stop();
        GoToNextScene();
    }
    
    private void GoToNextScene()
    {
        // end calibration
        WriteFile( CreateFile("SettingsStep.txt"), "2", "r");
        Loader.Load(Loader.Scene.MenuScene);
    }
    
    private void WriteFile(string filePath, string msg, string type)
    {
        if (type == "a")
            File.AppendAllText(filePath, msg);
        else if (type=="r")
            File.WriteAllText(filePath, msg);
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
}

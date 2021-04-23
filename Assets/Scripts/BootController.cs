using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BootController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // steps file
        WriteFile( CreateFile("SettingsStep.txt") , "1", "r" );
        GoToMenu();
    }

    private void GoToMenu()
    {
        Loader.Load(Loader.Scene.MenuScene);
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
    
}

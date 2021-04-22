using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class NativeShareScript : MonoBehaviour
{
    private string _pathLogFile;
    public Text TxtPath;
    public Button BtnMenu;

    public void Awake()
    {
        _pathLogFile = GetPathFile("Log.txt");
        BtnMenu.onClick.AddListener(GoToMenu);
    }

    void Start()
    {
        TxtPath.text = _pathLogFile;
        ShareBtnPress();
    }
    
    private string GetPathFile(string fileName) 
    {
        if ( SystemInfo.deviceModel == "PC")
        {
            return  Application.dataPath + "/" + fileName;
        }
        else
        {
            return Application.persistentDataPath + "/" + fileName;
        }
    }
    
    private void ShareBtnPress()
    {
        StartCoroutine(LoadFileAndShare());
    }
    
    private IEnumerator LoadFileAndShare()
    {
        yield return new WaitForEndOfFrame();
        print("sharing...");
        
        var shareSubject = "Enviar a: rafael.theoc@gmail.com: Resultados de prueba con proyecto Cogito";
        var shareMessage = "Seleccionar app correo"+
                           "\nCorreo a: rafael.theoc@gmail.com"+
                           "\n\n!Hola!"+
                           "\n\nNo he modificado el archivo log y lo anexo al correo."+
                           "\n\nA continuación escribo algún comentario extra que tengo sobre el experimento:"+
                           "\n\n"+
                           "\n\n"+
                           "\n\n-------------------------------------------------------"
                           ;
        
        new NativeShare().AddFile( _pathLogFile )
            .SetTitle("Select email app")
            .SetSubject( shareSubject )
            .SetText( shareMessage )
            .SetCallback( ( result, shareTarget ) => Debug.Log( "Share result: " + result + ", selected app: " + shareTarget ) )
            //.SetCallback( ( result, shareTarget ) => TxtPath.text = ""+result ) // set result from sharing in txt box. Note: this result is not reliable!!!
            .Share();
    }
    
    private void GoToMenu()
    {
        WriteFile( CreateFile("SettingsStep.txt"), "5", "r");   
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
    
}

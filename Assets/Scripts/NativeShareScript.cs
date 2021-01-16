﻿using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

public class NativeShareScript : MonoBehaviour {
    public GameObject CanvasShareObj;
    private bool isProcessing = false;
    private bool isFocus = false;
    //public GameObject CodeObjetc;
    //private CogitoController Cogito;
    private string _logPathFile;

    public void Awake()
    {
        //Cogito = CodeObjetc.GetComponent<CogitoController>();
    }

    public void SetPath(string path)
    {
        // TODO this should be read from a settings file
        _logPathFile = path;
    }
    
    public void ShareBtnPress()
    {
        if (!isProcessing)
        {
            CanvasShareObj.SetActive(true);
            StartCoroutine(ShareTextInAndroid());
        }
    }

    private string ReadContentLogFile(string filePath)
    {
        // read filepath and return all text as one string
        return System.IO.File.ReadAllText(filePath);
    }

#if UNITY_ANDROID
    private IEnumerator ShareTextInAndroid () {
        
        //string logPathFile = Cogito.GetLogPathFile();
        print("sharing...");

        var shareSubject = "Enviar a: rafael.theoc@gmail.com: Resultados de prueba con proyecto Cogito";
        var shareMessage = "Correo a: rafael.theoc@gmail.com"+
                            "\n\n!Hola!"+
                            "\n\nNo he modificado el siguiente contenido."+
                            "\n\nA continuación se incluyen los datos de mi archivo log:"+
                            "\n\n-------------------------------------------------------"+
                            "\n\n";
        shareMessage += ReadContentLogFile(_logPathFile);

        isProcessing = true;
        yield return new WaitForEndOfFrame();
        
        if (!Application.isEditor) {
            //Create intent for action send
            AndroidJavaClass   intentClass = new AndroidJavaClass ("android.content.Intent");
            AndroidJavaObject intentObject = new AndroidJavaObject ("android.content.Intent");
            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string> ("ACTION_SEND"));

            //put text and subject extra
            intentObject.Call<AndroidJavaObject> ("setType", "text/plain");
            intentObject.Call<AndroidJavaObject> ("putExtra", intentClass.GetStatic<string> ("EXTRA_SUBJECT"), shareSubject);
            intentObject.Call<AndroidJavaObject> ("putExtra", intentClass.GetStatic<string> ("EXTRA_TEXT"), shareMessage);

            //call createChooser method of activity class
            AndroidJavaClass unity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject> ("currentActivity");
            AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject> ("createChooser", intentObject, "Share your high score");
            currentActivity.Call ("startActivity", chooser);
        }

        yield return new WaitUntil (() => isFocus);
        CanvasShareObj.SetActive(false);
        isProcessing = false;
    }
 
    private void OnApplicationFocus(bool focus)
    {
        isFocus = focus;
    }
#endif
    
}
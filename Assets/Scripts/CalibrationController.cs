using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationController : MonoBehaviour
{
    public Button BtnNext;
    public AudioSource BackgroundSound;
    public Text textVol;
#if UNITY_ANDROID
    private AndroidNativeVolumeService sound = new AndroidNativeVolumeService();
#endif
    
    // Start is called before the first frame update
    void Start()
    {
        BtnNext.onClick.AddListener(GoToNextScene);
        BackgroundSound.Play();
    }

    void Update()
    {
#if UNITY_ANDROID
        float vol = 100.0f * sound.GetSystemVolume();
        textVol.text = "Volúmen: " + vol.ToString("0.00") + " %";
#endif
    }
    
    private void GoToNextScene()
    {
        BackgroundSound.Stop();
        Loader.Load(Loader.Scene.GameScene);
    }
}

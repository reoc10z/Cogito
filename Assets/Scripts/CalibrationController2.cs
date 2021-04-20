using UnityEngine;
using UnityEngine.UI;

public class CalibrationController2 : MonoBehaviour
{
    public Button btnNext;
    public AudioSource backgroundSound;
    public Text textVol;

#if UNITY_ANDROID
    private AndroidNativeVolumeService sound = new AndroidNativeVolumeService();
#endif
    
    // Start is called before the first frame update
    void Start()
    {
        // keep screen always active
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        btnNext.onClick.AddListener(ClickOnNext);
    }
    
        
    void Update()
    {
#if UNITY_ANDROID
        float vol = 100.0f * sound.GetSystemVolume();
        textVol.text = "Volúmen: " + vol.ToString("0.00") + " %";
#endif
    }
    
    
    private void ClickOnNext()
    {
        backgroundSound.Stop();
        GoToNextScene();
    }
    
    private void GoToNextScene()
    {
        Loader.Load(Loader.Scene.GameScene);
    }
    
}

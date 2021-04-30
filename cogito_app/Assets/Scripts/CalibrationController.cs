// #define ANDROID // comment this to emulate in unity editor

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
using System.IO;
using System.Linq;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif


public class CalibrationController : MonoBehaviour
{
    public Button btnNext;
    public AudioSource beepSound;
    public Text textVol;
    public Text textInformation;
    public Button btnAutoCalibration;
    public Text textWarning;
    public Toggle toggleOK;
    
#if UNITY_ANDROID
    private AndroidNativeVolumeService sound = new AndroidNativeVolumeService();
    private bool _microphPermission = false;
#else
    private bool _microphPermission = true;
#endif

    private GameObject dialog = null;
    private int _hapticDelay = 0;
    private int _hapticDelayTemp = 0;
    private Stopwatch _zeit = new Stopwatch();
    private long _deltaFramesTime = 0;
    private string _pathSettingsHapticDelayFile;
    private bool _toggleMsg = false;
    private AudioSource _audioMicrophone;
    private readonly int _nCalibrationRecs = 3;
    private int _kRecord = 0;
    private bool _ready2Record = false;
    private int _calibratingStep = -1;
    private readonly int _sampleRate = 44100;
    private float[] _dataAudioOriginal;
    private int _stage = 0;

    private void Awake()
    {
        textVol.gameObject.SetActive(false);
        btnAutoCalibration.gameObject.SetActive(false);
        toggleOK.isOn = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        // keep screen always active
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        _pathSettingsHapticDelayFile = GetPathFile("SettingsHapticDelay.txt");
        btnAutoCalibration.onClick.AddListener(AutoCalibration);
        btnNext.onClick.AddListener(ClickOnNext);

        _dataAudioOriginal = new float[beepSound.clip.samples];
        beepSound.clip.GetData(_dataAudioOriginal, 0);
        
#if UNITY_ANDROID 
        // ask permission to microphones access
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
            dialog = new GameObject();
        }
#endif
        
        _audioMicrophone = GetComponent<AudioSource>();
        _audioMicrophone.loop = false;
        
        _zeit.Start();
        
    }
    
    void Update()
    {
        switch (_stage)
        {
            case 0:
                // stage for introduction to calibrating
#if UNITY_ANDROID
                if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
                {
                    textWarning.text = "Para continuar acepta los permisos del micrófono.";
                    _microphPermission = false;
                }
                else
                {
                    textWarning.text = "";
                    _microphPermission = true;
                }
#endif
                break;
            
            case 1:
                // stage to calibrate
                if (_calibratingStep == 1)
                {
                    if (_kRecord < _nCalibrationRecs)
                    {
                        if (_ready2Record)
                        {
                            _audioMicrophone.clip = Microphone.Start("", false, 1, _sampleRate);
                    
                            // play sound
                            beepSound.Play();
                            _ready2Record = false;
                            _deltaFramesTime = _zeit.ElapsedMilliseconds; //timestamp
                            ShowDelayValue("...calculando...");
                        }
                        else
                        {
                            // when last recording finished, compute delay, then go to next recording
                            if (Microphone.IsRecording("") == false)
                            {
                                float[] dataAudioRecorded = new float[_audioMicrophone.clip.samples];
                                _audioMicrophone.clip.GetData(dataAudioRecorded, 0);
                                int delay = ComputeAudioDelay(_dataAudioOriginal, dataAudioRecorded);
                                print(delay);
                                _hapticDelayTemp += delay;
                                _ready2Record = true;
                                _kRecord++;
                            }
                        }
                    }
                    else
                    {
                        _audioMicrophone.clip = null;
                        _calibratingStep = 2;
                    }
                } else if (_calibratingStep == 2)
                {
                    _hapticDelay = -1*_hapticDelayTemp / _nCalibrationRecs;
                    // showing computed delay
                    ShowDelayValue( _hapticDelay.ToString() );
                    ActivatedButtons(true);
                    _calibratingStep = -1;
                }
                else
                {
#if UNITY_ANDROID
                    // test if headset is plugged
                    // Plugin for headset detection was downloaded from: https://github.com/DaVikingCode/UnityDetectHeadset
                    bool isHeadset = DetectHeadset.Detect();
#else
            bool isHeadset = false;
#endif
                    if (isHeadset)
                    {
                        textWarning.text = "¡Desconecta tus audífonos!";
                    }
                    else
                    {
                
#if UNITY_ANDROID
                        float vol = 100.0f * sound.GetSystemVolume();
#else
                float vol = 100.0f;
#endif
                
                        if (vol < 90)
                        {
                            textWarning.text = "Sube al máximo tu volúmen";
                        }
                        else
                        {
                            textWarning.text = "";
                        }
                    }
                }

                break;

        }
    }
    
    private void ShowDelayValue(string delay)
    {
        if(_toggleMsg)
            textVol.text = "> " + delay + " <";
        else
        {
            textVol.text = "< " + delay + " >";
        }
        _toggleMsg = !_toggleMsg;
    }
    
    // activate or deactivate buttons
    private void ActivatedButtons(bool active)
    {
        btnNext.interactable = active;
        btnAutoCalibration.interactable = active ;
    }
    
    private void AutoCalibration()
    {
#if UNITY_ANDROID
        // Plugin for headset detection was downloaded from: https://github.com/DaVikingCode/UnityDetectHeadset
        bool isHeadset = DetectHeadset.Detect();
#else
        bool isHeadset = false;
#endif
        
        if (!isHeadset) // at this point we need loudspeakers
        {
            
#if UNITY_ANDROID
            float vol = 100.0f * sound.GetSystemVolume();
#else
            float vol = 100.0f;
#endif
            
            if (vol > 90)
            {
                // do autocalibration just if loudspeakers volume is higher than 90%
                // block buttons
                ActivatedButtons(false);
        
                // update txt msg
                _hapticDelayTemp = 0;
                _kRecord = 0;
                _ready2Record = true;
                _calibratingStep = 1;
            }
        }
    }
    
    // delays means how much time the second array has to be shifted. However, due to our game logic, we need to 
    // shift the first array to the second one. Thus, Use the negative of the current output.
    private int ComputeAudioDelay(float[] AudioRef, float[] AudioShifted)
    {
        float[] crossCorr = MyCrossCorr( AudioRef , AudioShifted );
        float maxVal = crossCorr.Max();
        int maxValIndex = crossCorr.ToList().IndexOf(maxVal)+1;
        int delays_ms = 1000 * (maxValIndex - (AudioShifted.Length)) * 1 / _sampleRate ;
        return delays_ms;
    }
    
    // MyCrossCorr was taken from: https://stackoverflow.com/q/47181487/3430103 apr 12 - 2021
    // first array will be the referenced. The result will mean the time the second array has to be shifted to equal time of the first
    private float[] MyCrossCorr(float[] arr1, float[] arr2)
    {
        int lx = arr1.Length;
        int ly = arr2.Length;
        int jmin, jmax, index;
        index = 0;
        int lconv = lx + ly - 1;
        float[] z = new float[lconv];
        for (int i = 0; i < lconv; i++)
        {
            if (i >= ly)
            {
                jmin = i - ly + 1;
            }
            else
            {
                jmin = 0;
            }

            if (i < lx)
            {
                jmax = i;
            }
            else
            {
                jmax = lx -1;
            }

            for (int j = jmin; j <= jmax; j++)
            {
                index = ly - i + j - 1;
                z[i] = z[i] + (arr1[j] * arr2[index]);

            }
        }
        //Debug.Log(z);
        return z;
    }

    private void ClickOnNext()
    {
        bool instructionsDone = toggleOK.isOn;
        
        switch (_stage)
        {
            case 0:
                if (_microphPermission && instructionsDone)
                {
                    textInformation.text = "3- SIN AUDÍFONOS sube el volumen al máximo posible de tu celular." +
                                           "\n\n4- Oprime Autocalibración, y en silencio espera 30 segundos." +
                                           "\n\n5- Ve a la siguiente sección";
                    textVol.gameObject.SetActive(true);
                    textWarning.gameObject.SetActive(true);
                    btnAutoCalibration.gameObject.SetActive(true);
                    btnNext.interactable = false;
                    toggleOK.GetComponent<Toggle>().isOn = false;
                    _stage = 1;
                }
                else
                {
#if UNITY_ANDROID 
                    // ask for permission to microphones access
                    Permission.RequestUserPermission(Permission.Microphone);
                    dialog = new GameObject();
#endif
                }
                break;
            
            case 1:
                if (instructionsDone)
                {
                    //write delay time into file
                    WriteFile(_pathSettingsHapticDelayFile, ""+_hapticDelay,"r");
                    GoToNextScene();
                }
                break;
        }
    }
    
    private void GoToNextScene()
    {
        Loader.Load(Loader.Scene.CalibrationScene2);
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

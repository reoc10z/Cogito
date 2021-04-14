using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class CalibrationController : MonoBehaviour
{
    public Button BtnNext;
    public AudioSource BackgroundSound;
    public AudioSource BeepSound;
    public Text textVol;
    public Text TextInformation;
    public Button BtnPlay;
    public Button BtnDelayMore;
    public Button BtnDelayBitMore;
    public Button BtnDelayLess;
    public Button BtnDelayBitLess;
    public Button BtnAutoCalibration;

    private int _hapticDelay = 0;
    private int _hapticDelayTemp = 0;
    private int stage = 0;
    private Stopwatch _zeit = new Stopwatch();
    private long _deltaFramesTime = 0;
    private bool _playingStimuli = false;
    private string _pathSettingsHapticDelayFile;
    private bool _toggleMsg = false;
    private AudioSource _audioMicrophone;
    private readonly int _nCalibrationRecs = 3;
    private int _kRecord = 0;
    private bool _ready2record = false;
    private int _calibrating_step = 0;
    private readonly int _sampleRate = 44100;
    private float[] _dataAudioOriginal;

    private const int Vd = 0;  // vibrationDelay (ms): delay trying to synchronize audio and vibration pattern
    private const int Vt = 50; // vibrationTime
    private readonly List<long[]> _vibrationPatterns = new List<long[]>()
    {
        new long[] {Vd, Vt, Vt, Vt, Vt, Vt}, // 3 pulses
    };
    
#if UNITY_ANDROID
    private AndroidNativeVolumeService sound = new AndroidNativeVolumeService();
#endif


    void Awake()
    {
        BtnPlay.gameObject.SetActive(false);
        BtnDelayMore.gameObject.SetActive(false);
        BtnDelayBitMore.gameObject.SetActive(false);
        BtnDelayLess.gameObject.SetActive(false);
        BtnDelayBitLess.gameObject.SetActive(false);
        
        BtnAutoCalibration.gameObject.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        _pathSettingsHapticDelayFile = GetPathFile("SettingsHapticDelay.txt");
        BtnNext.onClick.AddListener(ClickOnNext);
        BackgroundSound.Play();
        BtnPlay.onClick.AddListener(ApplyStimuli);
        BtnDelayMore.onClick.AddListener(IncreaseHapticDelay);
        BtnDelayBitMore.onClick.AddListener(IncreaseBitHapticDelay);
        BtnDelayLess.onClick.AddListener(DecreaseHapticDelay);
        BtnDelayBitLess.onClick.AddListener(DecreaseBitHapticDelay);
        
        BtnAutoCalibration.onClick.AddListener(AutoCalibration);
        
        _dataAudioOriginal = new float[BeepSound.clip.samples];
        BeepSound.clip.GetData(_dataAudioOriginal, 0);
        
        _audioMicrophone = GetComponent<AudioSource>();
        _audioMicrophone.loop = false;
        
        _zeit.Start();
    }

    void Update()
    {
#if UNITY_ANDROID
        if (stage == 0)
        {
            float vol = 100.0f * sound.GetSystemVolume();
            textVol.text = "Volúmen: " + vol.ToString("0.00") + " %";
            _deltaFramesTime = _zeit.ElapsedMilliseconds;
        }
        
        else if (stage==1)
        {
            if (_playingStimuli)
            {               
                if (_zeit.ElapsedMilliseconds - _deltaFramesTime >= _hapticDelay)
                {
                    Vibration.Vibrate(_vibrationPatterns[0], -1);
                    _deltaFramesTime = _zeit.ElapsedMilliseconds;
                    _playingStimuli = false;
                }
            }
            else if (_calibrating_step == 1)
            {
                if (_kRecord < _nCalibrationRecs)
                {
                    if (_ready2record)
                    {
                        _audioMicrophone.clip = Microphone.Start("", false, 1, _sampleRate);
                        
                        // play sound
                        BeepSound.Play();
                        _ready2record = false;
                        _deltaFramesTime = _zeit.ElapsedMilliseconds; //timestamp
                        ShowDelayValue(""+ (_nCalibrationRecs-_kRecord));
                        
                        print(""+ _deltaFramesTime + " ms : recording- " + _kRecord); // todo: remove line
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
                            _ready2record = true;
                            _kRecord++;
                        }
                    }
                }
                else
                {
                    ShowDelayValue("...espera 15 segs...");
                    _calibrating_step = 2;
                }
            } else if (_calibrating_step == 2)
            {
                _hapticDelay = -1*_hapticDelayTemp / _nCalibrationRecs;
                // showing computed delay
                ShowDelayValue( _hapticDelay.ToString() );
                _calibrating_step = 0;
            }
        }
#endif
    }

    private void IncreaseHapticDelay()
    {
        _hapticDelay += 50;
        ApplyStimuli();
    }
    private void IncreaseBitHapticDelay()
    {
        _hapticDelay += 5;
        ApplyStimuli();
    }

    private void DecreaseHapticDelay()
    {
        _hapticDelay -= 50;
        if (_hapticDelay < 0)
            _hapticDelay = 0;
        ApplyStimuli();
    }
    private void DecreaseBitHapticDelay()
    {
        _hapticDelay -= 5;
        if (_hapticDelay < 0)
            _hapticDelay = 0;
        ApplyStimuli();
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
    
    
    private void AutoCalibration()
    {
        // update txt msg
        _hapticDelayTemp = 0;
        _kRecord = 0;
        _ready2record = true;
        _calibrating_step = 1;
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
    
    private void ApplyStimuli()
    {
        // update msg
        ShowDelayValue( _hapticDelay.ToString() );
        
        // play sound
        _deltaFramesTime = _zeit.ElapsedMilliseconds;
        BeepSound.Play();
        _playingStimuli = true;
        
        print(_deltaFramesTime + " : sound"); // todo: remove line
    }
    
    private void ClickOnNext()
    {
        if (stage == 0)
        {
            BackgroundSound.Stop();
            textVol.text = "> 0 <";
            TextInformation.text = "4- Pon mucha atención:" +
                                   "\nCon + y - haz que el sonido y la vibración se sincronicen." +
                                   "\nPista: lo más probable es que, inicialmente, el sonido suene después que la vibración";
            
            BtnPlay.gameObject.SetActive(true);
            BtnDelayMore.gameObject.SetActive(true);
            BtnDelayBitMore.gameObject.SetActive(true);
            BtnDelayLess.gameObject.SetActive(true);
            BtnDelayBitLess.gameObject.SetActive(true);
            
            BtnAutoCalibration.gameObject.SetActive(true);
            
            // Initialize the plugin for vibrations
            Vibration.Init();
            
            stage = 1;
        }
        else if (stage == 1)
        {
            GoToNextScene();
        }
    }
    private void GoToNextScene()
    {
        //write delay time into file
        WriteFile(_pathSettingsHapticDelayFile, ""+_hapticDelay,"r");
        Loader.Load(Loader.Scene.GameScene);
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
//using UnityEngine.UIElements;
using UnityEngine.UI;
using System.Diagnostics;
using System.IO;

public class CogitoController : MonoBehaviour
{

    // general settings
    public Button BtnMenu; // TODO: remove code line
    public Text TxtInstructions;
    private string[] _instructions = new string[]
    {
        "",
        "1- Después de que la app mueva la pelota, llévala al centro de la regla lo más rápido posible",
        "1- Después de que la app mueva la pelota, llévala al centro de la regla lo más rápido posible,\n2- Memoriza el patrón", 
        "1- Después de que la app mueva la pelota, llévala al centro de la regla lo más rápido posible",
        "1- Marca el patrón que memorizaste y pulsa OK"
    };
    private float _deltaMovement;
    private float _widthScreen;
    private short _level; // 0-easy, 1-medium, 2-hard
    private bool _playing;
    private bool _toVibrate;
    private bool _toPlaySound;
    private int _nStage = 0;
    private string _pathLogFile;
    private string _pathSettingsFile;
    private string _pathTestVersionFile;
    private string _testVersion;
    private string _logs = "";
    private int _cyclesByLevel;

    //Timers
    public float BallTimeCycle = 3.0f;
    private float _ballTime;
    private float _matrixTime;
    private readonly float _delayStimuliMs = 100; // 100 ms
    private long _timeSinceEndStimulus;
    private float _timeSinceAudioPlay;
    private float _timeSinceVibrationStarts;
    public int[] _timesFrame = new int[] {2000, 10000, 10000, 4000, 10000}; // in ms: stage0, stage1, stage2, ...
    private float _timerFrame;
    private float _timerBall;
    private Stopwatch _zeit = new Stopwatch();
    private long _deltaFramesTime;

    // ball
    public GameObject Ball;
    public GameObject Ruler;
    private float _center_intialX;
    private float _center_intialY;
    private float _xCurrent;

    private short[] _listBallPosition_level0 = new short[]
        {1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5}; //from -6 to 6

    private short[] _listBallPosition_level1 = new short[]
        {-1, -2, -3, -4, -5, -1, -2, -3, -4, -5, -1, -2, -3, -4, -5, -1, -2, -3, -4, -5}; //from -6 to 6

    private short[] _listBallPosition_level2 = new short[]
        {1, 2, 3, 4, 5, 6, -1, -2, -3, -4, -5, -6, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3}; //from -6 to 6
        //{1, -2, 3, -4, 5, 1, -2, 3, -4, 5, 1, -2, 3, -4, 5, 1, -2, 3, -4, 5, 1, -2, 3, -4, 5}; //from -6 to 6

    private short[] _listBallPosition;
    private short _ballPosition = 0;
    private short _nextBallPosition = 0;
    private short _idxBallPosition = 0;
    private int _idxStimuli = 0;
    private int _ballDirection;
    private Vector3 _nextPosition;
    private bool _newPosition;
    public GameObject ArrowsPanel;
    private bool _allowBallMovement;

    // question pattern: shown pattern to be learnt 
    public GameObject MatrixQuestion;

    private Image[] _listQuestionCells;

    //private static bool[] _pattern_0 = new bool[16] {true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false};
    //private static bool[] _pattern_1 = new bool[16] {false, true, false, true, false, true, false, true, false, true, false, true, false, true, false, true};
    // TODO: remove below code lines after level tests
    private static bool[] _pattern_0 = new bool[16]
    {
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false
    };

    private static bool[] _pattern_1 = new bool[16]
    {
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false
    };

    private static bool[] _pattern_2 = new bool[16]
    {
        false,
        false,
        false,
        false,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false
    };
        

    private static bool[] _pattern_3 = new bool[16]{
        false,
        false,
        false,
        false,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false
    };

private static bool[] _pattern_4 = new bool[16]
    {
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
    };

private static bool[] _pattern_5 = new bool[16] {false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, true};
    private List<bool[]> _listQuestionPatterns_level0 = new List<bool[]>(){
        _pattern_0, _pattern_1
    };

    private List<bool[]> _listQuestionPatterns_level1 = new List<bool[]>()
    {
        _pattern_2, _pattern_3
    };

    private List<bool[]> _listQuestionPatterns_level2 = new List<bool[]>()
    {
        _pattern_4, _pattern_5,
    };
    private List<bool[]> _listQuestionPatterns = new List<bool[]>();
    private short _kPattern = 0;
    
    // answer pattern: pattern to mark answers
    public GameObject MatrixAnswer;
    private Toggle[] _listAnswerCells;
    private bool[] _answerPattern = new bool[16] {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true};
    public Button Btn_OK;
    private bool _registerAnswer = false;
    
    //auditory
    public AudioSource[] AudioPulses = new AudioSource[6]; // up to 6 audio files for up to 6 pulses
    private readonly float _intrinsic_audioDelay = 50f; // ms
    private bool _isAudio;
    
    // haptic
    //vibration. Pattern has to be: off, on, off, on time
    private const int Vd = 0;  // vibrationDelay (ms): delay trying to synchronize audio and vibration pattern
    private const int Vt = 50; // vibrationTime
    private readonly List<long[]> _vibrationPatterns = new List<long[]>()
    {
        new long[] {Vd, Vt}, // 1 pulse
        new long[] {Vd, Vt, Vt, Vt}, // 2 pulses
        new long[] {Vd, Vt, Vt, Vt, Vt, Vt}, // 3 pulses
        new long[] {Vd, Vt, Vt, Vt, Vt, Vt, Vt, Vt}, // 4 pulses
        new long[] {Vd, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt}, // 5 pulses
        new long[] {Vd, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt} // 6 pulses
    };
    private long[] _vibrationTimePatterns; // array that includes the time length for each item in _vibratioPatterns
    private bool _isVibration;

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
    
    void Awake()
    {
        _widthScreen = Screen.width;
        // define path to setting file for level
        _pathSettingsFile = GetPathFile("SettingsLevel.txt");
        _pathTestVersionFile = GetPathFile("SettingsTestVersion.txt");
    }

    // Start is called before the first frame update
    void Start()
    {
        // keep screen always active
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        // log file
        _pathLogFile = GetPathFile("Log.txt");
        print(_pathLogFile);
        
        // test version: base, H, A, HA 
        _testVersion = ReadFile(_pathTestVersionFile);
        // If haptics:
        if (_testVersion == "H" || _testVersion == "HA")
        {
            // Initialize the plugin for vibrations
            Vibration.Init();
            _toVibrate = true;
        }
        else
            _toVibrate = false;
        // If auditory:
        if (_testVersion == "A" || _testVersion == "HA")
            _toPlaySound = true;
        else
            _toPlaySound = false;

        // initiate variables for the next level
        NextLevel();
        
        // general settingss
        BtnMenu.onClick.AddListener(GoToMenu); // TODO: remove code line 
        // when start method, the game has not started
        _playing = false;
        _ballDirection = 0;

        //timers
        _timeSinceEndStimulus = 0;
                
        // question pattern
        _listQuestionCells = MatrixQuestion.GetComponentsInChildren<Image>().ToArray(); // list cells in pattern // MatrixQuestion.GetComponentsInChildren<Image>().Skip(1).ToArray(); // first element is the pattern (thus, skip!)
        MatrixQuestion.SetActive(false); //turn off pattern
        
        // answer pattern
        _listAnswerCells = MatrixAnswer.GetComponentsInChildren<Toggle>().ToArray();
        Btn_OK.onClick.AddListener(BtnOKanswer);
        MatrixAnswer.SetActive(false);
        Btn_OK.gameObject.SetActive(false);

        // enlist next pattern
        NextPattern(_listQuestionCells, _listQuestionPatterns[_kPattern]);
        _kPattern += 1;
        
        // ball
        // initial ball position
        _newPosition = false;
        _allowBallMovement = false;
        _center_intialX = Ball.transform.position.x;
        _center_intialY = Ball.transform.position.y;
        _deltaMovement = _widthScreen / 15;
        Ruler.SetActive(false);
        Ball.SetActive(false);
        ArrowsPanel.SetActive(false);
        
        // audio
        AudioPulses = GetComponents<AudioSource>();
        // this variable will contain the duration of each vibration pattern in ms. It includes the delay value used to synchronise audio and haptics stimuli
        // it will be used to wait for the whole stimulus to later move the ball
        _vibrationTimePatterns = new long[_vibrationPatterns.Count];
        int k = 0;
        foreach (long[] vP in _vibrationPatterns)
        {
            _vibrationTimePatterns[k] = vP.Sum();
            k += 1;
        }
        _isAudio = false;
        
        // haptic
        _isVibration = false;
        
        // global timer 
        _zeit.Start();
        
        //_currentTime = zeit.ElapsedMilliseconds;
        //toLog(_currentTime + ": app starts");
    }
    
    void FixedUpdate()
    {
        // write log messages to log file
        WriteLog();
    }

   // Update is called once per frame
    void Update()
    {
        if (!_playing)
        {
            StartGame();
        }
        else
        {
            long deltaTime = _zeit.ElapsedMilliseconds - _deltaFramesTime;
            _deltaFramesTime += deltaTime;

            SelectStage(deltaTime);
            
            if ( CheckIfStimuliEnded() )
            {
                //After primmed stimuli end, we have to wait for 100 ms to change ball to a new position 
                if (_newPosition && _allowBallMovement) 
                {
                    if (_zeit.ElapsedMilliseconds - _timeSinceEndStimulus > _delayStimuliMs - 1) // _delayStimuli is 100ms
                    {
                        // Move ball to next predefined position by the game
                        Ball.transform.position = _nextPosition;
                        _ballPosition = _nextBallPosition;
                        ToLog("-10-system ball position: " + _ballPosition);
                        _newPosition = false;
                    }
                }
            }

            // get current ball position: it could be changed by the user with the button arrows
            _xCurrent = Ball.transform.position.x;
        
            // limit ball movement to screen size
            if (_allowBallMovement && _xCurrent - _center_intialX > -(_widthScreen/2 - 2*_deltaMovement) && _ballDirection<0)
            {
                // move left if user pressed button
                MoveBall(_xCurrent, -1);
            } else if (_allowBallMovement && _xCurrent - _center_intialX < (_widthScreen/2 - 2*_deltaMovement) && _ballDirection>0)
            {
                // move right if user pressed button
                MoveBall(_xCurrent, +1);
            }

            _ballDirection = 0;
        }
    }
    
    
    private short GetLevel()
    {
        // read settings file
        return short.Parse( ReadFile(_pathSettingsFile) );
    }
    
    private void NextLevel()
    {
        _level = GetLevel();
        if (_level > 2)
        {
            _level = 0;
        }
        
        if (_level == 0)
        {
            // ball positions
            _listBallPosition = _listBallPosition_level0;
            // patterns
            _listQuestionPatterns = _listQuestionPatterns_level0;
        } else if (_level == 1)
        {
            // ball positions
            _listBallPosition = _listBallPosition_level1;
            // patterns
            _listQuestionPatterns = _listQuestionPatterns_level1;
        } else if (_level == 2)
        {
            // ball positions
            _listBallPosition = _listBallPosition_level2;
            // patterns
            _listQuestionPatterns = _listQuestionPatterns_level2;
        }

        _idxBallPosition = 0;
        
        _cyclesByLevel = _listQuestionPatterns.Count;
    }

    private void SelectStage(long deltaTime)
    {
        if (_nStage == 0) 
        {
            // after click Start, game will wait for 2 seconds to start
            if (_timerFrame > 0)
            {
                _timerFrame -= deltaTime;
            }
            else
            {
                // go to next stage
                _nStage = 1;
                _timerFrame = _timesFrame[_nStage];
                _timerBall = BallTimeCycle; // 3 seconds
                TxtInstructions.text = _instructions[_nStage];
                BallController(true);
            }
        } else if (_nStage == 1) 
        {
            // stage 1 shows only the ball
            // ball
            if (_timerBall > 0)
            {
                _timerBall -= deltaTime;
            }
            else
            {
                // move ball
                BallController(true);
                // reset ball timer
                _timerBall = BallTimeCycle; // 3.0f
            }
                
            // next-frame timer
            if (_timerFrame > 0)
            {
                _timerFrame -= deltaTime;
            }
            else
            {
                // go to next stage
                _nStage = 2;
                _timerFrame = _timesFrame[_nStage];
                TxtInstructions.text = _instructions[_nStage];
                MatrixQuestionController(true);
            }
                
        } else if (_nStage == 2) 
        {
            // stage 2 shows the ball and the pattern to be recalled
            // ball
            if (_timerBall > 0)
            {
                _timerBall -= deltaTime;
            }
            else
            {
                // move ball
                BallController(true);
                // reset ball timer
                _timerBall = BallTimeCycle; // 3.0f
            }

            // next-frame timer
            if (_timerFrame > 0)
            {
                _timerFrame -= deltaTime;
            }
            else
            {
                // go to next stage
                _nStage = 3;
                _timerFrame = _timesFrame[_nStage];
                TxtInstructions.text = _instructions[_nStage];
                MatrixQuestionController(false);
            }
                
        } else if (_nStage == 3) 
        {
            // stage 2 shows only the ball
            // ball
            if (_timerBall > 0)
            {
                _timerBall -= deltaTime;
            }
            else
            {
                // move ball
                BallController(true);
                // reset ball timer
                _timerBall = BallTimeCycle; // 3.0f
            }
                
            // next-frame timer
            if (_timerFrame > 0)
            {
                _timerFrame -= deltaTime;
            }
            else
            {
                // go to next stage
                _nStage = 4;
                _timerFrame = _timesFrame[_nStage];
                TxtInstructions.text = _instructions[_nStage];
                BallController(false);
                MatrixAnswerController(true);
            } 
        } else if (_nStage == 4)
        {
            // stage 2 shows a matrix where the user has to recall the pattern
            // next-frame timer
            if (_timerFrame > 0)
            {
                _timerFrame -= deltaTime;
            }
            else 
            {
                // go to next stage
                MatrixAnswerController(false); 
                Ruler.SetActive(true);
                Ball.SetActive(true);
                ArrowsPanel.SetActive(true);
                _nStage = 0;
                _timerFrame = _timesFrame[_nStage];
                TxtInstructions.text = _instructions[_nStage];
                _cyclesByLevel -= 1;
                if (_cyclesByLevel == 0)
                {
                    GoToNextScene();
                }
            }
        } 
    }

    // CheckIfStimuliEnded: it checks if primmed stimuli have ended and returns a boolean
    private bool CheckIfStimuliEnded()
    {
        if (_isAudio)
        {
            if (_zeit.ElapsedMilliseconds - _timeSinceAudioPlay > AudioPulses[_idxStimuli].clip.length)
            {
                if (!AudioPulses[_idxStimuli].isPlaying)
                {
                    // when audio-play ends
                    _timeSinceEndStimulus = _zeit.ElapsedMilliseconds;
                    ToLog("-13- sound ends: " + (_idxStimuli + 1) );
                    _isAudio = false;
                    // vibration is false, just in case audio and vibrations are executed in parallel
                    _isVibration = false; // after testing, vibration always ends before the auditory stimulus
                }
            }
        }
        else
        {
            if (_isVibration)
            {
                if (_zeit.ElapsedMilliseconds - _timeSinceVibrationStarts > _vibrationTimePatterns[_idxStimuli])
                {
                    // when vibration ends
                    _timeSinceEndStimulus = _zeit.ElapsedMilliseconds;
                    ToLog("-15- vibration ends: " + (_idxStimuli + 1) );
                    _isVibration = false;
                    _isAudio = false; // because if.
                }
            }
        }
        return !(_isAudio | _isVibration);
    }
    
    // append new message to the general log message 
    private void ToLog(string msg)
    {
        msg = _zeit.ElapsedMilliseconds +" " + msg + "\n";
        _logs += msg;
        print(msg);
    }
    
    private string ReadFile(string filePath)
    {
        // read filepath and return all text as one string
        return System.IO.File.ReadAllText(filePath);
    }

    private void WriteFile(string filePath, string msg)
    {
        File.AppendAllText(filePath, msg);
    }
    
    // writeLog assumes log file was already created under the path in _pathLogFile
    private void WriteLog()
    {
        WriteFile(_pathLogFile, _logs);
        _logs = "";
    }
    private void BallController(bool on)
    {
        if (on)
        {
            Ball.SetActive(true);
            Ruler.SetActive(true);
            ArrowsPanel.SetActive(true);
            NextBallPosition();
            _allowBallMovement = true;
        }
        else
        {
            _allowBallMovement = false;
            Ball.transform.position = new Vector3(_center_intialX, _center_intialY, 0);
            _ballPosition = 0;
            ToLog("-10- system ball position: " + _ballPosition);
            Ball.SetActive(false);
            Ruler.SetActive(false);
            ArrowsPanel.SetActive(false);
        }
    }

    private void Vibrate(long[] vibrationPattern)
    {
        _timeSinceVibrationStarts = _zeit.ElapsedMilliseconds;
        ToLog("-14- vibration starts: " + (_idxStimuli + 1));
        Vibration.Vibrate(vibrationPattern, -1);
        _isVibration = true;
    }

    private void PlaySound(AudioSource _audio)
    {
        _timeSinceAudioPlay = _zeit.ElapsedMilliseconds;
        ToLog("-12- sound starts: "+ (_idxStimuli + 1) );
        _audio.Play(0);
        _isAudio = true;
    }
    
    private void NextBallPosition()
    {
        _nextBallPosition = _listBallPosition[_idxBallPosition];
        _idxStimuli = Math.Abs(_nextBallPosition)-1;
        //stimuli
        if (_level == 2)
        {
            if (_nextBallPosition != 0)
            {
                if (_toPlaySound)
                {
                    //auditory stimulus
                    PlaySound(AudioPulses[_idxStimuli]);
                }

                if (_toVibrate)
                {
                    // haptic stimulus
                    Vibrate(_vibrationPatterns[_idxStimuli]);
                }
                // delay of 100 ms after primmed stimuli is applied in Update()
            }
        }
        
        // we enlist the new ball position but we have to wait stimuli time + 100ms to move the ball 
        _nextPosition = new Vector3(_center_intialX + _nextBallPosition* _deltaMovement, _center_intialY, 0);
        _newPosition = true;
        
        // point to next position
        _idxBallPosition += 1;
    }

    // typeMovement: +1(right) ; -1(left)
    private void MoveBall(float xCurrent, short typeMovement)
    {
        if (typeMovement > 0)
        {
            // right
            Ball.transform.position = new Vector3(xCurrent+_deltaMovement, _center_intialY, 0);
            _ballPosition += 1;
            ToLog("-11- user ball position: "+_ballPosition);
        } else if (typeMovement < 0)
        {
            // left
            Ball.transform.position = new Vector3(xCurrent-_deltaMovement, _center_intialY, 0);
            _ballPosition -= 1; 
            ToLog("-11- user ball position: "+_ballPosition);
        }
    }

    private void MatrixQuestionController(bool on)
    {
        if (on)
        {
            // show pattern
            MatrixQuestion.SetActive(true);
            ToLog("-20- system pattern: " + _kPattern + " : " + 
                  String.Join(",", new List<bool>(_listQuestionPatterns[_kPattern]).ConvertAll(i => i.ToString()).ToArray())
                  );
        }
        else
        {
            // hide pattern
            MatrixQuestion.SetActive(false);
            // enlist next pattern
            NextPattern(_listQuestionCells, _listQuestionPatterns[_kPattern]);
            _kPattern += 1;
        }
    }

    private void MatrixAnswerController(bool on)
    {
        if (on)
        {
            _registerAnswer = true;
            MatrixAnswer.SetActive(true);
            Btn_OK.gameObject.SetActive(true);
            Btn_OK.interactable = true;
            ToLog("-21- answer pattern starts");
        }
        else
        {
            // _registerAnswer is used to control when the log message of the answer-matrix buttons appears. The below foreach calls the internally the function BtnOKanswer() 
            _registerAnswer = false;
            ToLog("-24- answer pattern ends by system: " + (_kPattern-1) + " : " + 
                  String.Join(",", new List<bool>(_answerPattern).ConvertAll(i => i.ToString()).ToArray())
            );
            MatrixAnswer.SetActive(false);
            Btn_OK.gameObject.SetActive(false);
            // reset cells to true (white). when changing the value of each cell, the attached-to-button function BtnOKanswer() modifies the array _answerPattern
            foreach (Toggle cell in _listAnswerCells)
            {
                cell.isOn = true;
            }
        }
        
    }
    
    // NextPattern enlists the next pattern by changing cell colors 
    private void NextPattern(Image[] listCells, bool[] pattern)
    {
        int k = 0;
        foreach (Image cell in listCells)
        {
            // paint white cells in 1 and paint black cells in 0
            cell.color =  (pattern[k] == true) ?  new Color(1,1,1) : cell.color = new Color(0,0,0);
            k += 1;
        }
    }
    
    public void BtnRight()
    {
        _ballDirection = 1;
    } 
    
    public void BtnLeft()
    { 
        _ballDirection = -1;
    }

    private void StartGame()
    {
        _nStage = 0;
        _timerFrame = _timesFrame[_nStage];
        _playing = !_playing;
        Ruler.SetActive(true);
        Ball.SetActive(true);
        ArrowsPanel.SetActive(true);
        _deltaFramesTime = _zeit.ElapsedMilliseconds;
        ToLog("-1- level starts: " + _level);
    }

    private void BtnOKanswer()
    {
        ToLog("-23- answer pattern ends by user ok-button");
        Btn_OK.interactable = false;
        MatrixAnswer.SetActive(false);
    }

    public void BtnAnswerPattern(int id)
    {
        _answerPattern[id] = !_answerPattern[id];
        if (_registerAnswer)
        {
            ToLog("-22- user pressed cell: " + id + " : " + _answerPattern[id] );
        }
    }

    private void GoToNextScene()
    {
        ToLog("-2- level ends: " + _level);
        WriteLog();
        Loader.Load(Loader.Scene.QuestionnaireScene);
    }

    
    // TODO: below code can be removed
    public void CheckVibrate()
    {
        _toVibrate = !_toVibrate;
    }

    public void CheckPlaySound()
    {
        _toPlaySound = !_toPlaySound;
    }
    
    private void GoToMenu()
    {
        Loader.Load(Loader.Scene.MenuScene);
    }
}

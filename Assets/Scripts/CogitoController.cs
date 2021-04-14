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
    public Text TxtInstructions_Ball;
    public Text TxtInstructions_Pattern;
    public GameObject ScreenMsg;
    public GameObject[] HighlightsInstructions = new GameObject[2];

    private float _deltaMovement;
    private float _widthScreen;
    private short _level; // (-1)test, 0-easy, 1-medium, 2-hard
    private bool _playing;
    private bool _toVibrate;
    private bool _toPlaySound;
    private int _nStage = 0;
    private string _pathLogFile;
    private string _pathSettingsFile;
    private string _pathSettingsHapticDelayFile;
    private string _pathTestVersionFile;
    private string _testVersion;
    private string _logs = "";
    private int _cyclesByLevel;

    //Timers
    public float BallTimeCycle = 3000.0f;
    private float _ballTime;
    private float _matrixTime;
    private readonly float _delayStimuliMs = 100; // 100 ms
    private long _timeSinceEndStimulus;
    private float _timeSinceAudioPlay;
    private float _timeSinceVibrationStarts;
    public int[] _timesFrame = new int[] {1000, 10000, 10000, 4000, 10000}; // in ms: stage0, stage1, stage2, ...
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

    // ball postiion values in range [-6,6] excluding 0
    private short[] _listBallPosition_levelTraining =
        {3, -3, 2, 3, -4, 1, -1, 2, -3, 4, 5, -6, 2, 3, -4, 1, -1, 2, -3, 4}; 

    private short[] _listBallPosition_level0 = new short[]
    {
        -6, -4, -2, -4, -2, -3,  1, -5, -1,  4, 2, -5, -4, -5,  4, -6, -4,
         5,  4, -4,  3, -1,  6,  1, -3, -2,  1, 4, -4,  1, -2, -1,  3,  5,
         3,  2, -3, -2,  2, -4, -5,  5, -1, -2, 2, -5,  3,  1,  2, -6, -1,
         5, -5,  5, -5,  3, -4, -1,  3, -6
    };

    private short[] _listBallPosition_level1 = new short[]
    {
        -5,  1, -6,  6,  3, -5, -1, -3, -4, -1, -2, -5, -4, -2,  6, -5, -2,
        3,  1, -2, -5, -2,  6,  5,  2, -1,  3,  2, -3,  5, -4, -5,  1, -4,
        2, -1,  2, -6, -5,  2,  4,  3, -1,  5, -2, -1,  2,  6, -2,  2, -2,
        -3,  6, -2,  4,  2, -2,  5,  2, -3, -6, -3,  2,  6,  4, -1,  6,  5,
        4, -3, -5, -2, -1, -5, -4,  1, -2, -6,  4, -1, -6, -3, -4,  3, -4,
        6,  3,  6,  5, -3
    };

    private short[] _listBallPosition_level2 = new short[]
    {
        2,  1, -2, -1, -5,  1, -4,  3, -3, -6,  6,  1, -1, -6, -3,  3, -3,
        -5, -3,  1, -3,  4,  5,  2, -1, -3, -4,  4,  3, -5, -6,  2, -2,  2,
        -4, -3, -1, -3,  4,  5, -3, -2,  3,  4, -1,  2,  5,  3, -2, -3,  5,
        -2, -3, -6, -5,  2,  3, -1,  3, -6,  3, -1,  5,  3, -5, -2, -5,  1,
        5,  4,  3, -4,  5, -1,  4, -6, -3, -5, -3,  4,  6,  2,  3, -3,  6,
        -4,  5, -5, -1,  2,  3,  1, -6,  6,  4, -3, -2, -5,  5, -1,
    };

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
    
    public GameObject MatrixQ_level0; 
    public GameObject MatrixQ_level1; 
    public GameObject MatrixQ_level2; 
    private GameObject _matrixQuestion;
    private Image[] _listQuestionCells;
    private List<bool[]> _listQuestionPatterns = new List<bool[]>();
    private short _kPattern = 0;

    // patterns by level
    
    // pattern: training level
    private readonly List<bool[]> _listQuestionPatterns_levelTraining = new List<bool[]>()
    {
        new bool[16] {false,false, true, true, false, false, true, true, true, true, true, true, true, true, true, true},
        new bool[16] {false, false, true, true, true, false, false, true, true, true, false, false, true, true, true, true },
        new bool[16], // pattern_end: it is set to false. This pattern HAS TO be used at the end of all pattern lists of each level
    };

    // pattern: level 0 or easy: 6 patterns (4x4 with 3 black cells)
    private List<bool[]> _listQuestionPatterns_level0 = new List<bool[]>()
    {
        new bool[16] { true,  true,  true,  false,  true,  true,  true,  true,  false,  true,  false,  true,  true,  true,  true,  true, },
        new bool[16] { true,  false,  false,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  false, },
        new bool[16] { true,  false,  true,  true,  true,  false,  false,  true,  true,  true,  true,  true,  true,  true,  true,  true, },
        new bool[16] { false,  true,  true,  true,  true,  true,  true,  true,  true,  false,  true,  true,  true,  true,  false,  true, },
        new bool[16] { true,  true,  true,  true,  true,  true,  false,  true,  true,  true,  true,  false,  true,  true,  false,  true, },
        new bool[16] { true,  true,  true,  true,  true,  false,  true,  true,  true,  true,  true,  true,  true,  false,  false,  true, },
        new bool[16], // pattern_end: it is set to false. This pattern HAS TO be used at the end of all pattern lists of each level
    };

    // pattern: level 1 or middle: 6 patterns (5x5 with 6 black cells)
    private List<bool[]> _listQuestionPatterns_level1 = new List<bool[]>()
    {
        new bool[25] { true,  true,  false,  true,  true,  true,  true,  true,  true,  false,  true,  false,  true,  true,  true,  true,  true,  true,  true,  true,  true,  false,  false,  true,  false, },
        new bool[25] { true,  true,  true,  false,  true,  true,  false,  true,  true,  false,  false,  true,  true,  true,  true,  true,  true,  false,  true,  true,  true,  false,  true,  true,  true, },
        new bool[25] { false,  true,  false,  true,  true,  true,  true,  true,  false,  false,  true,  true,  true,  true,  true,  true,  true,  true,  true,  false,  true,  false,  true,  true,  true, },
        new bool[25] { true,  true,  true,  true,  false,  true,  true,  true,  true,  false,  false,  true,  true,  false,  true,  true,  false,  true,  true,  true,  true,  false,  true,  true,  true, },
        new bool[25] { true,  true,  true,  false,  true,  true,  false,  false,  true,  true,  true,  true,  true,  false,  false,  true,  true,  true,  true,  true,  true,  true,  true,  false,  true, },
        new bool[25] { true,  true,  false,  true,  true,  true,  false,  true,  true,  true,  true,  true,  false,  false,  true,  true,  true,  true,  true,  true,  true,  false,  true,  false,  true, },
        new bool[25] { true,  false,  true,  false,  true,  true,  true,  true,  true,  true,  true,  false,  true,  false,  true,  true,  true,  false,  false,  true,  true,  true,  true,  true,  true, },
        new bool[25] { false,  false,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  false,  true,  true,  false,  true,  true,  false,  true,  false,  true,  true,  true, },
        new bool[25], // pattern_end: it is set to false. This pattern HAS TO be used at the end of all pattern lists of each level
    };

    // pattern: level 2 or hard: 9 patterns (6x6 with 9 black cells)
    private List<bool[]> _listQuestionPatterns_level2 = new List<bool[]>()
    {
        new bool[36] { true,  true,  true,  true,  true,  true,  true,  false,  true,  true,  true,  true,  true,  false,  true,  true,  false,  false,  true,  true,  true,  true,  false,  true,  true,  true,  false,  true,  true,  true,  false,  true,  false,  false,  true,  true, },
        new bool[36] { true,  false,  true,  true,  true,  true,  true,  true,  true,  false,  false,  true,  true,  true,  true,  false,  true,  false,  true,  true,  true,  true,  true,  false,  true,  true,  true,  false,  false,  true,  true,  true,  false,  true,  true,  true, },
        new bool[36] { true,  true,  false,  true,  true,  true,  true,  false,  false,  true,  false,  true,  false,  true,  true,  true,  true,  true,  true,  true,  false,  true,  true,  true,  true,  true,  false,  true,  true,  true,  true,  false,  true,  false,  true,  true, },
        new bool[36] { false,  false,  true,  true,  true,  true,  false,  true,  false,  false,  true,  true,  true,  false,  false,  true,  true,  true,  true,  false,  true,  false,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true, },
        new bool[36] { true,  false,  true,  true,  true,  true,  true,  false,  false,  true,  true,  true,  true,  false,  false,  true,  false,  true,  false,  true,  true,  true,  true,  true,  true,  true,  true,  false,  true,  true,  true,  true,  true,  true,  true,  false, },
        new bool[36] { false,  true,  true,  true,  true,  true,  true,  true,  true,  false,  false,  true,  true,  false,  true,  true,  true,  false,  true,  true,  false,  false,  true,  false,  true,  true,  true,  false,  true,  true,  true,  true,  true,  true,  true,  true, },
        new bool[36] { true,  false,  true,  true,  false,  true,  true,  true,  true,  false,  true,  true,  true,  true,  true,  false,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  false,  false,  true,  false,  true,  true,  false,  true,  false,  true, },
        new bool[36] { true,  false,  false,  true,  true,  true,  true,  false,  true,  false,  false,  true,  true,  true,  true,  true,  false,  true,  true,  true,  true,  false,  true,  true,  false,  true,  false,  true,  true,  true,  true,  true,  true,  true,  true,  true, },
        new bool[36] { true,  false,  true,  false,  true,  false,  false,  true,  true,  true,  true,  false,  true,  true,  false,  true,  true,  true,  true,  true,  true,  true,  true,  true,  false,  true,  true,  true,  false,  true,  true,  true,  true,  false,  true,  true, },
        new bool[36], // pattern_end: it is set to false. This pattern HAS TO be used at the end of all pattern lists of each level
    };

    // answer pattern: pattern to mark answers
    public GameObject MatrixA_level0;
    public GameObject MatrixA_level1;
    public GameObject MatrixA_level2;
    private GameObject _matrixAnswer;
    private Toggle[] _listAnswerCells;

    private bool[] _answerPattern;
    
    public Button Btn_OK;
    private bool _registerAnswer = false;

    //auditory
    public AudioSource BackgroundSound;
    public AudioSource[] AudioPulses = new AudioSource[6]; // up to 6 audio files for up to 6 pulses
    private readonly float _intrinsic_audioDelay = 50f; // ms
    private bool _isAudio;
    
    // haptic
    //vibration. Pattern has to be: off, on, off, on time
    private int Vd = 0;  // vibrationDelay (ms): delay trying to synchronize audio and vibration pattern
    private const int Vt = 50; // vibrationTime
    private List<long[]> _vibrationPatterns;
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
        _pathSettingsHapticDelayFile = GetPathFile("SettingsHapticDelay.txt");
        
        Vd = int.Parse( ReadFile(_pathSettingsHapticDelayFile) );
        _vibrationPatterns = new List<long[]>()
        {
            new long[] {Vd, Vt}, // 1 pulse
            new long[] {Vd, Vt, Vt, Vt}, // 2 pulses
            new long[] {Vd, Vt, Vt, Vt, Vt, Vt}, // 3 pulses
            new long[] {Vd, Vt, Vt, Vt, Vt, Vt, Vt, Vt}, // 4 pulses
            new long[] {Vd, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt}, // 5 pulses
            new long[] {Vd, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt} // 6 pulses
        };
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
        if (_testVersion == "Haptic" || _testVersion == "HapticAuditory")
        {
            // Initialize the plugin for vibrations
            Vibration.Init();
            _toVibrate = true;
        }
        else
            _toVibrate = false;
        // If auditory:
        if (_testVersion == "Auditory" || _testVersion == "HapticAuditory")
            _toPlaySound = true;
        else
            _toPlaySound = false;

        print(_testVersion);
        print(_toPlaySound);
        print(_toVibrate);
        
        // initiate variables for the next level. For example question and answer patterns
        NextLevel();
        
        // general settingss
        _playing = false; // when start method, the game has not started
        _ballDirection = 0;
        ScreenMsg.SetActive(true); // activate welcome message
        ScreenMsg.GetComponentsInChildren<Text>()[0].text = "¡Vamos,\nconcéntrate!";
        
        // highlights for testing level 
        HighlightsInstructions[0].SetActive(false);
        HighlightsInstructions[1].SetActive(false);

        // question pattern
        _listQuestionCells = _matrixQuestion.GetComponentsInChildren<Image>().ToArray(); // list cells in pattern // MatrixQuestion.GetComponentsInChildren<Image>().Skip(1).ToArray(); // first element is the pattern (thus, skip!)
        _kPattern = 0;
        MatrixQuestionController(false); // turn off pattern but enlist next pattern
        // answer pattern
        _listAnswerCells = _matrixAnswer.GetComponentsInChildren<Toggle>().ToArray();
        _matrixAnswer.SetActive(false);
        Btn_OK.onClick.AddListener(BtnOKanswer);
        Btn_OK.gameObject.SetActive(false);
        
        
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
        
        //timers
        _timeSinceEndStimulus = 0; // stimuli timer
        _zeit.Start(); // global timer 
    }
    
    void FixedUpdate()
    {
        // write log messages to log file: it is written every fixed time in order to do not delay the app when writting into disk
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
                        ToLog("_10_ system ball position _ " + _ballPosition +"_NA");
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
        // log any screen touch
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            ToLog("_5_ touch screen (x,y) _ " + (int)touch.position.x + " , " + (int)touch.position.y +"_NA" );
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

        if (_level == -1)
        {
            // level TRAINING
            BallTimeCycle = 5000.0f; // usually it is 3000.0f, but 5000.0 to warm up
            _timesFrame = new int[] {2000, 10000, 15000, 4000, 15000}; // more time for warming up
            // ball positions
            _listBallPosition = _listBallPosition_levelTraining;
            // question patterns
            _listQuestionPatterns = _listQuestionPatterns_levelTraining;
            _matrixQuestion = MatrixQ_level0;
            MatrixQ_level1.SetActive(false);
            MatrixQ_level2.SetActive(false);
            // answer patterns
            _answerPattern = new bool[16]
                {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true};
            _matrixAnswer = MatrixA_level0;
            MatrixA_level1.SetActive(false);
            MatrixA_level2.SetActive(false);
        }
        else if (_level == 0)
        {
            // level BASIC
            // ball positions
            _listBallPosition = _listBallPosition_level0;
            // question patterns
            _listQuestionPatterns = _listQuestionPatterns_level0;
            _matrixQuestion = MatrixQ_level0;
            MatrixQ_level1.SetActive(false);
            MatrixQ_level2.SetActive(false);
            // answer patterns
            _answerPattern = new bool[16] {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true};
            _matrixAnswer = MatrixA_level0;
            MatrixA_level1.SetActive(false);
            MatrixA_level2.SetActive(false);

        } else if (_level == 1)
        {
            // level MIDDLE
            // ball positions
            _listBallPosition = _listBallPosition_level1;
            // patterns
            _listQuestionPatterns = _listQuestionPatterns_level1;
            _matrixQuestion = MatrixQ_level1;
            MatrixQ_level0.SetActive(false);
            MatrixQ_level2.SetActive(false);
            _answerPattern = new bool[25] {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true};
            _matrixAnswer = MatrixA_level1;
            MatrixA_level0.SetActive(false);
            MatrixA_level2.SetActive(false);
            
        } else if (_level == 2)
        {
            // level HEAVY
            // ball positions
            _listBallPosition = _listBallPosition_level2;
            // patterns
            _listQuestionPatterns = _listQuestionPatterns_level2;
            _matrixQuestion = MatrixQ_level2;
            MatrixQ_level0.SetActive(false);
            MatrixQ_level1.SetActive(false);
            _answerPattern = new bool[36] {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true};
            _matrixAnswer = MatrixA_level2;
            MatrixA_level0.SetActive(false);
            MatrixA_level1.SetActive(false);
            
        }
        
        _idxBallPosition = 0; // reset index of ball position
        
        // get number of patters to show by level
        _cyclesByLevel = _listQuestionPatterns.Count-1; // -1 because one extra pattern is added at the end of each list. It was done to keep working the code logic of a predictive brain
    }

    private void SelectStage(long deltaTime)
    {
        if (_nStage == 0) 
        {
            // stage 0 shows a message to prepare user for next game
            // after click Start, game will wait for 2 seconds to start
            if (_timerFrame > 0)
            {
                _timerFrame -= deltaTime;
            }
            else
            {
                // deactivate welcome message
                ScreenMsg.SetActive(false);
                // go to next stage: 1
                if (_level == -1)
                {
                    //highlight some elements if level is testing
                    HighlightsInstructions[0].SetActive(true);
                    HighlightsInstructions[1].SetActive(false);
                }
                _nStage = 1;
                _timerFrame = _timesFrame[_nStage];
                _timerBall = BallTimeCycle; // 3 seconds
                UpdateInstructions();
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
                if (_level == -1)
                {
                    //highlight some elements if level is testing
                    HighlightsInstructions[0].SetActive(true);
                    HighlightsInstructions[1].SetActive(true);
                }
                _nStage = 2;
                _timerFrame = _timesFrame[_nStage];
                UpdateInstructions();
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
                // go to next stage: 3
                if (_level == -1)
                {
                    //highlight some elements if level is testing
                    HighlightsInstructions[0].SetActive(true);
                    HighlightsInstructions[1].SetActive(false);
                }
                _nStage = 3;
                _timerFrame = _timesFrame[_nStage];
                UpdateInstructions();
                MatrixQuestionController(false);
            }
                
        } else if (_nStage == 3) 
        {
            // stage 3 shows only the ball
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
                // go to next stage: 4
                if (_level == -1)
                {
                    //highlight some elements if level is testing
                    HighlightsInstructions[0].SetActive(false);
                    HighlightsInstructions[1].SetActive(true);
                }
                _nStage = 4;
                _timerFrame = _timesFrame[_nStage];
                UpdateInstructions();
                BallController(false);
                MatrixAnswerController(true);
            } 
        } else if (_nStage == 4)
        {
            // stage 4 shows a matrix where the user has to recall the pattern
            // next-frame timer
            if (_timerFrame > 0)
            {
                _timerFrame -= deltaTime;
            }
            else 
            {
                // go to next stage: 0
                if (_level == -1)
                {
                    //highlight some elements if level is testing
                    HighlightsInstructions[0].SetActive(false);
                    HighlightsInstructions[1].SetActive(false);
                }
                MatrixAnswerController(false);
                ScreenMsg.SetActive(true); // activate welcome message
                ScreenMsg.GetComponentsInChildren<Text>()[0].text = "¡Vamos,\nconcéntrate!";
                Ruler.SetActive(true);
                Ball.SetActive(true);
                ArrowsPanel.SetActive(true);
                _nStage = 0;
                _timerFrame = _timesFrame[_nStage];
                UpdateInstructions();
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
        if(_isVibration)
        {
            if (_zeit.ElapsedMilliseconds - _timeSinceVibrationStarts > _vibrationTimePatterns[_idxStimuli])
            {
                // when vibration ends
                _timeSinceEndStimulus = _zeit.ElapsedMilliseconds;
                _isVibration = false;
                _isAudio = false; // because if.
                ToLog("_15_ vibration ends _ " + (_idxStimuli + 1) +"_NA" , _timeSinceEndStimulus );
            }
        }
        else if (_isAudio)
        {
            if (_zeit.ElapsedMilliseconds - _timeSinceAudioPlay > _vibrationTimePatterns[_idxStimuli]) // _vibrationTimePatterns includes the delay included to correct the hw delay to reproduce an audio
            {
                // when audio-play ends
                _timeSinceEndStimulus = _zeit.ElapsedMilliseconds;
                _isAudio = false;
                _isVibration = false; // vibration is false, just in case audio and vibrations are executed in parallel. // after testing, vibration always ends before the auditory stimulus
                ToLog("_13_ sound ends _ " + (_idxStimuli + 1) +"_NA" , _timeSinceEndStimulus);
            }
        } 
        return !(_isAudio | _isVibration);
    }
    
    // append new message to the general log message
    private void ToLog(string msg, float timestamp = -1.0f)
    {
        if (_level < 0) 
            return; // if TESTING level do not log
        if (timestamp<0.0f)
            msg = _zeit.ElapsedMilliseconds +" " + msg + "\n";
        else
        {
            msg = timestamp +" " + msg + "\n";
        }
        _logs += msg;
        print(msg);
    }

    private void UpdateInstructions()
    {
        if (_nStage == 0)
        {
            TxtInstructions_Ball.text = "";
            TxtInstructions_Pattern.text = "";
        } 
        else if (_nStage == 1)
        {
            TxtInstructions_Ball.text = "Cada vez que la pelota se mueva, llévala al centro de la regla lo más rápido que puedas";
            //TxtInstructions_Pattern.text = "";
        } 
        else if (_nStage == 2)
        {
            //TxtInstructions_Ball.text = "Cada vez que la pelota se mueva, llévala al centro de la regla lo más rápido que puedas";
            TxtInstructions_Pattern.text = "Memoriza el patrón";
        } 
        else if (_nStage == 3)
        {
            //TxtInstructions_Ball.text = "Cada vez que la pelota se mueva, llévala al centro de la regla lo más rápido que puedas";
            TxtInstructions_Pattern.text = "";
        } 
        else if (_nStage == 4)
        {
            TxtInstructions_Ball.text = "";
            TxtInstructions_Pattern.text = "Marca el patrón que memorizaste y pulsa OK. Tienes máx. 10 seg.";
        }
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
            ToLog("_10_ system ball position _ " + _ballPosition +"_NA");
            Ball.SetActive(false);
            Ruler.SetActive(false);
            ArrowsPanel.SetActive(false);
        }
    }

    private void Vibrate(long[] vibrationPattern)
    {
        _timeSinceVibrationStarts = _zeit.ElapsedMilliseconds;
        Vibration.Vibrate(vibrationPattern, -1);
        ToLog("_14_ vibration starts _ " + (_idxStimuli + 1) +"_NA", _timeSinceVibrationStarts);
        _isVibration = true;
    }

    private void PlaySound(AudioSource _audio)
    {
        _timeSinceAudioPlay = _zeit.ElapsedMilliseconds;
        _audio.Play(0);
        ToLog("_12_ sound starts _ "+ (_idxStimuli + 1) +"_NA" , _timeSinceAudioPlay );
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
            ToLog("_11_ user ball position _ "+_ballPosition +"_NA");
        } else if (typeMovement < 0)
        {
            // left
            Ball.transform.position = new Vector3(xCurrent-_deltaMovement, _center_intialY, 0);
            _ballPosition -= 1; 
            ToLog("_11_ user ball position _ "+_ballPosition +"_NA");
        }
    }

    private void MatrixQuestionController(bool on)
    {
        if (on)
        {
            // show pattern
            _matrixQuestion.SetActive(true);
            ToLog("_20_ system pattern _ " + (_kPattern) + " _ " + 
                  String.Join(",", new List<bool>(_listQuestionPatterns[(_kPattern)]).ConvertAll(i => i.ToString()).ToArray())
                  );
            // increase counter to point to next pattern
            _kPattern += 1;
        }
        else
        {
            // hide pattern
            _matrixQuestion.SetActive(false);
            // enlist next pattern
            NextPattern(_listQuestionCells, _listQuestionPatterns[_kPattern]);
        }
    }

    private void MatrixAnswerController(bool on)
    {
        if (on)
        {
            _registerAnswer = true;
            _matrixAnswer.SetActive(true);
            Btn_OK.gameObject.SetActive(true);
            Btn_OK.interactable = true;
            ToLog("_21_ answer pattern starts" +"_NA" +"_NA");
        }
        else
        {
            // _registerAnswer is used to control when the log message of the answer-matrix buttons appears. The below foreach calls the internally the function BtnOKanswer() 
            _registerAnswer = false;
            ToLog("_24_ answer pattern ends by system _ " + (_kPattern - 1) + " _ " + 
                  String.Join(",", new List<bool>(_answerPattern).ConvertAll(i => i.ToString()).ToArray())
            );
            _matrixAnswer.SetActive(false);
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
        BackgroundSound.Play();
        _nStage = 0;
        _timerFrame = _timesFrame[_nStage];
        _playing = !_playing;
        Ruler.SetActive(true);
        Ball.SetActive(true);
        ArrowsPanel.SetActive(true);
        _deltaFramesTime = _zeit.ElapsedMilliseconds;
        ToLog("_1_ level starts _ " + _level +"_NA");
    }

    private void BtnOKanswer()
    {
        ToLog("_23_ answer pattern ends by user ok button" +"_NA" +"_NA");
        Btn_OK.interactable = false;
        _matrixAnswer.SetActive(false);
        ScreenMsg.SetActive(true);
        ScreenMsg.GetComponentsInChildren<Text>()[0].text = "Espera...";
        _timerFrame = 500.0f; // reduce time to go to next scene to 500ms
    }

    public void BtnAnswerPattern(int id)
    {
        _answerPattern[id] = !_answerPattern[id];
        if (_registerAnswer)
        {
            ToLog("_22_ user pressed cell _ " + id + " _ " + _answerPattern[id] );
        }
    }

    private void GoToNextScene()
    {
        BackgroundSound.Stop();
        ToLog("_2_ level ends _ " + _level +"_NA");
        WriteLog();
        Loader.Load(Loader.Scene.QuestionnaireScene);
    }
    
}

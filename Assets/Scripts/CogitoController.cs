using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
//using UnityEngine.UIElements;
using UnityEngine.UI;

public class CogitoController : MonoBehaviour
{
    // general settings
    private float _deltaMovement;
    private float _widthScreen;
    private short level; // 0-easy, 1-medium, 2-hard
    private bool _playing;
    private bool _toVibrate = true;
    private bool _toPlaySound = true;
    public Button Btn_OK;
    
    //Timers
    public float BallTimeCycle = 3.0f;
    private float _ballTime;
    public float MatrixTimeCycle = 34.0f;
    private float _matrixTime;
    private float _deltaTime;
    private float _startBallTime;
    private readonly float _timer100ms = 0.100f; // 100 ms
    private float _timeSinceEndStimulus;
    private float _timeSinceAudioPlay;
    private bool _timerIsRunning = true;
    public int[] _timesFrame = new int[] {2,5,5,2,5}; //{2, 10, 10, 4, 10};

    // ball
    public GameObject Ball;
    public GameObject Ruler;
    private float _xCenter;
    private float _yCenter;
    private float _xCurrent;
    private short[] _xPositions = new short[] {1,2,3,4,5};  //from -6 to 6
    private short _ballPosition = 0;
    private short _idPositionX = 0;
    private int _ballDirection;
    private Vector3 _nextPosition;
    private bool _newPosition;
    public GameObject ArrowsPanel;
    
    // visual pattern
    public GameObject MatrixQuestion;
    public GameObject MatrixAnswer;
    private Image[] _listCells;
    private static bool[] _pattern_0 = new bool[16] {true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false};
    private static bool[] _pattern_1 = new bool[16] {false, true, false, true, false, true, false, true, false, true, false, true, false, true, false, true};
    private List<bool[]> _list_patterns = new List<bool[]>(){_pattern_0, _pattern_1};
    private bool[] _answerPattern = new bool[16] {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true};
    private short _kPattern = 0;
    
    //auditory
    public AudioSource[] audioPulses = new AudioSource[6]; // up to 6 audio files for up to 6 pulses
    private readonly float _intrinsic_audioDelay = 50f; // ms
    private bool _isAudio;
    
    // haptic
    //vibration. Pattern has to be: off, on, off, on time
    private const int Vd = 0;  // vibrationDelay: delay trying to synchronize audio and vibration pattern
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
    
    //others to remove later
    public Image cellSound;
    public Image cellVibration;
    
    void Awake()
    {
        _widthScreen = Screen.width;
        print(_widthScreen);
        //height = Screen.height;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        Vibration.Init ();
        
        level = 2;
        // when start method, the game has not started
        _playing = false;
        //
        _ballDirection = 0;
        Btn_OK.gameObject.SetActive(false);
        
        //timers
        _timeSinceEndStimulus = _timer100ms;
        _timerIsRunning = false;
        
        // question pattern
        // pattern's elements
        _listCells = MatrixQuestion.GetComponentsInChildren<Image>().ToArray(); // MatrixQuestion.GetComponentsInChildren<Image>().Skip(1).ToArray(); // first element is the pattern (thus, skip!)
        
        //turn off pattern
        MatrixQuestion.SetActive(false);
        
        // answer pattern
        MatrixAnswer.SetActive(false);
        Btn_OK.onClick.AddListener(BtnOKanswer);
        
        // enlist next pattern
        NextPattern(_listCells, _list_patterns[_kPattern]);
        _kPattern += 1;
        
        // ball
        // initial ball position
        _newPosition = false;
        _xCenter = Ball.transform.position.x;
        _yCenter = Ball.transform.position.y;
        _deltaMovement = _widthScreen / 15;
        
        // audio
        audioPulses = GetComponents<AudioSource>();
        _isAudio = false;
    } 

    // physics of object
    void FixedUpdate()
    {
        if (_isAudio)
        {
            if (Time.time - _timeSinceAudioPlay > audioPulses[0].clip.length)
            {
                if (!audioPulses[0].isPlaying)
                {
                    _timeSinceEndStimulus = Time.time;
                    _isAudio = false;
                    
                    print("finish: " + _timeSinceEndStimulus.ToString("f6"));
                }
            }
        }
        else
        {
            if (_newPosition)
            {
                
                if (Time.time - _timeSinceEndStimulus > _timer100ms - 0.001f)
                {
                    Ball.transform.position = _nextPosition;
                    _newPosition = false;
                    
                    cellSound.color = new Color(1, 1, 1);
                    cellVibration.color = new Color(1, 1, 1);
                    print("ball: " + Time.time.ToString("f6"));
                }
            }
        }
        
        _xCurrent = Ball.transform.position.x;
        
        if (_xCurrent - _xCenter > -(_widthScreen/2 - 2*_deltaMovement) && _ballDirection<0)
        {
            // move left
            MoveBall(_xCurrent, -1);
        } else if (_xCurrent - _xCenter < (_widthScreen/2 - 2*_deltaMovement) && _ballDirection>0)
        {
            // move right
            MoveBall(_xCurrent, +1);
        }

        _ballDirection = 0;

    }

    private int _nFrame = 0;
    private float _timerFrame;
    private float _timerBall;
    
    
    // Update is called once per frame
    void Update()
    {
        if (!_playing)
        {
            BtnStartStop();
        }
        else
        {
            _deltaTime = Time.deltaTime;

            if (_nFrame == 0)
            {
                if (_timerFrame > 0)
                {
                    _timerFrame -= _deltaTime;
                }
                else
                {
                    // after click start, game will wait 2 seconds to start
                    _timerBall = BallTimeCycle; // 3 seconds
                    _nFrame = 1;
                    _timerFrame = _timesFrame[_nFrame];
                    BallController(true);
                    
                }
            } else if (_nFrame == 1)
            {
                // ball
                if (_timerBall > 0)
                {
                    _timerBall -= _deltaTime;
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
                    _timerFrame -= _deltaTime;
                }
                else
                {
                    _nFrame = 2;
                    _timerFrame = _timesFrame[_nFrame];
                    MatrixQuestionController(true);
                }
                
            } else if (_nFrame == 2)
            {
                // ball
                if (_timerBall > 0)
                {
                    _timerBall -= _deltaTime;
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
                    _timerFrame -= _deltaTime;
                }
                else
                {
                    _nFrame = 3;
                    _timerFrame = _timesFrame[_nFrame];
                    MatrixQuestionController(false);
                }
                
            } else if (_nFrame == 3)
            {
                // ball
                if (_timerBall > 0)
                {
                    _timerBall -= _deltaTime;
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
                    _timerFrame -= _deltaTime;
                }
                else
                {
                    _nFrame = 4;
                    _timerFrame = _timesFrame[_nFrame];
                    BallController(false);
                    MatrixAnswerController(true);
                }
            } else if (_nFrame == 4)
            {
                // next-frame timer
                if (_timerFrame > 0)
                {
                    _timerFrame -= _deltaTime;
                }
                else
                {
                    _timerBall = BallTimeCycle;
                    BallController(true);
                    MatrixAnswerController(false);
                    _nFrame = 1;
                    _timerFrame = _timesFrame[_nFrame];
                }
            } else if (_nFrame == 5)
            {
                
            }
        }
    }

    private void BallController(bool on)
    {
        if (on)
        {
            Ball.SetActive(true);
            Ruler.SetActive(true);
            ArrowsPanel.SetActive(true);
            NextBallPosition();
            _startBallTime = Time.time;
        }
        else
        {
            Ball.SetActive(false);
            Ruler.SetActive(false);
            ArrowsPanel.SetActive(false);
        }
    }

    private void Vibrate(long[] vibrationPattern)
    {
        if (_toVibrate)
        {
            cellVibration.color = new Color(0, 0, 0);
            Vibration.Vibrate(vibrationPattern, -1);
        }
    }

    private void PlaySound(AudioSource _audio)
    {
        if (_toPlaySound)
        {
            _timeSinceAudioPlay = Time.time;
            cellSound.color = new Color(0, 0, 0);
            _audio.Play(0);
            _isAudio = true;
            print("start: " + Time.time.ToString("f6"));
        }
    }
    
    private void NextBallPosition()
    {
        print("next ball position");
        _ballPosition = _xPositions[_idPositionX];
        
        //stimuli
        if (level == 2)
        {
            if (_ballPosition != 0)
            {
                // haptic stimulus
                Vibrate(_vibrationPatterns[_ballPosition-1]);
                //auditory stimulus
                PlaySound(audioPulses[_ballPosition-1]);
                // TODO: to add delay for 100ms!!
            }
        }
        
        _nextPosition = new Vector3(_xCenter + _ballPosition* _deltaMovement, _yCenter, 0);
        _newPosition = true;
        
        if (_idPositionX < _xPositions.Length-1 )
        {
            // point to next position
            _idPositionX += 1;
        }
        else
        {
            // reset idx position
            _idPositionX = 0;
        }
        
    }

    // typeMovement: +1(right) ; -1(left)
    private void MoveBall(float xCurrent, short typeMovement)
    {
        print("heereee");
        if (typeMovement > 0)
        {
            // right
            print("yepeeee: ");
            Ball.transform.position = new Vector3(xCurrent+_deltaMovement, _yCenter, 0);
            _ballPosition += 1; 
        } else if (typeMovement < 0)
        {
            // left
            Ball.transform.position = new Vector3(xCurrent-_deltaMovement, _yCenter, 0);
            _ballPosition -= 1; 
        }

        if (_ballPosition == 0)
        {
            var elapsedTime = Time.time - _startBallTime;
            print(_idPositionX + "- elapsed time: " + elapsedTime);
        }
        
    }

    private void MatrixQuestionController(bool on)
    {
        if (on)
        {
            // show pattern
            MatrixQuestion.SetActive(true);
        }
        else
        {
            // hide pattern
            MatrixQuestion.SetActive(false);
            // enlist next pattern
            NextPattern(_listCells, _list_patterns[_kPattern]);
            _kPattern += 1;
            if (_kPattern == 2)
            {
                _kPattern = 0;
            }
        }
    }

    private void MatrixAnswerController(bool on)
    {
        if (on)
        {
            MatrixAnswer.SetActive(true);
            Btn_OK.gameObject.SetActive(true);
            Btn_OK.interactable = true;
        }
        else
        {
            MatrixAnswer.SetActive(false);
            Btn_OK.gameObject.SetActive(false);
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

    private void BtnStartStop()
    {
        _nFrame = 0;
        _timerFrame = _timesFrame[_nFrame];
        _playing = !_playing;
        
        // BORRAR quizá lo siguiente:
        // this will launch multiple iterations
        // InvokeRepeating("LaunchIteration", 2.0f, 34.0f);
        
    }

    private void BtnOKanswer()
    {
        Btn_OK.interactable = false;
        MatrixAnswer.SetActive(false);
    }

    private float _timeToLaunchMatrixQuestion = 10.0f;
    private float _timeToLaunchMatrixAnswer = 24.0f;
    private void LaunchIteration()
    {
        // timer for methods, i.e. tasks
        InvokeRepeating("BallController",   0.0f,BallTimeCycle);
        _timerIsRunning = true;
    }

    public void BtnAnswerPattern(int id)
    {
        _answerPattern[id] = !_answerPattern[id];
        print(id + " : " + _answerPattern[id]);
    }

    public void CheckVibrate()
    {
        _toVibrate = !_toVibrate;
    }

    public void CheckPlaySound()
    {
        _toPlaySound = !_toPlaySound;
    }
    
}

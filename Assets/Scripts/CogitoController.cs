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
    public float _deltaMovement = 0.83f;
    public float _width_screen = 9.3f;
    private short level; // 0-easy, 1-medium, 2-hard
    private bool playing;
    private bool _toVibrate = true;
    private bool _toPlaySound = true;
    
    //Timers
    public float BallTimeCycle = 3.0f;
    private float _ballTime;
    public float MatrixTimeCycle = 34.0f;
    private float _matrixTime;
    private float _deltaTime;
    private float _startBallTime;
    private readonly float _timer100ms = 2.100f; // 100 ms
    private float _timeSinceEndStimulus;
    private float _timeSinceAudioPlay;
    
    // ball
    private float _xCenter;
    private float _yCenter;
    private float _xCurrent;
    private short[] _xPositions = new short[] {1,2,3,4,5};  //from -6 to 6
    private short _ballPosition = 0;
    private short _idx_xPosition = 0;
    private int _whichButton;
    private Vector3 _nextPosition;
    private bool _newPosition;
    
    // visual pattern
    public GameObject Matrix;
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
    private const int Vd = 100;  // vibrationDelay: delay trying to synchronize audio and vibration pattern
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
    
    void Awake()
    {
        _width_screen = Screen.width;
        //height = Screen.height;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        Vibration.Init ();
        
        level = 2;
        // when start method, the game has not started
        playing = false;
        //
        _whichButton = 0;
        
        //timers
        _timeSinceEndStimulus = _timer100ms;
        
        // pattern
        // pattern's elements
        _listCells = Matrix.GetComponentsInChildren<Image>().Skip(1).ToArray(); // first element is the pattern (thus, skip!)
        //turn off pattern
        Matrix.SetActive(false);
        MatrixAnswer.SetActive(false);
        // enlist next pattern
        NextPattern(_listCells, _list_patterns[_kPattern]);
        _kPattern += 1;
        
        // ball
        // initial ball position
        _newPosition = false;
        _xCenter = transform.position.x;
        _yCenter = transform.position.y;
        
        // audio
        audioPulses = GetComponents<AudioSource>();
        _isAudio = false;

        /*
        // initial clock times
        _ballTime = BallTime;
        _matrixTime = MatrixTime;
        */

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
                    transform.position = _nextPosition;
                    _newPosition = false;
                    
                    print("ball: " + Time.time.ToString("f6"));
                }
            }
        }



    //string currentTime = Time.time.ToString("f6");
        //string currentTime = Time.fixedDeltaTime.ToString("f6");
        //print("fixed: " + currentTime);
        _xCurrent = transform.position.x;
        
        if (_xCurrent - _xCenter > -(_width_screen/2) && _whichButton<0)
        {
            // move left
            MoveBall(_xCurrent, -1);
        } else if (_xCurrent - _xCenter < (_width_screen/2) && _whichButton>0)
        {
            // move right
            MoveBall(_xCurrent, +1);
        }

        _whichButton = 0;

    }

    // Update is called once per frame
    void Update()
    {
        if (!playing)
        {
            BtnStart();
            playing = true;
        }
        
        //string currentTime = Time.time.ToString("f6");
        //string currentTime = Time.deltaTime.ToString("f6");
        //print("updated: " + currentTime);
        
        //    _startBallTime = Time.time;
        //    _ballTime = BallTime;
        
    }

    private void BallController()
    {
        NextBallPosition();
        _startBallTime = Time.time;
    }

    private void Vibrate(long[] vibrationPattern)
    {
        if (_toVibrate)
        {
            Vibration.Vibrate(vibrationPattern, -1);
        }
    }

    private void PlaySound(AudioSource _audio)
    {
        if (_toPlaySound)
        {
            _timeSinceAudioPlay = Time.time;
            _audio.Play(0);
            _isAudio = true;
            print("start: " + Time.time.ToString("f6"));
        }
    }
    
    private void NextBallPosition()
    {
        _ballPosition = _xPositions[_idx_xPosition];
        
        //playing audio
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
        
        if (_idx_xPosition < _xPositions.Length-1 )
        {
            // point to next position
            _idx_xPosition += 1;
        }
        else
        {
            // reset idx position
            _idx_xPosition = 0;
        }
        
    }

    // typeMovement: +1(right) ; -1(left)
    private void MoveBall(float xCurrent, short typeMovement)
    {
        if (typeMovement > 0)
        {
            // right
            transform.position = new Vector3(xCurrent+_deltaMovement, _yCenter, 0);
            _ballPosition += 1; 
        } else if (typeMovement < 0)
        {
            // left
            transform.position = new Vector3(xCurrent-_deltaMovement, _yCenter, 0);
            _ballPosition -= 1; 
        }

        if (_ballPosition == 0)
        {
            var elapsedTime = Time.time - _startBallTime;
            print(_idx_xPosition + "- elapsed time: " + elapsedTime);
        }
        
    }

    private void MatrixController()
    {
        if (Matrix.activeSelf)
        {
            // hide pattern
            Matrix.SetActive(false) ;
            // enlist next pattern
            NextPattern(_listCells, _list_patterns[_kPattern]);
            _kPattern += 1;
            if (_kPattern == 2)
            {
                _kPattern = 0;
                MatrixAnswer.SetActive(true);
            }
        }
        else
        {
            //show pattern
            Matrix.SetActive(true);
            Invoke("MatrixController",10f);
        }
    }
    
    // NextPattern enlists the next pattern by changing cell colors 
    private void NextPattern(Image[] listCells, bool[] pattern)
    {
        int k = 0;
        foreach (Image cell in listCells)
        {
            /*
            if (pattern[k] == true)
            {
                // paint white cells in 1
                cell.color = new Color(1,1,1);
            }
            else
            {
                // paint black cells in 0
                cell.color = new Color(0,0,0);
            }
            */
            
            // paint white cells in 1 and paint black cells in 0
            cell.color =  (pattern[k] == true) ?  new Color(1,1,1) : cell.color = new Color(0,0,0);
            k += 1;
        }
    }
    
    public void BtnRight()
    {
        _whichButton = 1;
    } 
    
    public void BtnLeft()
    {
        _whichButton = -1;
    }

    private void BtnStart()
    {
        // timer for methods, i.e. tasks
        InvokeRepeating("MatrixController", 2.0f+10.0f,MatrixTimeCycle);
        InvokeRepeating("BallController", 2.0f+0.0f, BallTimeCycle);
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

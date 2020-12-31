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
    
    //Timers
    public float BallTime = 3.0f;
    private float _ballTime;
    public float MatrixTime = 34.0f;
    private float _matrixTime;
    private float _deltaTime;
    private float _startBallTime;
    
    // ball
    private float _xCenter;
    private float _yCenter;
    private float _xCurrent;
    private short[] _xPositions = new short[] {1,2,3,4,5};  //from -6 to 6
    private short _ballPosition = 0;
    private short _idx_xPosition = 0;
    private int _whichButton;
    
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
    public AudioSource audioData;
    
    // haptic
    //vibration. Pattern has to be: off, on, off, on time
    private const int Vt = 50; // vibrationTime
    private const int Vd = 0;  // vibrationDelay: delay trying to synchronize audio and vibration pattern
    private readonly List<long[]> _vibrationPatterns = new List<long[]>(){ new long[] {0}, new long[] {Vd, Vt}, new long[] {Vd, Vt, Vt, Vt}, new long[] {Vd, Vt, Vt, Vt, Vt, Vt}, new long[] {Vd, Vt, Vt, Vt, Vt, Vt, Vt, Vt}, new long[] {Vd, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt}, new long[] {Vd, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt, Vt} };
    // Start is called before the first frame update
    void Start()
    {
        Vibration.Init ();
        
        level = 2;
        // when start method, the game has not started
        playing = false;
        //
        _whichButton = 0;
        // pattern's elements
        _listCells = Matrix.GetComponentsInChildren<Image>().Skip(1).ToArray(); // first element is the pattern (thus, skip!)
        //turn off pattern
        Matrix.SetActive(false);
        MatrixAnswer.SetActive(false);
        // enlist next pattern
        NextPattern(_listCells, _list_patterns[_kPattern]);
        _kPattern += 1;
        // initial ball position
        _xCenter = transform.position.x;
        _yCenter = transform.position.y;
        
        audioData = GetComponent<AudioSource>();
        
        /*
        // initial clock times
        _ballTime = BallTime;
        _matrixTime = MatrixTime;
        */
        
    } 

    // physics of object
    void FixedUpdate()
    {
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

        /*
        _deltaTime = Time.deltaTime; 
        
        //time for matrix
        _matrixTime -= _deltaTime;
        if (_matrixTime <= 0)
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
                }
            }
            else
            {
                //unhide pattern
                Matrix.SetActive(true);
            }
            
            _matrixTime = MatrixTime;
        }
        */

        /*
        //time for ball 
        _ballTime -= _deltaTime;
        if ( _ballTime <= 0 )
        {
            NextBallPosition();
            _startBallTime = Time.time;
            _ballTime = BallTime;
        }
        */

    }

    private void BallController()
    {
        NextBallPosition();
        _startBallTime = Time.time;
    }

    private void NextBallPosition()
    {
        _ballPosition = _xPositions[_idx_xPosition];
        
        //playing audio
        if (level == 2)
        {
            Vibration.Vibrate ( _vibrationPatterns[_ballPosition], -1 );
            
            audioData.Play(0);
            // to add delay for 100ms!!
        }
        
        transform.position = new Vector3(_xCenter + _ballPosition* _deltaMovement, _yCenter, 0);
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
        InvokeRepeating("MatrixController", 2.0f+10.0f,MatrixTime);
        InvokeRepeating("BallController", 2.0f+0.0f, BallTime);
    }

    public void BtnAnswerPattern(int id)
    {
        _answerPattern[id] = !_answerPattern[id];
        print(id + " : " + _answerPattern[id]);
    }
    
    
}

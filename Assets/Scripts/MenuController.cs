using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    // general variables
    public Button BtnStartGame;
    public Button BtnBorrar; //TODO: borrar este objeto y método correspondiente StartQuestionnaires()
    private string _pathLogFile;
    public Text TextforPath;
    
    void Awake()
    {
        print("0");
        BtnStartGame.onClick.AddListener(StartGame);
        BtnBorrar.onClick.AddListener(StartQuestionnaires);
        print("1");
    }
    // Start is called before the first frame update
    void Start()
    {
        // initial logs
        //Path of the file
        // TODO: this variable should be written into a settings file
        if ( SystemInfo.deviceModel == "PC")
        {
            _pathLogFile = Application.dataPath + "/Log.txt";
        }
        else
        {
            _pathLogFile = Application.persistentDataPath + "/Log.txt";
        }
     
        print("a");
        
        // set path of log file to code for sharing
        //TODO: this setpath sould write it into a setting file
        TextforPath.text = _pathLogFile;
        print("b");
    }

    private void StartGame()
    {
        Loader.Load(Loader.Scene.GameScene);
        // _nStage = 0;
        // _timerFrame = _timesFrame[_nStage];
        // _playing = !_playing;
        // BtnStartGame.gameObject.SetActive(false);
        // BtnShare.gameObject.SetActive(false);
        // TextforPath.enabled = false;
        // Ruler.SetActive(true);
        // Ball.SetActive(true);
        // ArrowsPanel.SetActive(true);
        // _deltaFramesTime = _zeit.ElapsedMilliseconds;
        // ToLog("-1- game starts");
    }

    private void StartQuestionnaires()
    {
        Loader.Load(Loader.Scene.QuestionnaireScene);
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System.IO;

public class SurveyController : MonoBehaviour
{
    private bool waitingFirstClick = true;
    
    // general objects
    public GameObject PanelNasaInformation;
    public GameObject QuestionNasa;
    public Text WarningText;
    public Button BtnMenu;
    public int level = 2;
    private int nStage = -1; // start in -1
    private string _pathLogFile;
    private string _logs = "";
    
    // Timers
    private Stopwatch _zeit = new Stopwatch();
    
    // variables for Nasa
    // slider for answer
    public GameObject HandleSlider;
    private GameObject HandleSliderImg;
    private float sliderValue;
    // texts for questions
    public Text TextQuestionNasa;
    public Text TextExplanationNasa;
    public Text TextQuestionN;
    private string[] questions = new string[6] 
    {
        "EXIGENCIA MENTAL",
        "EXIGENCIA FÍSICA", 
        "EXIGENCIA TEMPORAL", 
        "DESEMPEÑO", 
        "ESFUERZO",
        "FRUSTRACIÓN"
    };
    private string[] explanations = new string[6]
    {
        " ¿Qué tanta exigencia mental tuvo la tarea?",
        "¿Qué tanta demanda física tuvo la tarea?",
        "¿Qué tan apresurada o afanada fue la tarea?",
        "¿Qué tan exitoso fuiste haciendo la tarea solicitada?",
        "¿Qué tan duro tuviste que trabajar para lograr tu nivel de desempeño?",
        "¿Qué tan inseguro, desanimado, irritado, estresado y molesto estuviste?"
    };
    
    // variables for other questions
    public GameObject QuestionVibration;
    public GameObject QuestionSound;
    public GameObject ToggleGroupV;
    public GameObject ToggleGroupS;
    
    void Start()
    {
        BtnMenu.onClick.AddListener(GoToMenu);
        
        HandleSliderImg = HandleSlider.GetComponent<Image>().gameObject;
        HandleSliderImg.SetActive(false);
        // elements for nasa
        PanelNasaInformation.SetActive(false);
        QuestionNasa.SetActive(false);
        
        // other questions
        QuestionVibration.SetActive(false);
        QuestionSound.SetActive(false);
        
        // initial logs
        //Path of the file
        if ( SystemInfo.deviceModel == "PC")
        {
            _pathLogFile = Application.dataPath + "/Log.txt";
        }
        else
        {
            _pathLogFile = Application.persistentDataPath + "/Log.txt";
        }
        
        // global timer 
        _zeit.Start();
        
        //append msg to log for adding later to log file
        ToLog("-30- survey starts: " + System.DateTime.Now );
    }
    
    void FixedUpdate()
    {
        // write log messages to log file
        WriteLog();
    }
    
    // append new message to the general log message 
    private void ToLog(string msg)
    {
        msg = _zeit.ElapsedMilliseconds +" " + msg + "\n";
        _logs += msg;
        print(msg);
    }
    
    private void WriteLog()
    {
        File.AppendAllText(_pathLogFile, _logs);
        _logs = "";
    }
    
    public void OnValueChangedSlider(float value)
    {
        if (waitingFirstClick)
        {
            HandleSliderImg.SetActive(true);
            waitingFirstClick = false;
        }

        sliderValue = value;
        print(sliderValue);
    }
    
    public void OnClickBtnNext()
    {
        if (nStage < 0)
        {
            // activate nasa elements
            PanelNasaInformation.SetActive(true);
            QuestionNasa.SetActive(true);
            
            // make ready next question
            nStage = 0;
            ResetSlider();
            ToLog("-31- next question starts");
        } else if (nStage < 5)
        {
            if (waitingFirstClick)
            {
                WarningText.text = "Marca una respuesta";
            }
            else
            {
                WarningText.text = "";
                // capture results from slider
                // log message
                ToLog("-32- question: " + (nStage) + " : " + questions[nStage] + " : " + sliderValue);

                // make ready next question
                nStage += 1;
                ResetSlider();
                ToLog("-31- next question starts");
            }
        }
        else if (nStage == 5 && level == 2)
        {
            // capture results from slider
            // log message
            ToLog("-32- question: " + (nStage) + " : " + questions[nStage] + " : " + sliderValue);
            nStage += 1;
            
            // section for questions about sounds and vibrations
            
            // deactivate elements for nasa
            PanelNasaInformation.SetActive(false);
            QuestionNasa.SetActive(false);
        
            // activate elements for other questions
            ToLog("-31- next question starts");
            QuestionVibration.SetActive(true);
            QuestionSound.SetActive(true);
        }
        else
        {
            if (level < 2)
            {
                // go to next level
                print("next game level");
            }
            else if (nStage == 6 && level == 2)
            {
                // get toggle answer
                // log message
                ToLog("-33- sound question : " + GetSelectedToggle(ToggleGroupS));
                ToLog("-34- vibration question : " + GetSelectedToggle(ToggleGroupV));

                // go to main for sending data
                nStage += 1;
            }
        }
    }

    private string GetSelectedToggle(GameObject tG)
    {
        Toggle[] toggles= tG.GetComponentsInChildren<Toggle>();
        foreach (var t in toggles)
        {
            //returns selected toggle
            if (t.isOn)
            {
                if (t.name == "ToggleY") return "yes";
                return "no";
            }
        }
              
        return null;           // if nothing is selected return null
    }
    
    private void ResetSlider()
    {
        HandleSliderImg.SetActive(false);
        waitingFirstClick = true;
        TextQuestionNasa.text = questions[nStage];
        TextQuestionN.text = (nStage + 1) + " de 6";
        TextExplanationNasa.text = explanations[nStage];
    }

    private void GoToMenu()
    {
        Loader.Load(Loader.Scene.MenuScene);
    }
    
}

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
    public Text InformationText;
    public Button BtnMenu;
    private int _level;
    private int nStage = 0; // start in -1
    private string _pathLogFile;
    private string _logs = "";
    private string _pathSettingsFile;
    
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
        // general variables
        BtnMenu.onClick.AddListener(GoToMenu);
        
        HandleSliderImg = HandleSlider.GetComponent<Image>().gameObject;
        HandleSliderImg.SetActive(false);
        // elements for nasa
        PanelNasaInformation.SetActive(false);
        QuestionNasa.SetActive(false);
        
        // other questions
        QuestionVibration.SetActive(false);
        QuestionSound.SetActive(false);
        
        // Path to log file
        _pathLogFile = GetPathFile("Log.txt");
        
        // define level for the current questionnaire
        _pathSettingsFile = GetPathFile("SettingsLevel.txt");
        _level = GetLevel();
            
        // global timer 
        _zeit.Start();
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
    
    private string ReadFile(string filePath)
    {
        // read filepath and return all text as one string
        return System.IO.File.ReadAllText(filePath);
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
    
    private short GetLevel()
    {
        // read settings file
        return short.Parse( ReadFile(_pathSettingsFile) );
    }
    
    public void OnClickBtnNext()
    {
        if (nStage == 0)
        {
            // deactivate previous objects
            InformationText.gameObject.SetActive(false);
            // activate nasa elements
            PanelNasaInformation.SetActive(true);
            QuestionNasa.SetActive(true);
            // make ready the first nasa question
            //append msg to log for adding later to log file
            ToLog("-3- survey starts at: " + System.DateTime.Now );
            NextQuestion();
        } else if (nStage < 7)
        {
            if (waitingFirstClick)
            {
                WarningText.text = "Marca una respuesta";
            }
            else
            {
                WarningText.text = "";
                // capture results from previous question, i.e. slider
                ToLog("-32- question: " + (nStage-1) + " : " + questions[nStage-1] + " : " + sliderValue);
                if (nStage < 6)
                {
                    // make ready next question
                    NextQuestion();
                }
                else
                {
                    // if it is last question, test if go to next question or next level
                    if (_level == 2)
                        NextQuestion();
                    else
                    {
                        GoToNextScene();
                    } 
                }
                    
                
            }
        }
        else if (nStage == 7)
        {
            if (GetSelectedToggle(ToggleGroupS) is null || GetSelectedToggle(ToggleGroupV) is null)
            {
                WarningText.text = "Responde todas las preguntas";
                return;
            }
            WarningText.text = "";
            // get toggle answer
            // log message
            ToLog("-33- sound question : " + GetSelectedToggle(ToggleGroupS));
            ToLog("-34- vibration question : " + GetSelectedToggle(ToggleGroupV));
            NextQuestion();
            
        } else if (nStage == 8)
        {
            WarningText.text = "";
            // go to main for sending data
            GoToNextScene();
        }
    }

    private string GetSelectedToggle(GameObject tG)
    {
        Toggle[] toggles= tG.GetComponentsInChildren<Toggle>();
        foreach (var t in toggles)
        {
            //returns selected toggle
            if (t.isOn) return t.name;
        }
        return null;           // if nothing is selected return null
    }

    //reset slider and make ready text for next question
    private void NextQuestion()
    {
        if (nStage < 6)
        {
            // NASA questions
            // set values for components
            HandleSliderImg.SetActive(false);
            waitingFirstClick = true;
            TextQuestionNasa.text = questions[nStage];
            TextQuestionN.text = (nStage + 1) + " de 6";
            TextExplanationNasa.text = explanations[nStage];
            
            // log message
            ToLog("-31- next question starts");
        }
        else if (nStage == 6)
        {
            // SOUND and VIBRATION questions
            // deactivate elements for nasa
            PanelNasaInformation.SetActive(false);
            QuestionNasa.SetActive(false);
            // activate elements for other questions
            QuestionVibration.SetActive(true);
            QuestionSound.SetActive(true);
            
            // log message
            ToLog("-31- next question starts");
        }
        else
        {
            // screen to say thank you
            // deactivate elements for other questions
            QuestionVibration.SetActive(false);
            QuestionSound.SetActive(false);
            
            InformationText.gameObject.SetActive(true);
            InformationText.text = "¡Muchas gracias por tu participación! :). \n\n " +
                                   "Ahora envia un correo con tus resultados usando el botón Enviar Resultados \n" +
                                   "en la siguiente pantalla";
        }
        nStage += 1;
    }

    
    private void GoToNextScene()
    {
        // write next level into settings file:
        int nextLevel = _level + 1;
        if (nextLevel < 3)
        {
            // go to next level game
            File.WriteAllText(_pathSettingsFile, "" + nextLevel );
            ToLog("-4- survey ends");
            WriteLog();
            Loader.Load(Loader.Scene.GameScene);
        }
        else
        {
            ToLog("-4- survey ends");
            WriteLog();
            // end game
            Loader.Load(Loader.Scene.MenuScene);
        }
    }

    private void GoToMenu()
    {
        Loader.Load(Loader.Scene.MenuScene);
    }
}

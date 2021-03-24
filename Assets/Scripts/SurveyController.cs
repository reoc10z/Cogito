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
    private int _level;
    private int nStage; 
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
        nStage = 0;
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

    void Update()
    {
        // log any screen touch
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            ToLog("_5_ touch screen (x,y) _ " + (int)touch.position.x + " , " + (int)touch.position.y +"_NA" );
        }
    }
    
    // append new message to the general log message 
    private void ToLog(string msg)
    {
        if (_level < 0) 
            return; // if TESTING level do not log
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
            ToLog("_3_ survey starts at _ " + System.DateTime.Now +"_NA" );
            NextQuestion(nStage);
            nStage += 1;
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
                ToLog("_32_ question: " + questions[nStage-1] + " _ " + (nStage-1) + " _ " + sliderValue);
                if (nStage < 6)
                {
                    // make ready next question
                    NextQuestion(nStage);
                    nStage += 1;
                }
                else
                {
                    if (_level == -1)
                    {
                        // if level for practice
                        NextQuestion(9);
                        nStage = 9;
                    }
                    else if (_level == 0 || _level == 1)
                    {
                        // easy and medium level
                        NextQuestion(8);
                        nStage = 8;
                    }
                    else if (_level == 2)
                    {
                        // if it is last question, test if to go to next question or to next level
                        NextQuestion(nStage);
                        nStage += 1;
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
            ToLog("_33_ sound question _ " + GetSelectedToggle(ToggleGroupS) +"_NA");
            ToLog("_34_ vibration question _ " + GetSelectedToggle(ToggleGroupV) +"_NA");
            NextQuestion(nStage);
            nStage += 1;
            
        } else if (nStage == 8 || nStage == 9)
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
    private void NextQuestion(int nextQuestion)
    {
        if (nextQuestion < 6)
        {
            // NASA questions
            // set values for components
            HandleSliderImg.SetActive(false);
            waitingFirstClick = true;
            TextQuestionNasa.text = questions[nextQuestion];
            TextQuestionN.text = (nextQuestion + 1) + " de 6";
            TextExplanationNasa.text = explanations[nextQuestion];
            
            // log message
            ToLog("_31_ next question starts_NA_NA");
        }
        else if (nextQuestion == 6)
        {
            // SOUND and VIBRATION questions
            // deactivate elements for nasa
            PanelNasaInformation.SetActive(false);
            QuestionNasa.SetActive(false);
            // activate elements for other questions
            QuestionVibration.SetActive(true);
            QuestionSound.SetActive(true);
            
            // log message
            ToLog("_31_ next question starts_NA_NA");
        }
        else if (nextQuestion == 7)
        {
            // last screen for hard level: screen to say thank you
            // deactivate elements for other questions
            QuestionVibration.SetActive(false);
            QuestionSound.SetActive(false);
            
            InformationText.gameObject.SetActive(true);
            InformationText.text = "¡Muchas gracias por tu participación! :). \n\n " +
                                   "Ahora, \n"+
                                   "1- Activa tus datos o wifi, \n" +
                                   "2- Envia un correo con tus resultados usando el botón Enviar Resultados en la siguiente pantalla";
        }
        else if (nextQuestion == 8)
        {
            // last screen for easy and medium level
            // deactivate elements for nasa
            PanelNasaInformation.SetActive(false);
            QuestionNasa.SetActive(false);
            
            // screen for ending the game for practice
            InformationText.gameObject.SetActive(true);
            InformationText.text = "¡Eso es!.\nAhora, el siguiente nivel.";
            
        }
        else if (nextQuestion == 9)
        {
            // last screen for practice level
            // deactivate elements for nasa
            PanelNasaInformation.SetActive(false);
            QuestionNasa.SetActive(false);
            
            // screen for ending the game for practice
            InformationText.gameObject.SetActive(true);
            InformationText.text = "¡Genial!\nTerminaste la práctica. Ahora ve a la opción: \"Iniciar el Test\".\n"+
                                   "O puedes volver a practicar.";
        }
    }
    
    private void GoToNextScene()
    {
        if (_level == 0 || _level == 1)
        {
            // for basic and medium level, go to next level game
            // write next level into settings file:
            int nextLevel = _level + 1;
            File.WriteAllText(_pathSettingsFile, "" + nextLevel );
            ToLog("_4_ survey ends_NA_NA");
            WriteLog();
            Loader.Load(Loader.Scene.GameScene);
        }
        else
        {
            // for last level, i.e. hard level; and for level for practice
            ToLog("_4_ survey ends_NA_NA");
            WriteLog();
            // end game
            Loader.Load(Loader.Scene.MenuScene);
        }
    }
    
}

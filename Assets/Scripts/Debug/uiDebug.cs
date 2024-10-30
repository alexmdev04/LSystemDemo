using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class uiDebug : MonoBehaviour
{
    public static uiDebug instance { get; private set; }
    [SerializeField] TextMeshProUGUI
        uiFPSText,
        uiRes,
        uiNotes,
        uiStatsPlayer,
        uiStatsMiscellaneous,
        uiStatsCamera,
        uiVersion;
    [SerializeField] GameObject 
        uiDebugGroup,
        uiFPS;
    [SerializeField] bool showNotes;
    [SerializeField] GameObject torchCrosshair;
    [SerializeField] float torchSwayDebugMultiplier = 1080f;
    RectTransform torchCrosshairTransform;
    public bool debugMode { get; private set; }
    public bool debugLines { get; private set; }
    public bool uiFPSEnabled;
    public float noclipSpeed = 10f;
    [Tooltip("Lower is faster")] public float statsRepeatRate = 0.02f;
    bool noclipEnabled, godEnabled;
    LensDistortion lensDistortion;
    int fps;
    public const string
        // player
        str_playerTitle = "<u>Player/Game;</u>",
        str_targetFPS = "\ntargetFPS = ",
        str_vSync = ", vSync = ",
        str_mouseRotation = "\nmouseRotation = ",
        str_lookSensitivity = "\nlookSensitivity = ",
        str_playerDimensions = "\nplayerRadius = ",
        str_multiply = " * ",
        // level
        str_levelTitle = "<u>Level;</u>\ninLevel = ",
        str_assetKey = "\nassetKey = ",
        str_inGameName = "\ninGameName = ",
        str_objectives = "\nobjectives; ",
        str_playerStartPos = "\nplayerStartPos = ",
        // stealth
        str_stealthTitle = "<u>Stealth;</u>",
        str_stealthLevel = "\nstealthLevel = ",
        str_stealthTimer = "\nstealthTimer = ",
        str_playerVisible = "\nplayerVisible = ",
        str_toLevelSpotted = "\ntoLevelSpotted = ",
        str_toLevelFullAlert = "\ntoLevelFullAlert = ",
        str_fromLevelSpotted = "\nfromLevelSpotted = ",
        str_toGameOver = "\ntoGameOver = ",
        str_toGameOverSequnceRunning = "\ntoGameOverSequnceRunning = ",
        //other
        str_x = "x",
        str_v = "v",
        str_dash = " - ",
        str_equals = " = ",
        str_divide = " / ",
        str_NewLine = "\n",
        str_NewLineDash = "\n - ",
        str_Hz = "Hz",
        str_ResFormat = "{0}:{1}",
        str_noclip = "noclip ",
        str_god = "god mode ",
        str_enabled = "enabled",
        str_disabled = "disabled",
        str_notesTitle = "<u>Notes:</u>",
        str_todoTitle = "<u>\nTo do:</u>",
        // class names
        str_uiDebug = "uiDebug",
        str_uiDebugConsole = "uiDebugConsole",
        str_StealthHandler = "StealthHandler",
        str_Level = "Level",
        str_LevelGoal = "LevelGoal",
        str_LevelLoader = "LevelLoader",
        str_MenuLevel = "MenuLevel";

    void Awake()
    { 
        instance = this;
        uiVersion.text = str_v + Application.version;
        uiFPSText = uiFPS.GetComponent<TextMeshProUGUI>();
        #if UNITY_EDITOR
        debugMode = true;
        #endif
        Game.instance.globalVolume.profile.TryGet(out lensDistortion);
        torchCrosshairTransform = torchCrosshair.GetComponent<RectTransform>();
    }
    void Start()
    {
        uiDebugConsole.instance.Start();
        StartRepeating();
        InvokeRepeating(nameof(GetStatsAlways), 0f, statsRepeatRate);
    }
    void StartRepeating()
    {
        InvokeRepeating(nameof(GetRes), 0f, 1f);
        InvokeRepeating(nameof(GetDebugNotes), 0f, 1f);
        InvokeRepeating(nameof(GetStats), 0f, statsRepeatRate);
    }
    void StopRepeating()
    {
        CancelInvoke(nameof(GetRes));
        CancelInvoke(nameof(GetDebugNotes));
        CancelInvoke(nameof(GetStats));
    }
    public void RefreshRepeating()
    {
        CancelInvoke();
        StartRepeating();
        CancelInvoke(nameof(GetStatsAlways));
        InvokeRepeating(nameof(GetStatsAlways), 0f, statsRepeatRate);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3)) 
        {
            debugMode = !debugMode; 
            if (debugMode) { StartRepeating(); }
            else { StopRepeating();  }
            if (!uiFPSEnabled) uiFPS.SetActive(debugMode);
        }
        uiDebugGroup.SetActive(debugMode);
        if (debugMode & !uiDebugConsole.instance.gameObject.activeSelf) { Controls(); }
        /* toggles debug console -> */ if ((Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Tilde)) && debugMode) { uiDebugConsole.instance.gameObject.SetActive(!uiDebugConsole.instance.gameObject.activeSelf); }
        Noclip();
        //Player.instance.moveActive = !noclipEnabled;
        TorchSwayDebug();
        GetFPS();
    }
    void GetRes() // gets the current resolution, refresh rate and aspect ratio
    {
        float gcd = Extensions.CalcGCD(Screen.width, Screen.height);
        uiRes.text = Screen.width.ToString() + str_x + Screen.height.ToString() + str_NewLine
            + Screen.currentResolution.refreshRateRatio + str_Hz + str_NewLine +
            (string.Format(str_ResFormat, Screen.width / gcd, Screen.height / gcd));
    }
    void GetFPS() // fps counter
    {
        fps = (int)(1.0f / Time.unscaledDeltaTime);
    }
    void GetStats() // contructs all stats for the debug overlay, uses stringbuilder & append to slightly improve performance
    {
        if (!debugMode)
        {
            uiStatsPlayer.text = string.Empty;
            uiStatsMiscellaneous.text = string.Empty;
            return;
        }
        uiStatsPlayer.text = Player.instance.debugGetStats().ToString();
        uiStatsMiscellaneous.text = new StringBuilder("<u>Miscellaneous;</u>")
            .Append("\nuiFadeAlpha = ").Append(ui.instance.uiFadeAlpha).ToString();
        uiStatsCamera.text = CameraHandler.instance.debugGetStats().ToString();
    }

    void GetStatsAlways()
    {
        uiFPSText.text = fps.ToString();
    }
    void GetDebugNotes() // scans all loaded scenes and their root game objects for uiDebugNote components and combines them all
    {
        if (!showNotes) { return; }
        if (!debugMode) { return; }
        List<uiDebugNote> noteComponents = new();
        List<Scene> scenes = new List<Scene>();
        StringBuilder notesText = new(str_notesTitle), todosText = new (str_todoTitle);

        for (int i = 0; i < SceneManager.sceneCount; i++) { scenes.Add(SceneManager.GetSceneAt(i)); }
        foreach (Scene scene in scenes) 
        { 
            foreach (GameObject rootObject in scene.GetRootGameObjects()) 
            {
                uiDebugNote noteComponent = rootObject.GetComponent<uiDebugNote>();
                if (noteComponent != null) { noteComponents.Add(noteComponent); }
            }
        }
        foreach (uiDebugNote noteComponent in noteComponents)
        {
            if (noteComponent.notes.Length > 0)
            {
                notesText.Append(str_NewLine).Append(noteComponent.gameObject.name);
                foreach (string note in noteComponent.notes) { notesText.Append(str_NewLineDash).Append(note); }
            }

            if (noteComponent.toDos.Length > 0)
            {
                todosText.Append("\n").Append(noteComponent.gameObject.name);
                foreach (string todo in noteComponent.toDos) { todosText.Append(str_NewLineDash).Append(todo); }
            }
        }
        uiNotes.text = notesText.Append(todosText).ToString();
    }
    void Controls()
    {
        //if (Input.GetKey(KeyCode.LeftControl))
        //{
        //    lensDistortion.intensity.value -= Input.mouseScrollDelta.y * 0.05f;
        //}
        //else
        //{
        //    CameraHandler.mainCamera.fieldOfView -= Input.mouseScrollDelta.y;
        //}

        //if (Player.instance.frictionType)
        //{
        //    Player.instance.friction2 = Math.Clamp(Player.instance.friction2 + (Input.mouseScrollDelta.y * 0.1f), 1f, 5f);
        //}
        //else
        //{
        //    Player.instance.friction1 = Math.Clamp(Player.instance.friction1 + (Input.mouseScrollDelta.y * 0.01f), 0.01f, 1f);
        //}

        // enables debug notes on screen
        if (Input.GetKeyDown(KeyCode.Insert)) { showNotes = !showNotes; }

        if (Input.GetKeyDown(KeyCode.F4)) { debugLines = debugMode && !debugLines; }
        if (Input.GetKeyDown(KeyCode.F5)) { WorldGen.instance.refresh = true; }
        if (Input.GetKeyDown(KeyCode.F6)) { ToggleNoclip(); }
        // 
        if (Input.GetKeyDown(KeyCode.F9)) { ui.instance.ToggleSpeedometer(); }
    }
    void Noclip()
    {
        if (!noclipEnabled) { Player.instance.PlayerFreeze(false); return;  }
        else { Player.instance.PlayerFreeze(true); }
        // wasd movement
        if (uiDebugConsole.instance.gameObject.activeSelf) { return; }
        Player.instance.transform.position += noclipSpeed * (Player.instance.sprinting ? 2 : 1) * Time.deltaTime * Player.instance.cameraTransformReadOnly.TransformDirection(new(Extensions.FloatFromAxis(Input.GetKey(KeyCode.D), Input.GetKey(KeyCode.A)), 0, Extensions.FloatFromAxis(Input.GetKey(KeyCode.W), Input.GetKey(KeyCode.S))));
    }
    public void ToggleNoclip()
    {
        noclipEnabled = !noclipEnabled;
        CameraHandler.instance.walkingAnimEnable = !noclipEnabled;
        uiMessage.instance.New(str_noclip + (noclipEnabled ? str_enabled : str_disabled), str_uiDebug); 
    }
    public void ToggleGod()
    {
        godEnabled = !godEnabled;
        uiMessage.instance.New(str_god + (godEnabled ? str_enabled : str_disabled), str_uiDebug);
    }
    public void ToggleFPS()
    {
        uiFPSEnabled = !uiFPSEnabled;
        if (!uiFPSEnabled && debugMode) { return; }
        uiFPS.SetActive(uiFPSEnabled);
    }
    public void TorchSwayDebug()
    {
        torchCrosshairTransform.position = CameraHandler.mainCamera.WorldToScreenPoint(
            TorchHandler.instance.torch.transform.position + (TorchHandler.instance.torch.transform.TransformDirection(Vector3.forward) * TorchHandler.instance.torch.range));
            //+ new Vector3((Screen.width - Display.instance.resolution.x) / 2, 0, 0)
            // width of one black bar, width of window res - display res / 2
    }
}
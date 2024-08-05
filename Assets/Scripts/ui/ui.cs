using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ui : MonoBehaviour
{
    public static ui instance { get; private set; }
    public Button initialButtonToFocus;
    public uiSettings settings { get; private set; }
    public uiGameOver gameOver { get; private set; }
    //public List<uiObjective> 
    //    uiObjectives;
    public bool 
        uiFadeToBlack;
    //[SerializeField] GameObject 
    //    uiObjectivePrefab,
    //    uiObjectivesParent;
    [SerializeField] TextMeshProUGUI 
        uiSpeedometer,
        uiMazeSize,
        uiPlayerGridPosition,
        uiPlayerLives;
    [SerializeField] Image 
        uiFade;
    [SerializeField] float 
        uiFadeInSpeed,
        uiFadeOutSpeed;
    public float uiFadeAlpha;
    const string
        uiLevelNumText = "Level ",
        uiSectionNumText = "Section ",
        uiLevelNumDashText = "-";
    void Awake()
    {
        instance = this;
        settings = GetComponentInChildren<uiSettings>();
        gameOver = GetComponentInChildren<uiGameOver>();
        settings.gameObject.SetActive(false);
        gameOver.gameObject.SetActive(false);
        //LevelLoader.instance.levelLoaded.AddListener(Refresh);
    }
    void Start()
    {
        initialButtonToFocus.Select();
        //InvokeRepeating(nameof(uiObjectivesRefresh), 1f, 1f);
    }
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape)) { settings.gameObject.SetActive(!settings.gameObject.activeSelf); }
        uiFadeUpdate();
        uiSpeedometerUpdate();
        uiMazeSizeUpdate();
        uiPlayerGridPositionUpdate();
        uiPlayerLives.text = (Game.instance.playerHits - Game.instance.playerHitsTaken).ToString();
    }
    public void StartGame()
    {

    }
    public void Quit()
    {
        uiDebugConsole.instance.InternalCommandCall("quit");
    }
    /// <summary>
    /// Updates the color of the ui fade element on screen used to hide the screen, uiFadeToBlack controls the direction of the fade
    /// </summary>
    void uiFadeUpdate()
    {
        uiFade.color = new Color(0, 0, 0, System.Math.Clamp(uiFade.color.a + (uiFadeToBlack ? Time.deltaTime * uiFadeInSpeed : -Time.deltaTime * uiFadeOutSpeed), 0f, 1f));
        uiFadeAlpha = uiFade.color.a;
    }
    public void uiFadeAlphaSet(float alpha) => uiFade.color = new Color(0, 0, 0, alpha);
    void uiSpeedometerUpdate()
    {
        uiSpeedometer.text = (Player.instance.playerSpeed > Player.instance.movementSpeedReadOnly ? "<color=green>" : "") +
            "<line-height=40%>" + Player.instance.playerSpeed + "\n" + "<size=50%>m/s";
    }
    void uiMazeSizeUpdate()
    {
        uiMazeSize.text = WorldGen.instance.worldSizeX + "x" + WorldGen.instance.worldSizeZ;
    }
    void uiPlayerGridPositionUpdate()
    {
        //uiPlayerGridPosition.text = "(" + Player.instance.gridPosition.x + "," + Player.instance.gridPosition.z + ")";
        uiPlayerGridPosition.text = Player.instance.gridIndex.ToStringBuilder().ToString();
    }
    public void ToggleSpeedometer()
    {
        uiSpeedometer.gameObject.SetActive(!uiSpeedometer.gameObject.activeSelf);
    }
}
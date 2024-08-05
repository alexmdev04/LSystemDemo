using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uiSettings : MonoBehaviour
{
    [SerializeField] TMP_InputField 
        sensitivityInputField,
        renderDistanceInputField,
        mazeSizeXInputField,
        mazeSizeZInputField;
    [SerializeField] Button 
        buttonToFocus;
    public GameObject 
        resetMessage;
    bool skipOnEnable = false;

    void Awake() 
    {
        mazeSizeXInputField.onValueChanged.AddListener(delegate { mazeSizeZInputField.text = mazeSizeXInputField.text; });
    }
    void OnEnable()
    {
        if (!skipOnEnable) { skipOnEnable = true; return; }
        if (Game.instance.paused) { gameObject.SetActive(false); }
        Game.instance.Pause(true);
        buttonToFocus.Select();
        sensitivityInputField.text = Player.instance.lookSensitivity.y.ToString();
        renderDistanceInputField.text = ChunkRenderer.instance.renderDistance.ToString();
        mazeSizeXInputField.text = WorldGen.instance.worldSizeX.ToString();
        //mazeSizeZInputField.text = WorldGen.instance.worldSizeZ.ToString();
    }
    void OnDisable()
    {
        Game.instance.Pause(false);
    }
    public void Quit()
    {
        uiDebugConsole.instance.InternalCommandCall("exit");
    }
    public void Resume()
    {
        gameObject.SetActive(false);
    }
    public void Reset()
    {
        resetMessage.SetActive(false);
        uiDebugConsole.instance.InternalCommandCall("reset");
        Resume();
    }
    public void SetSensitivity()
    {
        if (float.TryParse(sensitivityInputField.text, out float sensitivity))
        {
            sensitivity = System.Math.Clamp(sensitivity, 0.0001f, 100000f);
            Player.instance.lookSensitivity = new(sensitivity, sensitivity);
        }
    }
    public void SetRenderDistance()
    {
        if (int.TryParse(renderDistanceInputField.text, out int renderDistance))
        {
            renderDistance = System.Math.Clamp(renderDistance, 1, 25);
            ChunkRenderer.instance.SetRenderDistance(renderDistance);
        }
    }
    public void SetMazeSizeX()
    {
        if (int.TryParse(mazeSizeXInputField.text, out int mazeSizeX))
        {
            WorldGen.instance.worldSizeXNew = mazeSizeX;
            resetMessage.SetActive(true);
        }
    }
    public void SetMazeSizeZ()
    {
        if (int.TryParse(mazeSizeZInputField.text, out int mazeSizeZ))
        {
            mazeSizeZ = System.Math.Clamp(mazeSizeZ, 2, 10000);
            WorldGen.instance.worldSizeZNew = mazeSizeZ;
            resetMessage.SetActive(true);
        }
    }
}

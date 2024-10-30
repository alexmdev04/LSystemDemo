using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uiSettings : MonoBehaviour
{
    [SerializeField] TMP_InputField 
        sensitivityInputField,
        renderDistanceInputField;
    [SerializeField] Button 
        buttonToFocus;
    bool skipOnEnable = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { Resume(); }
    }

    void OnEnable()
    {
        if (!skipOnEnable) { skipOnEnable = true; return; }
        if (Game.instance.paused) { gameObject.SetActive(false); }
        Game.instance.Pause(true);
        buttonToFocus.Select();
        sensitivityInputField.text = Player.instance.lookSensitivity.y.ToString();
        renderDistanceInputField.text = ChunkRenderer.instance.renderDistance.ToString();
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
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uiGameOver : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI 
        papersCollectedText,
        timeAliveText,
        mazeSizeText,
        titleText;
    [SerializeField] Button buttonToFocus;
    bool skippedOnEnable = false;

    void OnEnable()
    {
        if (!skippedOnEnable) { skippedOnEnable = true; return; }
        Game.instance.Pause(true);
        buttonToFocus.Select();
        titleText.text = WorldGen.instance.won ? "You Win!" : "Game Over!";
        papersCollectedText.text = "Papers Collected: " + Game.instance.papersCollected + "/8";
        timeAliveText.text = "Time Alive: " + TimeSpan.FromSeconds(Game.instance.clock).TotalSeconds.ToString() + "s";
        mazeSizeText.text = "Maze Size: " + WorldGen.instance.worldSizeX + " x " + WorldGen.instance.worldSizeZ;
    }
    void OnDisable()
    {
        Game.instance.Pause(false);
    }
    public void Quit()
    {
        uiDebugConsole.instance.InternalCommandCall("exit");
    }
    public void Reset()
    {
        uiDebugConsole.instance.InternalCommandCall("reset");
        Game.instance.clock = 0;
        gameObject.SetActive(false);
    }
}

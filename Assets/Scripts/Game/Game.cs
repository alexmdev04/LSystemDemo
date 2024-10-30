using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
//using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
//using UnityEngine.ResourceManagement.AsyncOperations;
//using UnityEngine.ResourceManagement.ResourceProviders;

public class Game : MonoBehaviour
{
    public enum Directions
    {
        north,
        northEast,
        east,
        southEast,
        south,
        southWest,
        west,
        northWest,
    }
    public LayerMask defaultLayer;
    public static Game instance { get; private set; }
    public System.Random random = new();
    public double clock;
    public bool inGame, paused;
    public int 
        papersCollected = 0,
        paperCount = 8,
        playerHits = 2,
        playerHitsTaken = 0;
    public float
        damageVignetteIntensityMin = 0.25f,
        damageVignetteIntensityMax = 1f,
        filmGrainIntensityDistance = 20f;
    public Volume globalVolume;
    public FilmGrain filmGrain;
    public Vignette vignette;
    void Awake()
    {
        instance = this;
        globalVolume = GetComponent<Volume>();
        globalVolume.profile.TryGet(out filmGrain);
        globalVolume.profile.TryGet(out vignette);
        //Addressables.InitializeAsync();
    #if !UNITY_EDITOR
        UnityEngine.SceneManagement.SceneManager.LoadScene("ui", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        UnityEngine.SceneManagement.SceneManager.LoadScene("level0", UnityEngine.SceneManagement.LoadSceneMode.Additive);
    #endif
    }
    void Start()
    {
        Pause(true);
    }
    void Update()
    {
        
    }
    public void Reset()
    {
        vignette.color.value = Color.black;
        vignette.intensity.value = 0.25f;
        papersCollected = 0;
        playerHitsTaken = 0;
    }
    public void Pause(bool state = false)
    {
        paused = state;
        Time.timeScale = state ? 0 : 1;
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
        InputHandler.instance.SetActive(!state);
    }
    public void AttackPlayer()
    {
        if (playerHitsTaken < playerHits - 1)
        {
            playerHitsTaken++;
            vignette.color.value = Color.red;
            vignette.intensity.value = Mathf.Lerp(0f, 1f, (float) playerHitsTaken / (float) playerHits);
        }
        else // GAME OVER
        {
            ui.instance.gameOver.gameObject.SetActive(true);
        }
    }
}
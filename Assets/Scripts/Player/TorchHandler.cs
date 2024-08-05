using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchHandler : MonoBehaviour
{
    public static TorchHandler instance { get; private set; }
    public Light torch { get; private set; }
    [SerializeField] bool 
        torchActive,
        torchIntensityScalerActive;
    [SerializeField] float
        intensityMax = 300f,
        intensityMin = 1f,
        intensityLerpSpeed = 10f,
        intensityDistance = 50f,
        lookSwayLerpSpeed = 25f;
    [SerializeField] GameObject
        intensityOrigin;
    [SerializeField] bool 
        idleSwayEnable = true;
    [SerializeField] float
        idleSwaySpeedMultiplier = 1.25f,
        idleSwayDistanceMultiplier = 0.05f;

    RaycastHit torchHit;
    void Awake()
    {
        instance = this;
        torch = GetComponent<Light>();
    }
    void Update()
    {
        torch.transform.position = CameraHandler.mainCamera.transform.position;
        TorchIntensityScaler();
        TorchRotation();
    }
    void TorchRotation()
    {
        // torch sway
        if (!torchActive)
        {
            torch.transform.localRotation = Player.instance.cameraTransformReadOnly.localRotation;
            return;
        }
        torch.transform.localRotation = Quaternion.Euler(
            Quaternion.Lerp(torch.transform.localRotation, Quaternion.Euler(Player.instance.cameraTransformReadOnly.localEulerAngles), Time.deltaTime * lookSwayLerpSpeed).eulerAngles);
        // idle sway
        if (idleSwayEnable)
        {
            float gameClockScaled = (float)Game.instance.clock * idleSwaySpeedMultiplier;
            torch.transform.localEulerAngles += (Time.timeScale == 0) ? 
            Vector3.zero :
            new Vector3(MathF.Cos(gameClockScaled), MathF.Sin(gameClockScaled), 0f) * idleSwayDistanceMultiplier;
        }
    }
    public void TorchSetActive(bool state) => torchActive = state;
    public void ToggleTorch()
    {
        //if (torchActive) { torch.gameObject.SetActive(!torch.gameObject.activeSelf); }
        if (torchActive) { torch.enabled = !torch.enabled; }
    }
    /// <summary>
    /// Scales the intensity of the torch depending on how close a wall is to the player
    /// </summary>
    void TorchIntensityScaler()
    {
        if (torch.gameObject.activeSelf && torchIntensityScalerActive)
        {
            torch.intensity = Mathf.Lerp(torch.intensity,
                Physics.Raycast(intensityOrigin.transform.position, intensityOrigin.transform.forward, out torchHit, intensityDistance) ?
                Mathf.Lerp(intensityMin, intensityMax, Vector3.Distance(intensityOrigin.transform.position, torchHit.point) / intensityDistance)
                : intensityMax
                , intensityLerpSpeed * Time.deltaTime);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactable : MonoBehaviour
{
    public Key key = Key.F;
    public string prompt = "Interact";
    public Interactions.interactType interactType;
    public float interactDistanceOverride;
    public GameObject interactObjectOverride;
    Action<GameObject> action;
    void Awake()
    {
        Interactions.TryGetAction(interactType, out action);
        if (interactObjectOverride == null) { interactObjectOverride = gameObject; }
    }
    public void Interact()
    {
        action(interactObjectOverride);
    }
#if UNITY_EDITOR
    [Space] public bool refresh;
    void Update()
    {
        if (refresh) { Awake(); refresh = false; }
    }
#endif
}
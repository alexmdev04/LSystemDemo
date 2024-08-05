using System;
using System.Reflection;
using UnityEngine;

public static class Interactions
{
    public enum interactType
    {
        Default,
        ToggleMenu,
        ToggleDoor,
        ScaleUp,
        ScaleDown
    }
    public static bool TryGetAction(interactType interactType, out Action<GameObject> action)
    {
        try
        {
            action = (Action<GameObject>)Delegate.CreateDelegate(typeof(Action<GameObject>), typeof(Interactions).GetMethod(interactType.ToString()));
            return true;
        }
        catch (Exception)
        {
            action = null;
            return false;
        }
    }
    public static bool TryGetAction(string interactType, out Action<GameObject> action)
    {
        try
        {
            action = (Action<GameObject>)Delegate.CreateDelegate(typeof(Action<GameObject>), typeof(Interactions).GetMethod(interactType));
            return true;
        }
        catch (Exception)
        {
            action = null;
            return false;
        }
    }
    public static void Default(GameObject interactableObject)
    {
        uiMessage.instance.New("boing");
    }
    public static void ping(GameObject interactableObject)
    {
        uiMessage.instance.New("pong");
    }
    public static void ScaleUp(GameObject interactableObject)
    {
        interactableObject.transform.localScale += Vector3.one * 0.1f;
    }
    public static void ScaleDown(GameObject interactableObject)
    {
        interactableObject.transform.localScale -= Vector3.one * 0.1f;
    }
}

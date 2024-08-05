using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class uiInteract : MonoBehaviour
{
    RectTransform rectTransform;

    [SerializeField] TextMeshProUGUI interactPrompt;
    RectTransform interactPromptTransform;

    [SerializeField] GameObject buttonIcon;
    RectTransform buttonIconTransform;

    [SerializeField] TextMeshProUGUI buttonPrompt;

    [SerializeField] LayerMask interactLayer;

    [SerializeField] float 
        height,
        iconTextGap = 20f,
        interactDistance = 5f;

    float width;

    Ray interactRay;

    RaycastHit interactHit;

    bool interactFound;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        interactPromptTransform = interactPrompt.GetComponent<RectTransform>();
        buttonIconTransform = buttonIcon.GetComponent<RectTransform>();
    }
    void Update()
    {
        interactRay = new Ray(Player.instance.cameraTransformReadOnly.position, Player.instance.cameraTransformReadOnly.TransformDirection(Vector3.forward));
        interactPromptScaling();
        interactCast();
        buttonIcon.SetActive(interactCheck());
    }
    void interactPromptScaling()
    {
        interactPromptTransform.anchoredPosition = new Vector2(iconTextGap, 0);
        width = buttonIconTransform.sizeDelta.x + iconTextGap + Vector2.Distance(interactPromptTransform.anchoredPosition, interactPrompt.GetPositionOfLastLetter());
        rectTransform.sizeDelta = new Vector2(width, height);
        buttonIconTransform.sizeDelta = new Vector2 (height, height);
        interactPromptTransform.sizeDelta = new Vector2(Screen.width, height - 15f);
    }
    void interactCast()
    {
        Debug.DrawLine(Player.instance.cameraTransformReadOnly.position, Player.instance.cameraTransformReadOnly.position + Player.instance.cameraTransformReadOnly.TransformDirection(Vector3.forward), Color.green);
        interactFound = Physics.Raycast(interactRay, out interactHit, interactDistance, (int)interactLayer);        
    }
    bool interactCheck()
    {
        if (!interactFound) { return false; }

        Interactable interactable = interactHit.collider.GetComponent<Interactable>();

        if (interactable.interactDistanceOverride != 0f)
        {
            if (Vector3.Distance(interactRay.origin, interactHit.point) > interactable.interactDistanceOverride) { return false; }
        }

        buttonPrompt.text = interactable.key.ToStringTranslate();
        interactPrompt.text = interactable.prompt;

        if (Keyboard.current[interactable.key].wasPressedThisFrame)
        {
            interactable.Interact();
        }

        return true;
    }
}
  
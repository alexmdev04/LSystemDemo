using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class InputHandler : MonoBehaviour
{ 
    // this script "binds" input maps to actions in a centralised location / creates publically available values that can be accessed but not edited

    public static InputHandler instance { get; private set; }
    public PlayerInput input { get; private set; }
    public bool active { get; private set; }

    void Awake()
    {
        instance = this;
        input = new();
        input.Player.Enable();
    }
    void Update()
    {
        if (!active | uiDebugConsole.instance.gameObject.activeSelf) 
        {
            Player.instance.mouseDelta = Vector2.zero;
            Player.instance.movementDirection = Vector3.zero;
            return;
        }

        // mouse vector
        Player.instance.mouseDelta = input.Player.Look.ReadValue<Vector2>() * Player.instance.mouseDeltaMultiplier;

        // movement vector

        //Vector3 movementDirectionKBM = input.Player.Move.bindings[0]..ReadValue<Vector3>();
        //Vector2 movementDirectionStick = input.Player.Move.ReadValue<Vector2>();
        Vector2 movementDirectionVector2 = input.Player.Move.ReadValue<Vector2>();
        Player.instance.movementDirection = new(movementDirectionVector2.x, 0, movementDirectionVector2.y);

        //Player.instance.movementDirection = input.Player.Move.ReadValue<Vector3>();
        
        // sprint
        Player.instance.SetSprint(input.Player.Sprint.IsPressed());

        // crouch
        Player.instance.SetCrouch(input.Player.Crouch.IsPressed());

        // jump
        if (input.Player.Jump.WasPressedThisFrame()) { Player.instance.Jump(); }

        // reset
        if (input.Player.Reset.WasPressedThisFrame()) { uiDebugConsole.instance.InternalCommandCall("reset"); }

        // toggle torch
        if (input.Player.Torch.WasPressedThisFrame()) { TorchHandler.instance.ToggleTorch(); }

        // toggle pause
        if (input.Player.Pause.WasPressedThisFrame()) { ui.instance.settings.gameObject.SetActive(!ui.instance.settings.gameObject.activeSelf); }
        if (input.UI.Unpause.WasPressedThisFrame()) { ui.instance.settings.gameObject.SetActive(!ui.instance.settings.gameObject.activeSelf); }
	}
    public void SetActive(bool state)
    {
        active = state;
    }
}
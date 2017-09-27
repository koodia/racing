using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inprovements: eInputAction list to string
/// </summary>
public class UnityInputManager : InputManager
{
    [SerializeField]
    private string playerAxisPrefix = "";
    [SerializeField]
    private int maxNumberOfPlayers = 1;

    //Each action is mapped to corresponding Unity axis
    [Header("Handling")]
    [SerializeField]
    private string throttleAxis = "Throttle";
    [SerializeField]
    private string brakeOrReverseAxis = "BrakeOrReverse";
    [SerializeField]
    private string handbrakeAxis = "Handbrake";
    [SerializeField]
    private string horizontalAxis = "Horizontal";
    [SerializeField]
    private string verticalAxis = "Vertical";
    [SerializeField]
    private string gearUpAxis = "GearUp";
    [SerializeField]
    private string gearDownAxis = "GearDown";
    [SerializeField]
    private string clutchAxis = "Clutch";
    [SerializeField]
    private string pitchForwardAxis = "PitchForward";
    [SerializeField]
    private string pitchBackwardAxis = "PitchBackward";
    [SerializeField]
    private string yawRightAxis = "YawRight";
    [SerializeField]
    private string yawLeftAxis = "YawLeft";

    [Header("Other controls")]
    [SerializeField]
    private string mirrorAxis = "Mirror";  //look behind
    [SerializeField]
    private string resetAxis = "Reset"; //debugging thingi
    [SerializeField]
    private string engineStartAxis = "EngineStart";
    [SerializeField]
    private string nitroAxis = "Nitro";
    [SerializeField]
    private string cameraAngleAxis = "CameraAngle";
    [SerializeField]
    private string cycleAbilityNextAxis = "CycleAbilityNext";
    [SerializeField]
    private string cycleAbilityPreviousAxis = "CycleAbilityPrevious";
    [SerializeField]
    private string useOrFireAxis = "UseOrFire";
    [SerializeField]
    private string pauseAxis = "Pause";

    [Header("Menu Navigation")]
    [SerializeField]
    private string submitAxis = "Submit";
    [SerializeField]
    private string cancelAxis = "Cancel";
    [SerializeField]
    private string rightAxis = "Right";
    [SerializeField]
    private string leftAxis = "Left";

    private Dictionary<int, string>[] actions;


    protected override void Awake()
    {
        base.Awake();
        //if (InputManager.instance != null)
        //{
        //    return;
        //}

        instance = this;
        actions = new Dictionary<int, string>[maxNumberOfPlayers];

        for (int i = 0; i < maxNumberOfPlayers; i++)
        {
            Dictionary<int, string> playerActions = new Dictionary<int, string>();
            actions[i] = playerActions;
            string prefix = !string.IsNullOrEmpty(this.playerAxisPrefix) ? this.playerAxisPrefix + 1 : string.Empty;
            AddAction(InputAction.Throttle, prefix + throttleAxis, playerActions);
            AddAction(InputAction.BrakeOrReverse, prefix + brakeOrReverseAxis, playerActions);
            AddAction(InputAction.Handbrake, prefix + handbrakeAxis, playerActions);
            AddAction(InputAction.Horizontal, prefix + horizontalAxis, playerActions);
            AddAction(InputAction.Vertical, prefix + verticalAxis, playerActions);
            AddAction(InputAction.GearUp, prefix + gearUpAxis, playerActions);
            AddAction(InputAction.GearDown, prefix + gearDownAxis, playerActions);
            AddAction(InputAction.Clutch, prefix + clutchAxis, playerActions);
            AddAction(InputAction.PitchForward, prefix + pitchForwardAxis, playerActions);
            AddAction(InputAction.PitchBackward, prefix + pitchBackwardAxis, playerActions);
            AddAction(InputAction.YawRight, prefix + yawRightAxis, playerActions);
            AddAction(InputAction.YawLeft, prefix + yawLeftAxis, playerActions);

            AddAction(InputAction.Mirror, prefix + mirrorAxis, playerActions);
            AddAction(InputAction.Reset, prefix + resetAxis, playerActions);
            AddAction(InputAction.EngineStart, prefix + engineStartAxis, playerActions);
            AddAction(InputAction.Nitro, prefix + nitroAxis, playerActions);
            AddAction(InputAction.CameraAngle, prefix + cameraAngleAxis, playerActions);
            AddAction(InputAction.UseOrFire, prefix + useOrFireAxis, playerActions);
            AddAction(InputAction.Pause, prefix + pauseAxis, playerActions);

            AddAction(InputAction.Submit, prefix + submitAxis, playerActions);
            AddAction(InputAction.Cancel, prefix + cancelAxis, playerActions);
            AddAction(InputAction.Right, prefix + rightAxis, playerActions);
            AddAction(InputAction.Left, prefix + leftAxis, playerActions);
            AddAction(InputAction.CycleAbilityNext, prefix + cycleAbilityNextAxis, playerActions);
            AddAction(InputAction.CycleAbilityPrevious, prefix + cycleAbilityPreviousAxis, playerActions);
        }
}

    private static void AddAction(InputAction action, string actionName, Dictionary<int,string> actions)
    {
        if (string.IsNullOrEmpty(actionName)){
            return;
        }
        actions.Add((int)action,actionName);
    }

    public override bool GetButton(int playerId, InputAction action)
    {
        return Input.GetButton(actions[playerId][(int)action]);
    }

    public override bool GetButtonDown(int playerId, InputAction action)
    {
        return Input.GetButtonDown(actions[playerId][(int)action]);
    }

    public override bool GetButtonUp(int playerId, InputAction action)
    {
        return Input.GetButtonUp(actions[playerId][(int)action]);
    }

    public override float GetAxis(int playerId, InputAction action)
    {
        return Input.GetAxis(actions[playerId][(int)action]);
    }
}

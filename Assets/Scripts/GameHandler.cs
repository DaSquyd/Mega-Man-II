using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour {
    public struct VirtualController {
        public VirtualButton Jump;
        public VirtualButton Shoot;
        public VirtualButton Left;
        public VirtualButton Right;
        public VirtualButton Up;
        public VirtualButton Down;
        public VirtualButton Start;
        public VirtualButton Select;
        public VirtualButton FrameStep;
    }

    public struct VirtualButton {
        public bool Value;
        public bool Press;
        public int IntValue {
            get {
                return Value ? 1 : 0;
            }
        }
    }

    public static VirtualController vc = new VirtualController();

    public static GameHandler handler;

    public static bool FrameStepping = false;
    public static bool Step {
        get; private set;
    }

    public static Player player;
    public static Camera camera;
    public static Image healthBar;

    public Player m_player;
    public Camera m_camera;
    public Image m_healthBar;

    private void Awake() {
        handler = this;

        player = m_player;
        camera = m_camera;
        healthBar = m_healthBar;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    private void FixedUpdate() {
        Controls();

        Step = false;

        if (!FrameStepping)
            return;
        if (vc.FrameStep.Press) {
            Step = true;
        }
    }

    private void Controls() {

        SetControlOutput(ref vc.Jump, KeyCode.X, KeyCode.K, "Jump");
        SetControlOutput(ref vc.Shoot, KeyCode.Z, KeyCode.J, "Shoot");
        SetControlOutput(ref vc.Left, KeyCode.LeftArrow, KeyCode.A, "Horizontal", true);
        SetControlOutput(ref vc.Right, KeyCode.RightArrow, KeyCode.D, "Horizontal", false);
        SetControlOutput(ref vc.Up, KeyCode.UpArrow, KeyCode.W, "Vertical", true);
        SetControlOutput(ref vc.Down, KeyCode.DownArrow, KeyCode.S, "Vertical", false);
        SetControlOutput(ref vc.Start, KeyCode.Return, KeyCode.Return, "Start");
        SetControlOutput(ref vc.Select, KeyCode.RightShift, KeyCode.RightShift, "Select");
        //SetControlOutput(ref vc.FrameStep, KeyCode.Slash, "Step");
    }

    private void SetControlOutput(ref VirtualButton button, KeyCode key1, KeyCode key2, string axis, bool negative = false) {

        string newAxis = "None";

        if (Input.GetJoystickNames().Length > 0) {
#if UNITY_EDITOR
            if (Input.GetJoystickNames()[1].Contains("Xbox")) {
#else
        if (Input.GetJoystickNames()[0].Contains("Xbox")) {
#endif
                newAxis = axis + "_Xbox";
            } else {
                newAxis = axis + "_PS";
            }
        }

        if (Input.GetKey(key1) || Input.GetKey(key2) || Input.GetAxisRaw(newAxis) * (negative ? -1f : 1f) > 0f) {
            if (button.Value) {
                button.Press = false;
            } else {
                button.Value = true;
                button.Press = true;
            }
        } else {
            button.Value = false;
            button.Press = false;
        }
    }

    private void LateUpdate() {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Return)) {
            FrameStepping = !FrameStepping;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour {
    public struct VirtualController {
        public VirtualButton A;
        public VirtualButton B;
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

    private void Start() {
        handler = this;
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
        SetControlOutput(ref vc.A, KeyCode.X, "Jump");
        SetControlOutput(ref vc.B, KeyCode.Z, "Shoot");
        SetControlOutput(ref vc.Left, KeyCode.LeftArrow, "Horizontal", true);
        SetControlOutput(ref vc.Right, KeyCode.RightArrow, "Horizontal", false);
        SetControlOutput(ref vc.Up, KeyCode.UpArrow, "Vertical", true);
        SetControlOutput(ref vc.Down, KeyCode.DownArrow, "Vertical", false);
        SetControlOutput(ref vc.Start, KeyCode.Return, "Submit");
        SetControlOutput(ref vc.Select, KeyCode.RightShift, "Cancel");
        SetControlOutput(ref vc.FrameStep, KeyCode.Slash, "Step");
    }

    private void SetControlOutput(ref VirtualButton button, KeyCode key, string axis, bool negative = false) {
        if (Input.GetKey(key) || Input.GetAxisRaw(axis) * (negative ? -1f : 1f) > 0f) {
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

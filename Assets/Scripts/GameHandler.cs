using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public static GameHandler handler;

    public static bool FrameStepping = false;
    public static bool Step { get; private set; }

    private void Start()
    {
        handler = this;
    }

    private void FixedUpdate()
    {
        Step = false;

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Return))
        {
            FrameStepping = !FrameStepping;
        }

        if (!FrameStepping)
            return;
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Step = true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public float height;
    public float leftBound;
    public float rightBound;

    private void FixedUpdate() {
        if (GameHandler.player == null)
            return;

        transform.position = new Vector3(Mathf.Clamp(GameHandler.player.transform.position.x, leftBound, rightBound), height, -10f);
    }
}

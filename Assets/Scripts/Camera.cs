using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour {

    public Player player;
    public float height;
    public float leftBound;
    public float rightBound;

    private void FixedUpdate() {
        transform.position = new Vector3(Mathf.Clamp(player.transform.position.x, leftBound, rightBound), height, -10f);
    }
}

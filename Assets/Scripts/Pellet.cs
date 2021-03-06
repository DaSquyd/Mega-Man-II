﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pellet : MonoBehaviour {
    private void FixedUpdate() {
        transform.Translate(16f / 60f, 0f, 0f);

        if (Mathf.Abs(transform.position.x - GameHandler.camera.transform.position.x) > 8) {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.tag != "Enemy")
            return;

        GameHandler.PlayAudio(GameHandler.enemyDieAudio);

        Instantiate(collision.GetComponent<Telly>().particle, collision.transform.position + new Vector3(0f, 0f, -5f), Quaternion.Euler(90f, 0f, 0f));

        Destroy(collision.gameObject);
        Destroy(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Telly : MonoBehaviour {

    public float Speed;
    public AudioSource dieAudio;

    Camera camera;
    Player player;

    Vector2 old;
    Vector2 target;
    int count;

    private void Start() {
        camera = GameHandler.camera;
        player = GameHandler.player;

        dieAudio = GetComponent<AudioSource>();

        old = transform.position;
        target = GetAngle();
    }

    private void FixedUpdate() {
        if (player == null)
            return;

        if (count < 30) {
            transform.position = new Vector2(Mathf.Lerp(old.x, target.x, count / 30f), Mathf.Lerp(old.y, target.y, count / 30f));

            count++;
        } else {
            old = transform.position;
            target = GetAngle();
            count = 0;
        }

        if (Mathf.Abs(transform.position.x - camera.transform.position.x) > 8) {
            EnemySpawner.Despawned = true;
            Destroy(gameObject);
        }
    }

    Vector2 GetAngle() {
        if (player == null)
            return new Vector2();

        float y = 0f;

        if (transform.position.y - (player.transform.position.y + 1000f) < 0)
            y = 2f;

        float angle = Mathf.RoundToInt(Mathf.Atan2((player.transform.position.y + y) - transform.position.y, player.transform.position.x - transform.position.x) * Mathf.Rad2Deg / 45f) * 45 * Mathf.Deg2Rad;

        float sin = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);

        return new Vector2(cos, sin) * Speed + (Vector2) transform.position;
    }
}

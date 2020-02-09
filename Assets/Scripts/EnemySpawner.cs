using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    public List<Transform> positions;

    List<Transform> onScreen = new List<Transform>();

    public GameObject enemy;

    List<GameObject> enemies = new List<GameObject>();

    int count;

    private void FixedUpdate() {
        onScreen = new List<Transform>();
        foreach (Transform t in positions) {
            if (Mathf.Abs(GameHandler.camera.transform.position.x - t.position.x) <= 8) {
                onScreen.Add(t);
            }
        }

        List<GameObject> removal = new List<GameObject>();
        foreach (GameObject go in enemies) {
            if (go == null)
                removal.Add(go);
        }
        foreach (GameObject go in removal) {
            enemies.Remove(go);
        }
        if (removal.Count > 0) {
            count = 0;
        }

        if (count < 180) {
            count++;
        }

        if (onScreen.Count == 0)
            return;

        if (count >= 90 && enemies.Count < 3) {
            GameObject go = Instantiate(enemy, onScreen[(int) Mathf.Floor(Random.value * onScreen.Count)].position, new Quaternion());

            enemies.Add(go);

            count = 0;
        }
    }
}

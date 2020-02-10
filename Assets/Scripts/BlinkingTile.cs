using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingTile : MonoBehaviour {

    public enum Direction {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        REFLECT
    }

    public bool isVisible;
    public int invisFrames;
    public int visFrames;
    public int offset;

    public Direction right;
    public Direction left;
    public Direction down;
    public Direction up;

    BoxCollider2D collider;
    SpriteRenderer spriteRenderer;
    Sprite sprite;
    int count;
    Player player;

    bool playing;
    float proximityTrigger = 8f;

    private void Start() {
        player = GameHandler.player;

        collider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        sprite = spriteRenderer.sprite;

        isVisible = false;
        collider.enabled = false;
        spriteRenderer.sprite = null;
    }

    private void FixedUpdate() {

        player = GameHandler.player;

        if (player == null)
            return;

        if (!playing && Mathf.Abs(transform.position.x - player.transform.position.x) <= proximityTrigger) {
            playing = true;

            count = offset;
        }

        if (playing && Mathf.Abs(transform.position.x - player.transform.position.x) > proximityTrigger) {
            isVisible = false;
            collider.enabled = false;
            spriteRenderer.sprite = null;
            playing = false;
        }

        if (!playing)
            return;

        if (isVisible) {
            if (count < visFrames) {

                count++;
            } else {
                isVisible = false;
                collider.enabled = false;
                spriteRenderer.sprite = null;
                count = 0;
            }
        } else {
            if (count < invisFrames) {

                count++;
            } else {
                isVisible = true;
                collider.enabled = true;
                spriteRenderer.sprite = sprite;
                count = 0;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision) {
        if (collision.gameObject != player.gameObject)
            return;


        float xPos = player.transform.position.x;
        float yPos = player.transform.position.y;

        int dir = (Mathf.FloorToInt(Mathf.Atan2(transform.position.x - xPos, transform.position.y - (yPos + 0.77f)) * Mathf.Rad2Deg) + 45 + 180) % 360;

        Direction send = Direction.UP;
        if (dir >= 0 && dir < 90) {
            if (up != Direction.REFLECT)
                send = up;
            else {
                if (xPos- transform.position.x > 0)
                    send = Direction.RIGHT;
                else
                    send = Direction.LEFT;
            }
        } else if (dir >= 90 && dir < 180) {
            if (right != Direction.REFLECT)
                send = right;
            else {
                if ((yPos + 0.77f) - transform.position.y > 0)
                    send = Direction.UP;
                else
                    send = Direction.DOWN;
            }
        } else if (dir >= 180 && dir < 270) {
            if (down != Direction.REFLECT)
                send = down;
            else {
                if (xPos - transform.position.x > 0)
                    send = Direction.RIGHT;
                else
                    send = Direction.LEFT;
            }
        } else if (dir >= 270 && dir < 360) {
            if (left != Direction.REFLECT)
                send = left;
            else {
                if ((yPos + 0.77f) - transform.position.y > 0)
                    send = Direction.UP;
                else
                    send = Direction.DOWN;
            }
        }

        switch (send) {
            case Direction.UP:
                player.transform.position = new Vector2(xPos, transform.position.y + 0.77f);
                break;
            case Direction.DOWN:
                player.transform.position = new Vector2(xPos, transform.position.y - 2.25f);
                break;
            case Direction.LEFT:
                player.transform.position = new Vector2(transform.position.x - 0.5001f - (7f / 16f), yPos);
                break;
            case Direction.RIGHT:
                player.transform.position = new Vector2(transform.position.x + 0.5001f + (7f / 16f), yPos);
                break;
            default:
                break;
        }
    }
}

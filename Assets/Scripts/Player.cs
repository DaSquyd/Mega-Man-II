using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    const float PIXELS = 32f;

    [System.Serializable()]
    public struct Corner {
        public Vector2 position;
        public DirectionH directionX;
        public DirectionV directionY;
    }

    public enum DirectionH {
        LEFT,
        RIGHT
    }

    public enum DirectionV {
        UP,
        DOWN
    }

    // Public Vars
    public Vector2 Velocity;
    public Vector2 Movement;

    public DirectionH Facing;

    public string GroundSpeed;
    public string AccelSpeed;
    public string decelSpeed;
    public string AirSpeed;
    public string JumpPower;
    public string MinJump;
    public string JumpCancelSpeed;
    public string MaxFallSpeed;
    public string ClimbSpeed;
    public string Gravity;

    public float DetectionBufferH;
    public float DetectionBufferV;

    public bool Stunned;
    public bool OnGround;
    public bool Jumping;
    public bool Shooting;

    // Private Vars
    int oldDir;
    int dirLock;
    public bool maxSpeed;
    int accelCount;
    public int decelCount;
    bool accelerating;
    public bool decelerating;

    int shootCount;

    SpriteRenderer spriteRenderer;
    Animator animator;
    BoxCollider2D col;

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();

        Debug.Log("Ground Speed: " + Convert(GroundSpeed, 1f, false));
        Debug.Log("Air Speed: " + Convert(AirSpeed, 1f, false));
        Debug.Log("Jump Power: " + Convert(JumpPower, 1f, true));
        Debug.Log("Min Jump: " + Convert(MinJump, 1f, false));
        Debug.Log("Jump Cancel Speed: " + Convert(JumpCancelSpeed, 1f, false));
        Debug.Log("Max Fall Speed: " + Convert(MaxFallSpeed, 1f, false));
        Debug.Log("Climb Speed: " + Convert(ClimbSpeed, 1f, false));
        Debug.Log("Gravity: " + Convert(Gravity, 1f, false));
    }


    private void FixedUpdate() {

        Controls();

        Move();


        if (GameHandler.FrameStepping && !GameHandler.Step)
            return;

        transform.Translate(Movement);
        if (!OnGround) {
            Velocity -= new Vector2(0f, Convert(Gravity));
        } else {
            Velocity = new Vector2(Velocity.x, -Convert(Gravity));
        }
    }

    private void Controls() {
        if (Stunned)
            return;

        float moveSpeed = Convert(OnGround ? GroundSpeed : AirSpeed);

        int dir = GameHandler.vc.Right.IntValue - GameHandler.vc.Left.IntValue;

        if (dir != 0 && dir != oldDir)
            accelerating = true;

        if (accelerating) {
            if (accelCount < 7) {
                moveSpeed = Convert(AccelSpeed);
                accelCount++;
            } else {
                accelerating = false;
                maxSpeed = true;
            }
        } else {
            accelCount = 0;
        }

        if (dir == 0 && oldDir != 0 && maxSpeed)
            decelerating = true;

        if (decelerating) {
            if (decelCount < 8) {
                dir = oldDir;
                moveSpeed = Convert(decelSpeed);
                decelCount++;
            } else {
                decelerating = false;
            }
        } else {
            decelCount = 0;
        }

        if (dir == 0) {
            accelerating = false;
            maxSpeed = false;
        }

        if (dir != 0)
            decelerating = false;

        oldDir = dir;

        Velocity = new Vector2(dir * moveSpeed, Velocity.y);

        if (OnGround && GameHandler.vc.A.Press) {
            Velocity = new Vector2(Velocity.x, Convert(JumpPower));
            Debug.Log(Convert(JumpPower));
            OnGround = false;
            Jumping = true;
        }

        if (Movement.y > Convert(MinJump) && !GameHandler.vc.A.Value && Jumping) {
            Debug.Log("Jump Cancelled");
            Movement = new Vector2(Movement.x, Convert(JumpCancelSpeed));
            Velocity = new Vector2(Velocity.x, Convert(JumpCancelSpeed));
            Jumping = false;
        }

        if (Movement.y < 0f)
            Jumping = false;


        if (GameHandler.vc.B.Press) {
            Shooting = true;
            shootCount = 0;
        }

        if (Shooting) {
            shootCount++;

            if (shootCount >= 40) {
                shootCount = 0;
                Shooting = false;
            }
        }

        if (Velocity.x > 0f)
            Facing = DirectionH.RIGHT;
        if (Velocity.x < 0f)
            Facing = DirectionH.LEFT;

        if (Facing == DirectionH.RIGHT)
            spriteRenderer.flipX = false;

        if (Facing == DirectionH.LEFT)
            spriteRenderer.flipX = true;


        animator.SetBool("Running", maxSpeed);
        animator.SetBool("OnGround", OnGround);
        animator.SetBool("Shooting", Shooting);
        //animator.SetBool("Climbing", Climbing);
    }

    void Move() {
        LayerMask mask = LayerMask.GetMask("Ground");

        OnGround = false;

        Movement = new Vector2();

        Vector2 position = transform.position;


        // UP
        Vector2 up = Vector2.up * Mathf.Max(Velocity.y, 0f);
        for (int i = 0; i <= 4; i++) {
            Vector2 rayPos = new Vector2((position.x - 7f / 16f) + (i / 4f * 14f / 16f), position.y + (24f / 16f) + DetectionBufferV);

            RaycastHit2D hit = Physics2D.Raycast(rayPos, up, up.magnitude, mask);

            if (hit.collider != null && hit.distance < up.magnitude) {
                up = new Vector2(0f, hit.distance);
#if UNITY_EDITOR
                if (!GameHandler.FrameStepping || (GameHandler.FrameStepping && GameHandler.Step))
#endif
                    Velocity = new Vector2(Velocity.x, -Convert(Gravity));
            }

            Debug.DrawRay(rayPos, up, Color.cyan, Time.fixedDeltaTime);
        }


        // DOWN
        Vector2 down = Vector2.up * Mathf.Min(Velocity.y, 0f);
        for (int i = 0; i <= 4; i++) {
            Vector2 rayPos = new Vector2((position.x - 7f / 16f) + (i / 4f * 14f / 16f), position.y - DetectionBufferV);

            RaycastHit2D hit = Physics2D.Raycast(rayPos, down, down.magnitude, mask);

            if (hit.collider != null && hit.distance < down.magnitude) {
                down = new Vector2(0f, -hit.distance);
                OnGround = true;
#if UNITY_EDITOR
                if (!GameHandler.FrameStepping || (GameHandler.FrameStepping && GameHandler.Step))
#endif
                    Velocity = new Vector2(Velocity.x, 0f);
            }

            Debug.DrawRay(rayPos, down, Color.cyan);
        }


        // RIGHT
        Vector2 right = Vector2.right * Mathf.Max(0f, Velocity.x);
        for (int i = 0; i <= 4; i++) {
            Vector2 rayPos = new Vector2(position.x + (7f / 16f) + DetectionBufferH, position.y + (i / 4f * 24f / 16f));

            RaycastHit2D hit = Physics2D.Raycast(rayPos, right, right.magnitude, mask);

            if (hit.collider != null && hit.distance < right.magnitude) {
                right = new Vector2(hit.distance, 0f);
#if UNITY_EDITOR
                if (!GameHandler.FrameStepping || (GameHandler.FrameStepping && GameHandler.Step))
#endif
                    Velocity = new Vector2(0f, Velocity.y);
            }

            Debug.DrawRay(rayPos, right, Color.cyan, Time.fixedDeltaTime);
        }
        //Debug.DrawRay(position + Vector2.right * (7f / 16f), down, Color.cyan, Time.fixedDeltaTime);


        // LEFT
        Vector2 left = Vector2.right * Mathf.Min(0f, Velocity.x);
        for (int i = 0; i <= 4; i++) {
            Vector2 rayPos = new Vector2(position.x - (7f / 16f) - DetectionBufferH, position.y + (i / 4f * 24f / 16f));

            RaycastHit2D hit = Physics2D.Raycast(rayPos, left, left.magnitude, mask);

            if (hit.collider != null && hit.distance < left.magnitude) {
                left = new Vector2(-hit.distance, 0f);
#if UNITY_EDITOR
                if (!GameHandler.FrameStepping || (GameHandler.FrameStepping && GameHandler.Step))
#endif
                    Velocity = new Vector2(0f, Velocity.y);
            }

            Debug.DrawRay(rayPos, left, Color.cyan, Time.fixedDeltaTime);
        }

        if (GameHandler.FrameStepping && !GameHandler.Step)
            return;

        Movement = up + down + right + left;
    }

    private float Convert(string hex, float multiply = 2f, bool divideByPixels = true) {
        string[] segments = hex.Split('.');

        float first = int.Parse(segments[0], System.Globalization.NumberStyles.HexNumber);
        float last = int.Parse(segments[1], System.Globalization.NumberStyles.HexNumber) / 256f;

        if (divideByPixels)
            return (first + last) / PIXELS * multiply;
        else
            return first + last * multiply;
    }
}

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

    public bool Stunned;
    public bool OnGround;
    public bool Jumping;

    public float floatHeight;
    public float liftForce;
    public float damping;

    public Vector2 shape;

    public List<Corner> corners;

    // Private Vars
    int oldDir;
    int dirLock;
    bool maxSpeed;
    int accelCount;
    int decelCount;

    BoxCollider2D col;

    private void Start() {
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

        if (GameHandler.FrameStepping && !GameHandler.Step)
            return;


        Move();


        transform.Translate(Movement);
        if (!OnGround) {
            Velocity -= new Vector2(0f, Convert(Gravity));
        } else {
            Velocity = new Vector2(Velocity.x, -Convert(Gravity));
        }
    }

    private void Controls()
    {
        if (Stunned)
            return;

        float moveSpeed = Convert(OnGround ? GroundSpeed : AirSpeed);

        int dir = GameHandler.vc.Right.IntValue - GameHandler.vc.Left.IntValue;

        if (Mathf.Abs(dir) == 1 && dir != oldDir)
        {
            if (accelCount < 7)
            {
                moveSpeed = Convert(AccelSpeed);
                accelCount++;
            }
        }
        else
        {
            accelCount = 0;
        }

        if (dir == 0 && Mathf.Abs(oldDir) == 1)
        {
            if (decelCount < 8)
            {
                dir = oldDir;
                moveSpeed = Convert(decelSpeed);
                decelCount++;
            }
        }
        else
        {
            decelCount = 0;
        }

        oldDir = dir;

        Velocity = new Vector2(dir * moveSpeed, Velocity.y);

        if (OnGround && GameHandler.vc.A.Press)
        {
            Velocity = new Vector2(Velocity.x, Convert(JumpPower));
            Debug.Log(Convert(JumpPower));
            OnGround = false;
            Jumping = true;
        }

        if (Movement.y > Convert(MinJump) && !GameHandler.vc.A.Value && Jumping)
        {
            Debug.Log("Jump Cancelled");
            Movement = new Vector2(Movement.x, Convert(JumpCancelSpeed));
            Velocity = new Vector2(Velocity.x, Convert(JumpCancelSpeed));
            Jumping = false;
        }

        if (Movement.y < 0f)
            Jumping = false;
    }

    void Move()
    {
        LayerMask mask = LayerMask.GetMask("Ground");

        OnGround = false;

        Movement = new Vector2();

        Vector2 position = transform.position;

        Vector2 up = Vector2.up * Mathf.Max(Velocity.y, 0f);

        for (int i = 0; i <= 4; i++)
        {
            Vector2 rayPos = new Vector2((position.x - 7f / 16f) + (i / 4f * 14f / 16f), position.y + 1f);

            RaycastHit2D hit = Physics2D.Raycast(rayPos, up, up.magnitude, mask);

            if (hit.collider != null && hit.distance < up.magnitude)
            {
                up = new Vector2(0f, hit.distance);
                Velocity = new Vector2(Velocity.x, -Convert(Gravity));
            }
        }


        Vector2 down = Vector2.up * Mathf.Min(Velocity.y, 0f);

        for (int i = 0; i <= 4; i++)
        {
            Vector2 rayPos = new Vector2((position.x - 7f / 16f) + (i / 4f * 14f / 16f), position.y - 1f);

            RaycastHit2D hit = Physics2D.Raycast(rayPos, down, down.magnitude, mask);

            if (hit.collider != null && hit.distance < down.magnitude)
            {
                down = new Vector2(0f, -hit.distance);
                Velocity = new Vector2(Velocity.x, 0f);
                OnGround = true;
            }
        }

        Movement = up + down;

        Debug.DrawRay(position + Vector2.down, down, Color.cyan, Time.fixedDeltaTime);
    }

    private void MoveOld() {
        LayerMask wallMask = LayerMask.GetMask("Wall");
        LayerMask groundMask = LayerMask.GetMask("Ground");

        Movement = new Vector2();
        OnGround = false;

        foreach (Corner c in corners) {
            Vector2 position = (Vector2) transform.position + c.position;
            Vector2 directionH = c.directionX == DirectionH.LEFT ? new Vector2(Mathf.Min(Velocity.x, 0f), 0f) : new Vector2(Mathf.Max(Velocity.x, 0f), 0f);
            Vector2 directionV = c.directionY == DirectionV.DOWN ? new Vector2(0f, Mathf.Min(Velocity.y, 0f)) : new Vector2(0f, Mathf.Max(Velocity.y, 0f));

            if (directionV.y < -Convert(MaxFallSpeed) / 2f)
                directionV = new Vector2(0f, -Convert(MaxFallSpeed) / 2f);

            RaycastHit2D hitH = Physics2D.Raycast(position, directionH.normalized, directionH.magnitude * 2f, wallMask);
            RaycastHit2D hitV = Physics2D.Raycast(position, directionV.normalized, directionV.magnitude * 2f, groundMask);

            if (hitH.collider != null) {
                directionH = hitH.normal * -hitH.distance;
            }
            if (hitV.collider != null) {
                directionV = hitV.normal * -hitV.distance / 2f;
            }

            Debug.Log(directionV);

            Movement += (directionH + directionV);

            if (Movement.y > Convert(MinJump) && !GameHandler.vc.A.Value && Jumping) {
                Debug.Log("Jump Cancelled");
                Movement = new Vector2(Movement.x, Convert(JumpCancelSpeed));
                Velocity = new Vector2(Velocity.x, Convert(JumpCancelSpeed));
                Jumping = false;
            }

            if (Movement.y < 0f)
                Jumping = false;

            if (hitV.collider != null && hitV.distance == 0f && Velocity.y <= 0f) {
                OnGround = true;
            }

            
            Debug.DrawRay(position, new Vector2(Movement.x, 0f), Color.cyan, Time.fixedDeltaTime, false);
            Debug.DrawRay(position, new Vector2(0f, Movement.y), Color.red, Time.fixedDeltaTime, false);
        }
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

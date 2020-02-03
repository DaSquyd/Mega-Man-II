using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [System.Serializable()]
    public struct Corner
    {
        public Vector2 position;
        public DirectionH directionX;
        public DirectionV directionY;
    }

    public enum DirectionH
    {
        LEFT,
        RIGHT
    }

    public enum DirectionV
    {
        UP,
        DOWN
    }

    // Public Vars
    public Vector2 Velocity;
    public Vector2 Movement;

    public float Speed;

    public bool Stunned;
    public bool OnGround;
    public bool Jumping;

    public float floatHeight;
    public float liftForce;
    public float damping;

    [Tooltip("In m/s^2")]
    public float gravityAcceleration;

    public Vector2 shape;

    public List<Corner> corners;

    // Private Vars
    BoxCollider2D col;

    private void Start()
    {
        col = GetComponent<BoxCollider2D>();
    }


    private void FixedUpdate()
    {
        if (GameHandler.FrameStepping && !GameHandler.Step)
            return;

        Controls();

        transform.Translate(Movement);
        if (!OnGround)
        {
            Velocity += new Vector2(0f, gravityAcceleration);
        }
        else
        {
            Velocity = new Vector2(Velocity.x, 0f);
        }

        Move();
    }

    //https://gamefaqs.gamespot.com/boards/950625-mega-man-9/46926500

    private void Controls()
    {
        if (Stunned)
            return;
        Velocity = new Vector2(Input.GetAxis("Horizontal") * Speed, Velocity.y);

        if (OnGround && Input.GetKeyDown(KeyCode.UpArrow))
        {
            Velocity = new Vector2(Velocity.x, 4.875f);
            OnGround = false;
            Jumping = true;
        }
    }

    private void Move()
    {
        LayerMask groundMask = LayerMask.GetMask("Ground");

        Movement = new Vector2();
        OnGround = false;

        foreach (Corner c in corners)
        {
            Vector2 position = (Vector2)transform.position + c.position;
            Vector2 directionH = (c.directionX == DirectionH.LEFT ? new Vector2(Mathf.Min(Velocity.x, 0f), 0f) : new Vector2(Mathf.Max(Velocity.x, 0f), 0f)) * Time.fixedDeltaTime;
            Vector2 directionV = (c.directionY == DirectionV.DOWN ? new Vector2(0f, Mathf.Min(Velocity.y, 0f)) : new Vector2(0f, Mathf.Max(Velocity.y, 0f))) * Time.fixedDeltaTime;

            RaycastHit2D hitH = Physics2D.Raycast(position, directionH.normalized, directionH.magnitude, groundMask);
            RaycastHit2D hitV = Physics2D.Raycast(position, directionV.normalized, directionV.magnitude, groundMask);

            if (hitH.collider != null)
            {
                directionH = hitH.normal * -hitH.distance;
            }
            if (hitV.collider != null)
            {
                directionV = hitV.normal * -hitV.distance;
            }

            Debug.Log("Normal: " + directionV);

            Debug.DrawRay(position, directionH, Color.cyan, Time.fixedDeltaTime, false);
            Debug.DrawRay(position, directionV, Color.cyan, Time.fixedDeltaTime, false);

            Movement += directionH + directionV / 2f;

            if (hitV.collider != null && hitV.distance == 0f && Velocity.y <= 0f)
                OnGround = true;
        }
    }
}

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
        LayerMask groundMask = LayerMask.GetMask("Ground");

        foreach (Corner c in corners)
        {
            Vector2 position = (Vector2)transform.position + c.position;
            Vector2 directionH = c.directionX == DirectionH.LEFT ? new Vector2(Mathf.Min(Velocity.x, 0f), 0f) : new Vector2(Mathf.Max(Velocity.x, 0f), 0f);
            Vector2 directionV = c.directionY == DirectionV.DOWN ? new Vector2(0f, Mathf.Min(Velocity.y, 0f)) : new Vector2(0f, Mathf.Max(Velocity.y, 0f));

            RaycastHit2D hitH = Physics2D.Raycast(position, directionH, directionH.x, groundMask);
            RaycastHit2D hitV = Physics2D.Raycast(position, directionV, directionV.y, groundMask);

            Debug.Log(directionV);
            Debug.Log(directionV.y);

            if (hitV.collider != null && hitV.distance != 0)
            {
                directionV = new Vector2(0f, -hitV.distance);
            }

            Debug.DrawRay(position, hitH.normal, Color.cyan, Time.fixedDeltaTime, false);
            Debug.DrawRay(position, hitV.normal, Color.cyan, Time.fixedDeltaTime, false);

            //Velocity = new Vector2(hitH.normal.x * hitH.distance, hitV.normal.y * hitV.distance);

            // GRAVITY
            //Velocity = new Vector2(Velocity.x, Velocity.y - (gravityAcceleration * Time.fixedDeltaTime));

        }

        
    }

    private void Update()
    {
        // Move
        if (Input.GetKeyDown(KeyCode.Space))
            transform.position = transform.position + (Vector3)Velocity;
    }
}

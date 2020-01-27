using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Public Vars
    public float floatHeight;
    public float liftForce;
    public float damping;

    [Tooltip("In m/s^2")]
    public float acceleration;

    public Vector2 shape;

    // Private Vars
    Rigidbody2D rb;
    BoxCollider2D col;

    Vector2 pointTL;
    Vector2 pointTR;
    Vector2 pointBL;
    Vector2 pointBR;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        pointTL = new Vector2(0f, shape.y);
        pointTR = new Vector2(shape.x, shape.y);
    }

    private void FixedUpdate()
    {
        LayerMask mask = LayerMask.GetMask("Ground");

        RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.down, floatHeight, mask);
        Debug.DrawRay(transform.position, Vector2.down, Color.cyan, Time.fixedDeltaTime, false);

        //float dist = 

        if (hit.collider != null)
        {
            float distance = Mathf.Abs(hit.point.y - transform.position.y);
            float heightError = floatHeight - distance;

            float force = liftForce * heightError - rb.velocity.y * damping;

            rb.AddForce(Vector2.up * force);
        }
    }
}

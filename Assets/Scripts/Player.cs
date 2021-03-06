﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public static Player player;


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

    public ParticleSystem dieParticle;

    public Pellet pelletPrefab;

    public List<Pellet> pellets = new List<Pellet>();

    // Private Vars
    int oldDir;
    int dirLock;
    public bool maxSpeed;
    int accelCount;
    public int decelCount;
    bool accelerating;
    public bool decelerating;

    public bool invulnerable;
    int invulnCount;

    int health = 10;

    int shootCount;

    string lastGroundTag;

    SpriteRenderer spriteRenderer;
    Animator animator;
    BoxCollider2D col;

    AudioSource hitAudio;
    AudioSource shootAudio;

    private void Start() {
        if (GameHandler.handler == null) {
            SceneManager.LoadScene(0);
        }

        player = this;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();

        GetComponents<AudioSource>();

        /*
        Debug.Log("Ground Speed: " + Convert(GroundSpeed, 1f, false));
        Debug.Log("Air Speed: " + Convert(AirSpeed, 1f, false));
        Debug.Log("Jump Power: " + Convert(JumpPower, 1f, true));
        Debug.Log("Min Jump: " + Convert(MinJump, 1f, false));
        Debug.Log("Jump Cancel Speed: " + Convert(JumpCancelSpeed, 1f, false));
        Debug.Log("Max Fall Speed: " + Convert(MaxFallSpeed, 1f, false));
        Debug.Log("Climb Speed: " + Convert(ClimbSpeed, 1f, false));
        Debug.Log("Gravity: " + Convert(Gravity, 1f, false));
        */
    }
    
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            SceneManager.LoadScene(2);
        }
    }

    private void FixedUpdate() {
        if (Camera.main.transform.position.y - transform.position.y > 8) {
            health = 0;
            GameHandler.healthBarImage.fillAmount = health / 10f;
            Die();
        }

        if (invulnCount < 120) {
            invulnCount++;
        } else {
            invulnerable = false;
        }
        if (invulnCount >= 30) {
            Stunned = false;
        }

        Controls();

        Move();

        if (Facing == DirectionH.RIGHT)
            spriteRenderer.flipX = false;

        if (Facing == DirectionH.LEFT)
            spriteRenderer.flipX = true;

        animator.SetBool("Running", maxSpeed);
        animator.SetBool("OnGround", OnGround);
        animator.SetBool("Shooting", Shooting);
        //animator.SetBool("Climbing", Climbing);
        animator.SetBool("Invulnerable", invulnerable);
        animator.SetBool("Stunned", Stunned);

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

        float belt = 0;

        if (lastGroundTag == "Belt Left" && OnGround)
            belt = -0.05f;
        else if (lastGroundTag == "Belt Right" && OnGround)
            belt = 0.05f;

        Velocity = new Vector2(dir * moveSpeed + belt, Velocity.y);

        if (OnGround && GameHandler.vc.Jump.Press) {
            Velocity = new Vector2(Velocity.x, Convert(JumpPower));
            OnGround = false;
            Jumping = true;
        }

        if (Movement.y > Convert(MinJump) && !GameHandler.vc.Jump.Value && Jumping) {
            Movement = new Vector2(Movement.x, Convert(JumpCancelSpeed));
            Velocity = new Vector2(Velocity.x, Convert(JumpCancelSpeed));
            Jumping = false;
        }

        if (Movement.y < 0f)
            Jumping = false;


        List<Pellet> removal = new List<Pellet>();

        foreach (Pellet p in pellets) {
            if (p == null)
                removal.Add(p);
        }
        foreach (Pellet p in removal) {
            pellets.Remove(p);
        }

        if (GameHandler.vc.Shoot.Press && pellets.Count < 3) {
            Pellet pellet = Instantiate(pelletPrefab, transform.position + new Vector3(Facing == DirectionH.RIGHT ? 1f : -1f, 0.77f, 0f), Quaternion.Euler(0f, Facing == DirectionH.RIGHT ? 0f : 180f, 0f));

            pellets.Add(pellet);

            Shooting = true;
            shootCount = 0;

            GameHandler.PlayAudio(GameHandler.playerShootAudio);

        }

        if (Shooting) {
            shootCount++;

            if (shootCount >= 40) {
                shootCount = 0;
                Shooting = false;
            }
        }

        if (dir == 1)
            Facing = DirectionH.RIGHT;
        if (dir == -1)
            Facing = DirectionH.LEFT;
    }

    void Move() {
        LayerMask mask = LayerMask.GetMask("Ground", "Belt Left", "Belt Right");

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
                lastGroundTag = hit.collider.tag;
                Velocity = new Vector2(Velocity.x, 0f);
            }

            Debug.DrawRay(rayPos, down, Color.cyan);
            if (i == 4) {
                //Debug.Log("y pos: " + rayPos.y);
            }
        }


        // RIGHT
        Vector2 right = Vector2.right * Mathf.Max(0f, Velocity.x);
        for (int i = 0; i <= 4; i++) {
            Vector2 rayPos = new Vector2(position.x + (7f / 16f) + DetectionBufferH, position.y + (i / 4f * 24f / 16f));

            RaycastHit2D hit = Physics2D.Raycast(rayPos, right, right.magnitude, mask);

            if (hit.collider != null && hit.distance < right.magnitude) {
                right = new Vector2(hit.distance, 0f);
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
    void OnTriggerStay2D(Collider2D collision) {

        if (collision.tag == "Enemy" && !invulnerable) {
            if (collision.transform.position.x - transform.position.x > 0) {
                Facing = DirectionH.RIGHT;
                Velocity = new Vector2(-Convert("00.80"), Convert("01.00"));
            } else {
                Facing = DirectionH.LEFT;
                Velocity = new Vector2(Convert("00.80"), Convert("01.00"));
            }
            Stunned = true;
            invulnerable = true;
            invulnCount = 0;

            health--;

            GameHandler.healthBarImage.fillAmount = health / 10f;

            if (health == 0) {
                Die();
            } else {
                GameHandler.PlayAudio(GameHandler.playerHitAudio);
            }
        }

        if (collision.tag == "Lava" && !invulnerable) {
            health = 0;
            GameHandler.healthBarImage.fillAmount = health / 10f;
            Die();
        }

        if (collision.gameObject.tag == "Level2") {
            SceneManager.LoadScene(2);
        }

        if (collision.tag == "Win") {
            GameHandler.Win();
            Destroy(gameObject);
        }
    }

    public void Die() {
        Instantiate(dieParticle, transform.position + new Vector3(0f, 0.77f, -5f), Quaternion.Euler(90f, 0f, 0f));

        Destroy(gameObject);
        GameHandler.PlayAudio(GameHandler.playerDieAudio);
        GameHandler.Lose();
    }
}

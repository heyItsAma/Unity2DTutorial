using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{   
    //Finite state machine (player statuses)
    private enum State { idle, running, jumping, falling, hurt }
    private State state = State.idle;

    //Inspector variables
    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D collider2D;

    //Serializable lets me change value in Unity UI but keep it a private variable
    [SerializeField] private LayerMask ground;
    [SerializeField] private float playerSpeed = 5f;
    [SerializeField] private float playerJumpPower = 1f;
    [SerializeField] private int cherries = 0;
    [SerializeField] private Text cherryText;
    [SerializeField] private float hurtForce = 10f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        collider2D = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if(state != State.hurt)
        {
            InputManager();
        }

        VelocityStateSwitch();
        animator.SetInteger("state", (int)state);
    }

    private void OnCollisionEnter2D(Collision2D enemyObject)
    {
        if (enemyObject.gameObject.tag == "Enemy")
        {
            if(state == State.falling)
            {
                Destroy(enemyObject.gameObject);
            } else
            {
                state = State.hurt;
                if(enemyObject.gameObject.transform.position.x > transform.position.x)
                {
                    // If enemy position is to my right, therefore I should take damage and move to the left
                    rb.velocity = new Vector2(-hurtForce, rb.velocity.y);
                } else
                {
                    // Enemy is to my left, so I take damage and move to the right
                    rb.velocity = new Vector2(hurtForce, rb.velocity.y);
                }
            }          
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Collectibles")
        {
            Destroy(collision.gameObject);
            cherries+=1;
            cherryText.text = cherries.ToString();
        }
    }

    private void InputManager()
    {
        float hDirection = Input.GetAxis("Horizontal");
        _ = Input.GetAxis("Vertical");

        if (hDirection < 0)
        {
            rb.velocity = new Vector2(-playerSpeed, rb.velocity.y); //negative for left
            transform.localScale = new Vector2(-1, 1);
        }
        else if (hDirection > 0)
        {
            rb.velocity = new Vector2(playerSpeed, rb.velocity.y);
            transform.localScale = new Vector2(1, 1);
        }

        if (Input.GetKeyDown("space") && collider2D.IsTouchingLayers(ground))
        {
            rb.velocity = new Vector2(rb.velocity.x, playerJumpPower);
            state = State.jumping;
        }
    }

    private void VelocityStateSwitch()
    {   
        if (state == State.jumping)
        {

            if (rb.velocity.y < 0.1f) {
                state = State.falling;
            }

        }
        else if (state == State.falling)
        {
            if (collider2D.IsTouchingLayers(ground))
            {
                state = State.idle;
            }
        }
        else if (state == State.hurt)
        {
            if(Mathf.Abs(rb.velocity.x) < 0.1f)
            {
                state = State.idle;
            }
        }
        else if (Mathf.Abs(rb.velocity.x) > 2f)
        {
            //We are moving
            state = State.running;
        }
        else
        {
            state = State.idle;
        }
    }
}

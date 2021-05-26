using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * This script controls the player controller
 */

public class PlayerController : MonoBehaviour
{
    private int amountOfJumpsLeft;

    private bool isFacingRight = true;
    private bool isWalking;
    private bool canJump;
    public bool isDead = false;
    public bool isFrozen = false;
    public bool isBlocking = false;

    private Rigidbody2D rb;
    private Animator anim;

    public float movementSpeed = 10.0f;
    public float jumpForce = 16.0f;
    public float groundCheckRadius;

    public int amountOfJumps = 1;
    public int maxHealth = 100;
    public int currentHealth;

    public HealthBar healthBar;
    public Transform groundCheck;
    private LayerMask whatIsGround;
    public GameOverScreen gameOverScreen;
    private PlayerCombat playerCombat;

    private float horizontal;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerCombat = GetComponent<PlayerCombat>();
        healthBar.SetMax(maxHealth);
        amountOfJumpsLeft = amountOfJumps;
        whatIsGround = LayerMask.GetMask("Ground", "ignoreGround");

    }

    private void Update()
    {
       // IsGrounded();

        if (!PauseMenuManager.isPaused)
        {
            if (!isDead)
            {
                if (!isBlocking)
                {
                    rb.velocity = new Vector2(horizontal * movementSpeed, rb.velocity.y);
                }
                CheckIfCanJump();
                UpdateAnimations();
                CheckMovementDirection();
            }
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        Debug.Log("trying to move");
        horizontal = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (canJump)
        {
            GetComponent<Animator>().SetTrigger("isJumping");
            if (context.performed)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                amountOfJumpsLeft--;
            }
            if (context.canceled && rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            }
        }
    }

    public void Block(InputAction.CallbackContext context)
    {
        if (IsGrounded())
        {
            if (context.performed)
            {
                StartCoroutine(playerCombat.UseStamina(20f));
                isBlocking = true;
                rb.constraints = RigidbodyConstraints2D.FreezePosition;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                isFrozen = true;
            }
            if (context.canceled)
            {
                isBlocking = false;
                rb.constraints = RigidbodyConstraints2D.None;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                isFrozen = false;
            }
        }
    }

    //checks if player is on the ground
    private bool IsGrounded()
    {
    
        // return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        return true;
    }

    //checks if player can jump and if they have any more jumps left
    private void CheckIfCanJump()
    {
        if (IsGrounded() && rb.velocity.y <= 1)
        {
            amountOfJumpsLeft = amountOfJumps;
        }

        if (amountOfJumpsLeft <= 0)
        {
            canJump = false;
        }
        else
        {
            canJump = true;
        }
    }
    private void CheckMovementDirection()
    {
        if (isFacingRight && horizontal < 0)
        {
            Flip();
        }
        else if (!isFacingRight && horizontal > 0)
        {
            Flip();
        }

        if (rb.velocity.x > 0.01f || rb.velocity.x < -0.01f)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    private void UpdateAnimations()
    {
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", IsGrounded());
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isBlocking", isBlocking);
    }
    //handles controller inputs for functions other than movement

    private void Flip()
    {
        if (!isFrozen)
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    public void TakeDamage(int damage, bool ignoreBlock)
    {
        if (!isDead)
        {
            if (isBlocking && !ignoreBlock)
            {
                anim.SetTrigger("isBlock");
                currentHealth -= (int)(damage * 0.1);
            }
            else
            {
                anim.SetTrigger("isHit");
                currentHealth -= damage;
            }
        }
        healthBar.Set(currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    //Freezes the player, used for after attacks so you can't move and spam attacks
    public void Freeze()
    {
        StartCoroutine("freezeTime");
    }

    private IEnumerator freezeTime()
    {
        isFrozen = true;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(0.5f);
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        isFrozen = false;

    }

    private void Die()
    {
        isDead = true;
        anim.SetBool("isWalking", false);
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        anim.SetBool("isDead", true);

        gameObject.SetActive(false);
        //needs a continue/restart btn
        //gameOverScreen.Setup();
        //currentHealth = 100;
        //healthBar.Set(currentHealth);
        //rb.constraints = RigidbodyConstraints2D.None;
        //rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        //gameObject.SetActive(true);
    }
}
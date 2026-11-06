using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float accelTime = 0.08f;
    public float decelTime = 0.12f;
    public bool isStunned = false;
    public int GetXDirection() => daggerXDirection;
    public HPBarUI hpBar;
    public Vector2 lookDirection { get; private set; }

    [Header("Audio")]
    public AudioClip daggerThrowSFX;
    protected AudioSource audioSource;

    [SerializeField]
    protected GameObject _daggerPrefab;
    protected int daggerRotation;
    protected int daggerXDirection;     // Values can be 1, 0, and -1
    protected int daggerYDirection;     // ^Same here
    protected Vector2 daggerVector;
    [SerializeField]
    protected float _daggerSpeed;
    [SerializeField]
    protected float _daggerCooldown;
    protected float _lastThrowTime;

    protected bool isInWater = false;
    protected bool _throwKeyHold;
    protected Rigidbody2D rb;
    protected Vector2 input;
    protected Vector2 targetVelocity;
    protected Vector2 velocityRef;
    protected Animator animator;
    protected bool touchingWater = false;
    protected bool touchingGround = false;

    protected void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    protected void Update()
    {
        float x = 0f;
        float y = 0f;
        bool isInWater = touchingWater && !touchingGround;

        animator.SetBool("InWater", isInWater);

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed)
            {
                x -= 1f;
                daggerXDirection = -1;
                animator.SetBool("Running", true);
                if (!Keyboard.current.sKey.isPressed && !Keyboard.current.wKey.isPressed)
                {
                    animator.SetFloat("LastYInput", 0);
                    daggerYDirection = 0;
                }
                animator.SetFloat("LastXInput", -1);
                animator.SetFloat("CurrentXInput", -1);
            }
            else if (Keyboard.current.dKey.isPressed)
            {
                x += 1f;
                daggerXDirection = 1;
                animator.SetBool("Running", true);
                if (!Keyboard.current.sKey.isPressed && !Keyboard.current.wKey.isPressed)
                {
                    animator.SetFloat("LastYInput", 0);
                    daggerYDirection = 0;
                }
                animator.SetFloat("LastXInput", 1);
                animator.SetFloat("CurrentXInput", 1);
            }
            else animator.SetFloat("CurrentXInput", 0);
            if (Keyboard.current.sKey.isPressed)
            {
                y -= 1f;
                daggerYDirection = -1;
                animator.SetBool("Running", true);
                if (!Keyboard.current.aKey.isPressed && !Keyboard.current.dKey.isPressed)
                {
                    animator.SetFloat("LastXInput", 0);
                    daggerXDirection = 0;
                }
                animator.SetFloat("LastYInput", -1);
                animator.SetFloat("CurrentYInput", -1);
            }
            else if (Keyboard.current.wKey.isPressed)
            {
                y += 1f;
                daggerYDirection = 1;
                animator.SetBool("Running", true);
                if (!Keyboard.current.aKey.isPressed && !Keyboard.current.dKey.isPressed)
                {
                    animator.SetFloat("LastXInput", 0);
                    daggerXDirection = 0;
                }
                animator.SetFloat("LastYInput", 1);
                animator.SetFloat("CurrentYInput", 1);
            }
            else animator.SetFloat("CurrentYInput", 0);

            if (!Keyboard.current.wKey.isPressed && !Keyboard.current.sKey.isPressed && !Keyboard.current.dKey.isPressed && !Keyboard.current.aKey.isPressed) animator.SetBool("Running", false);

            if (Keyboard.current.spaceKey.isPressed) _throwKeyHold = true;
            else _throwKeyHold = false;
        }
        else animator.SetBool("Running", false);
        if (_throwKeyHold == true)
        {
            float timeSinceLastThrow = Time.time - _lastThrowTime;
            if (timeSinceLastThrow > _daggerCooldown)
            {
                ThrowDagger();
                _lastThrowTime = Time.time;
            }
        }
        input = new Vector2(x, y);
        if (input.sqrMagnitude > 1f) input = input.normalized;
        if (input != Vector2.zero) lookDirection = input.normalized;
        targetVelocity = input * moveSpeed;
    }

    protected void ThrowDagger()
    {
        // direction in unity assumes 0 deg. is Right, not Up, keep this in mind
        if (daggerXDirection == 0 && daggerYDirection == 1) daggerRotation = 90;
        else if (daggerXDirection == 0 && daggerYDirection == -1) daggerRotation = 270;
        else if (daggerYDirection == 0 && daggerXDirection == 1) daggerRotation = 0;
        else if (daggerYDirection == 0 && daggerXDirection == -1) daggerRotation = 180;
        else if (daggerXDirection == 1 && daggerYDirection == 1) daggerRotation = 45;
        else if (daggerXDirection == 1 && daggerYDirection == -1) daggerRotation = 315;
        else if (daggerXDirection == -1 && daggerYDirection == 1) daggerRotation = 135;
        else if (daggerXDirection == -1 && daggerYDirection == -1) daggerRotation = 225;
        if (daggerThrowSFX != null) audioSource.PlayOneShot(daggerThrowSFX);
        Quaternion rot = Quaternion.Euler(0, 0, daggerRotation);
        GameObject dagger = Instantiate(_daggerPrefab, transform.position, rot);
        Rigidbody2D rigidbody = dagger.GetComponent<Rigidbody2D>();
        daggerVector = new Vector2(daggerXDirection, daggerYDirection);
        rigidbody.linearVelocity = daggerVector.normalized * _daggerSpeed;
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Water")) touchingWater = true;
        if (collision.CompareTag("Ground")) touchingGround = true;
    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Water")) touchingWater = false;
        if (collision.CompareTag("Ground")) touchingGround = false;
    }

    public bool IsInWater()
    {
        return touchingWater && !touchingGround;
    }

    protected void FixedUpdate()
    {
        if (isStunned) return;

        float dampingTime = (targetVelocity.sqrMagnitude > 0.0001f) ? accelTime : decelTime;
        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref velocityRef, dampingTime);
    }

}

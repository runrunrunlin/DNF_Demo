using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ===================== 移动参数 =====================
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float knockbackForce = 15f;

    // ===================== 地面检测 =====================
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // ===================== HitBox引用 =====================
    [SerializeField] private HitBoxController hitBoxController_Punch;
    [SerializeField] private HitBoxController hitBoxController_Kick;

    // ===================== 视觉反馈 =====================
    [SerializeField] private VisualFeedback visualFeedback;

    // ===================== 音效 =====================
    [SerializeField] private AudioClip punchWhoosh;
    [SerializeField] private AudioClip kickWhoosh;
    [SerializeField] private float whooshVolume = 0.4f;
    private AudioSource audioSource;

    // ===================== 组件引用 =====================
    private Rigidbody2D rb;
    private Animator anim;

    // ===================== 运行时状态 =====================
    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isAttacking;
    private bool isKnockedBack;
    private bool hasEnteredAttack1;
    private bool comboQueued;
    private bool punchHitBoxActive;
    private bool kickHitBoxActive;
    private bool punchWhooshPlayed;
    private bool kickWhooshPlayed;

    void Start()
    {
        rb          = GetComponent<Rigidbody2D>();
        anim        = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        CheckGround();
        HandleMovement();
        HandleJump();
        HandleAttack();
        CheckAttackEnd();
        HandleHitBoxes();
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        anim.SetBool("isGrounded", isGrounded);
    }

    private void HandleMovement()
    {
        if (isKnockedBack) return;

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            anim.SetBool("isRunning", false);
            return;
        }

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        anim.SetBool("isRunning", horizontalInput != 0f);

        if (horizontalInput > 0 && !isFacingRight) Flip();
        else if (horizontalInput < 0 && isFacingRight) Flip();
    }

    private void HandleJump()
    {
        if (isKnockedBack) return;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("jumpTrigger");
        }
    }

    private void HandleAttack()
    {
        if (!Input.GetKeyDown(KeyCode.J)) return;

        if (!isAttacking)
        {
            isAttacking = true;
            hasEnteredAttack1 = false;
            comboQueued = false;
            punchWhooshPlayed = false;
            anim.SetTrigger("attackTrigger");
            visualFeedback?.OnAttack();
            return;
        }

        if (comboQueued) return;

        if (hasEnteredAttack1)
        {
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Attack1"))
            {
                float t = state.normalizedTime;
                if (t >= 0.4f && t < 0.8f)
                    FireCombo();
                else if (t < 0.4f)
                    comboQueued = true;
            }
        }
        else
        {
            comboQueued = true;
        }
    }

    private void CheckAttackEnd()
    {
        if (!isAttacking) return;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        bool inAttack1 = stateInfo.IsName("Attack1");
        bool inAttack2 = stateInfo.IsName("Attack2");

        if (inAttack1)
        {
            hasEnteredAttack1 = true;

            if (comboQueued)
            {
                float t = stateInfo.normalizedTime;
                if (t >= 0.4f && t < 0.8f)
                {
                    comboQueued = false;
                    FireCombo();
                }
                else if (t >= 0.8f)
                {
                    comboQueued = false;
                }
            }
        }

        if (hasEnteredAttack1 && !inAttack1 && !inAttack2)
        {
            isAttacking = false;
            hasEnteredAttack1 = false;
            comboQueued = false;
        }
    }

    private void FireCombo()
    {
        kickWhooshPlayed = false;
        anim.SetTrigger("comboTrigger");
        visualFeedback?.OnAttack();
    }

    private void HandleHitBoxes()
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        // Punch HitBox - Attack1
        if (stateInfo.IsName("Attack1"))
        {
            float t = stateInfo.normalizedTime;

            // 出拳开始时播放风声（20%时触发，早于HitBox）
            if (t >= 0.2f && !punchWhooshPlayed)
            {
                punchWhooshPlayed = true;
                if (punchWhoosh != null && audioSource != null)
                    audioSource.PlayOneShot(punchWhoosh, whooshVolume);
            }

            bool shouldBeActive = t >= 0.4f && t < 0.6f;
            if (shouldBeActive && !punchHitBoxActive)
            {
                punchHitBoxActive = true;
                hitBoxController_Punch?.EnableHitBox();
            }
            else if (!shouldBeActive && punchHitBoxActive)
            {
                punchHitBoxActive = false;
                hitBoxController_Punch?.DisableHitBox();
            }
        }
        else if (punchHitBoxActive)
        {
            punchHitBoxActive = false;
            hitBoxController_Punch?.DisableHitBox();
        }

        // Kick HitBox - Attack2
        if (stateInfo.IsName("Attack2"))
        {
            float t = stateInfo.normalizedTime;

            // 踢腿开始时播放风声
            if (t >= 0.2f && !kickWhooshPlayed)
            {
                kickWhooshPlayed = true;
                if (kickWhoosh != null && audioSource != null)
                    audioSource.PlayOneShot(kickWhoosh, whooshVolume);
            }

            bool shouldBeActive = t >= 0.4f && t < 0.6f;
            if (shouldBeActive && !kickHitBoxActive)
            {
                kickHitBoxActive = true;
                hitBoxController_Kick?.EnableHitBox();
            }
            else if (!shouldBeActive && kickHitBoxActive)
            {
                kickHitBoxActive = false;
                hitBoxController_Kick?.DisableHitBox();
            }
        }
        else if (kickHitBoxActive)
        {
            kickHitBoxActive = false;
            hitBoxController_Kick?.DisableHitBox();
        }
    }

    public void ApplyKnockback(Vector3 enemyPosition)
    {
        float dir = transform.position.x > enemyPosition.x ? 1f : -1f;
        rb.AddForce(new Vector2(dir * knockbackForce, 0f), ForceMode2D.Impulse);
        StartCoroutine(KnockbackRoutine(0.3f));
    }

    private IEnumerator KnockbackRoutine(float duration)
    {
        isKnockedBack = true;
        yield return new WaitForSeconds(duration);
        isKnockedBack = false;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
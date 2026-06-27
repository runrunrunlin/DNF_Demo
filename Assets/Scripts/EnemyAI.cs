using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // ===================== 检测范围参数 =====================
    [SerializeField] private float detectionRange = 8f;   // 发现玩家的距离
    [SerializeField] private float attackRange    = 1.5f; // 触发攻击的距离
    [SerializeField] private float moveSpeed      = 3f;   // 追击速度
    [SerializeField] private float attackCooldown = 1.5f; // 攻击冷却时间（秒）
    [SerializeField] private float hurtDuration   = 0.5f; // 受伤硬直时长（秒）

    // ===================== 组件 & 引用 =====================
    private Rigidbody2D   rb;
    private Animator      anim;
    private CharacterStats enemyStats;   // 自身属性（用于获取攻击伤害）
    private CharacterStats playerStats;  // 玩家属性（用于调用TakeDamage）
    private Transform      playerTrans;  // 玩家Transform，用于计算距离和方向

    // ===================== 状态机 =====================
    private enum State { Idle, Chase, Attack, Hurt, Dead }
    private State currentState = State.Idle;

    // ===================== 运行时计时器 =====================
    private float attackTimer;    // 攻击冷却计时器，归零后才能再次攻击
    private float hurtTimer;      // 受伤硬直倒计时
    private float knockbackTimer; // 击退期间 > 0，此时跳过移动逻辑让物理力自然推动敌人

    // ===================== 朝向 =====================
    private bool isFacingRight = true;

    // -------------------------------------------------------

    void Start()
    {
        rb          = GetComponent<Rigidbody2D>();
        anim        = GetComponent<Animator>();
        enemyStats  = GetComponent<CharacterStats>();

        // 通过Tag找到玩家，获取其组件引用
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTrans = player.transform;
            playerStats = player.GetComponent<CharacterStats>();
        }
    }

    void Update()
    {
        if (playerTrans == null) return;

        // 被击退时跳过所有移动逻辑，让 Rigidbody2D 的物理力自然推动敌人
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.deltaTime;
            return;
        }

        // 根据当前状态分发逻辑
        switch (currentState)
        {
            case State.Idle:   HandleIdle();   break;
            case State.Chase:  HandleChase();  break;
            case State.Attack: HandleAttack(); break;
            case State.Hurt:   HandleHurt();   break;
            case State.Dead:                   break; // 死亡状态不做任何事
        }
    }

    // ===================== 各状态处理 =====================

    // 待机：检测玩家是否进入探测范围
    private void HandleIdle()
    {
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isRunning", false);

        if (DistanceToPlayer() <= detectionRange)
        {
            ChangeState(State.Chase);
        }
    }

    // 追击：向玩家移动，进入攻击范围则切换为Attack
    private void HandleChase()
    {
        FacePlayer();
        anim.SetBool("isRunning", true);

        float dist = DistanceToPlayer();

        if (dist <= attackRange)
        {
            ChangeState(State.Attack);
            return;
        }

        if (dist > detectionRange)
        {
            // 玩家跑出探测范围，回到Idle
            ChangeState(State.Idle);
            return;
        }

        // 向玩家方向平移
        if (rb.bodyType == RigidbodyType2D.Static) return;
        float dir = playerTrans.position.x > transform.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    // 攻击：停下来，冷却完毕后对玩家造成伤害
    private void HandleAttack()
    {
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isRunning", false);
        FacePlayer();

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            anim.SetTrigger("attackTrigger");
            StartCoroutine(DealDamageAfterDelay(0.3f));
        }

        // 玩家离开攻击范围则继续追击
        if (DistanceToPlayer() > attackRange)
        {
            ChangeState(State.Chase);
        }
    }

    // 受伤硬直：等待硬直时间结束后恢复追击
    private void HandleHurt()
    {
        if (rb.bodyType == RigidbodyType2D.Static) return;
        rb.linearVelocity = Vector2.zero;

        hurtTimer -= Time.deltaTime;
        if (hurtTimer <= 0f)
        {
            ChangeState(State.Chase);
        }
    }

    // ===================== 公开接口 =====================

    // 由 HitBoxController.ApplyKnockback() 调用，开始击退无敌窗口
    public void StartKnockback(float duration)
    {
        knockbackTimer = duration;
    }

    // 由 CharacterStats.TakeDamage() 调用，触发受伤状态
    public void OnHurt()
    {
        if (currentState == State.Dead) return;

        hurtTimer = hurtDuration;
        ChangeState(State.Hurt);
    }

    // 由 CharacterStats.Die() 调用，进入死亡状态
    public void OnDead()
    {
        ChangeState(State.Dead);
    }

    private IEnumerator DealDamageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (playerTrans == null) yield break;  
        
        Rigidbody2D playerRb = playerTrans.GetComponent<Rigidbody2D>();

        if (playerRb != null && playerRb.bodyType == RigidbodyType2D.Static) yield break;

        if (currentState != State.Dead && playerStats != null && !playerStats.IsDead)
        {
            playerStats.TakeDamage(enemyStats.AttackDamage, transform.position);
        }
    }

    // ===================== 辅助方法 =====================

    // 切换状态，并统一处理进入新状态时的Animator参数
    private void ChangeState(State newState)
    {
        currentState = newState;

        switch (newState)
        {
            case State.Hurt:
                anim.SetTrigger("hurtTrigger");
                break;
            case State.Dead:
                rb.linearVelocity = Vector2.zero;
                anim.SetBool("isDead", true);
                break;
        }
    }

    // 计算与玩家的水平距离
    private float DistanceToPlayer()
    {
        return Mathf.Abs(playerTrans.position.x - transform.position.x);
    }

    // 根据玩家位置翻转敌人朝向
    private void FacePlayer()
    {
        bool playerIsRight = playerTrans.position.x > transform.position.x;

        if (playerIsRight && !isFacingRight)
        {
            Flip();
        }
        else if (!playerIsRight && isFacingRight)
        {
            Flip();
        }
    }

    // 水平翻转（缩放X取反）
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    // 在Scene视图中可视化检测范围，方便调试
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // 探测范围

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);    // 攻击范围
    }
}

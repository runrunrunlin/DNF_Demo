using System;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    // ===================== 角色属性 =====================
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float attackDamage = 20f;

    private float currentHP;
    private bool isDead;

    // ===================== 音效 =====================
    [SerializeField] private AudioClip hurtSound;
    private AudioSource audioSource;

    // ===================== 血条UI事件 =====================
    public event Action<float, float> OnHealthChanged;

    // ===================== 组件引用 =====================
    private Animator anim;
    [SerializeField] private VisualFeedback visualFeedback;

    void Start()
    {
        currentHP = maxHP;
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // ===================== 受伤逻辑 =====================

    public void TakeDamage(float damage, Vector3 attackerPosition)
    {
        if (isDead) return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0f);

        UpdateHealthUI();
        visualFeedback?.OnHurt();
        GetComponent<PlayerController>()?.ApplyKnockback(attackerPosition);
        if (gameObject.CompareTag("Player")) ScreenFlash.Instance?.Flash();

        // 播放受伤音效
        if (hurtSound != null && audioSource != null)
            audioSource.PlayOneShot(hurtSound, 0.4f);

        if (currentHP <= 0f)
        {
            Die();
        }
        else
        {
            anim.SetTrigger("hurtTrigger");
        }
    }

    // ===================== 死亡逻辑 =====================

    private void Die()
    {
        isDead = true;
        anim.SetBool("isDead", true);
        visualFeedback?.OnDead();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Static;

        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null) enemyAI.enabled = false;

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null) playerController.enabled = false;

        Destroy(gameObject, 2f);
    }

    // ===================== 血条UI通知 =====================

    private void UpdateHealthUI()
    {
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    // ===================== 对外只读属性 =====================

    public float CurrentHP    => currentHP;
    public float MaxHP        => maxHP;
    public float AttackDamage => attackDamage;
    public bool  IsDead       => isDead;
}
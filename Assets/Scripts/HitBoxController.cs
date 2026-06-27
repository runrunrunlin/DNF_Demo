using System.Collections.Generic;
using UnityEngine;

public class HitBoxController : MonoBehaviour
{
    // ===================== 伤害来源 =====================
    [SerializeField] private CharacterStats ownerStats;

    // ===================== 击退力 =====================
    [SerializeField] private float knockbackForce = 4f;

    // ===================== 音效 =====================
    [SerializeField] private AudioClip hitSound;
    private AudioSource audioSource;

    // ===================== 命中记录 =====================
    private readonly HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

    // ===================== 组件缓存 =====================
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        audioSource = GetComponentInParent<AudioSource>();
    }

    // ===================== 碰撞检测 =====================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        if (hitEnemies.Contains(other.gameObject)) return;

        hitEnemies.Add(other.gameObject);

        CharacterStats enemyStats = other.GetComponent<CharacterStats>();
        if (enemyStats != null)
        {
            enemyStats.TakeDamage(ownerStats.AttackDamage, ownerStats.transform.position);
            ApplyKnockback(other.gameObject);
            HitStop.Instance?.TriggerHitStop(0.15f);
            if (hitSound != null && audioSource != null)
                audioSource.PlayOneShot(hitSound);
        }
    }

    // ===================== 攻击生命周期 =====================

    public void EnableHitBox()
    {
        hitEnemies.Clear();
        gameObject.SetActive(true);
        ImmediateOverlapCheck();
    }

    public void DisableHitBox()
    {
        gameObject.SetActive(false);
        hitEnemies.Clear();
    }

    // ===================== 击退 =====================

    private void ApplyKnockback(GameObject enemy)
    {
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb == null) return;

        float dirX = enemy.transform.position.x - ownerStats.transform.position.x;
        Vector2 force = new Vector2(dirX, 0f).normalized * knockbackForce;
        enemyRb.AddForce(force, ForceMode2D.Impulse);

        enemy.GetComponent<EnemyAI>()?.StartKnockback(0.15f);
    }

    // ===================== 主动检测（处理休眠刚体） =====================

    private void ImmediateOverlapCheck()
    {
        if (boxCollider == null) return;

        Vector2 center = (Vector2)transform.TransformPoint(boxCollider.offset);
        Vector2 size = new Vector2(
            boxCollider.size.x * Mathf.Abs(transform.lossyScale.x),
            boxCollider.size.y * Mathf.Abs(transform.lossyScale.y)
        );

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, transform.eulerAngles.z);
        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            if (hitEnemies.Contains(hit.gameObject)) continue;

            hitEnemies.Add(hit.gameObject);

            CharacterStats stats = hit.GetComponent<CharacterStats>();
            if (stats != null)
            {
                stats.TakeDamage(ownerStats.AttackDamage, ownerStats.transform.position);
                ApplyKnockback(hit.gameObject);
                HitStop.Instance?.TriggerHitStop(0.15f);
                if (hitSound != null && audioSource != null)
                    audioSource.PlayOneShot(hitSound);
            }
        }
    }
}
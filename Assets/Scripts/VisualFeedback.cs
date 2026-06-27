using System.Collections;
using UnityEngine;

public class VisualFeedback : MonoBehaviour
{
    [SerializeField] private int   hurtFlashCount    = 3;
    [SerializeField] private float hurtFlashInterval = 0.15f;

    private SpriteRenderer sr;
    private Coroutine activeCoroutine;

    private static readonly Color hurtColor = new Color(1f, 0.2f, 0.2f, 1f);
    private static readonly Color deadColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void OnAttack() { }

    public void OnHurt()
    {
        StartFeedback(HurtRoutine());
    }

    public void OnDead()
    {
        StartFeedback(DeadRoutine());
    }

    private void StartFeedback(IEnumerator routine)
    {
        StopActiveCoroutine();
        activeCoroutine = StartCoroutine(routine);
    }

    private void StopActiveCoroutine()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
    }

    private IEnumerator DeadRoutine()
    {
        if (sr != null) sr.color = deadColor;

        float elapsed  = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (sr != null)
            {
                Color c = deadColor;
                c.a     = Mathf.Lerp(1f, 0f, elapsed / duration);
                sr.color = c;
            }
            yield return null;
        }

        if (sr != null) sr.color = new Color(deadColor.r, deadColor.g, deadColor.b, 0f);
        activeCoroutine = null;
    }

    private IEnumerator HurtRoutine()
    {
        for (int i = 0; i < hurtFlashCount; i++)
        {
            if (sr != null) sr.color = hurtColor;
            yield return new WaitForSeconds(hurtFlashInterval);
            if (sr != null) sr.color = Color.white;
            yield return new WaitForSeconds(hurtFlashInterval);
        }
        activeCoroutine = null;
    }
}

using System.Collections;
using UnityEngine;

// 命中顿帧：击中敌人时短暂压缩 timeScale 以产生冲击感。
// 作为单例挂在场景中任意持久存在的 GameObject 上（如 Main Camera）。
public class HitStop : MonoBehaviour
{
    public static HitStop Instance { get; private set; }

    private Coroutine hitStopCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // 触发顿帧：把 timeScale 压到 0.05（接近但不完全停止），
    // 等待 duration 真实秒后恢复为 1。
    // 若上一次顿帧尚未结束就再次被调用（如连续命中），
    // 则取消上一次，重新计时，避免 timeScale 永远无法复原。
    public void TriggerHitStop(float duration)
    {
        if (hitStopCoroutine != null)
            StopCoroutine(hitStopCoroutine);

        hitStopCoroutine = StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(duration); // 用真实时间，不受 timeScale 影响
        Time.timeScale = 1f;
        hitStopCoroutine = null;
    }
}

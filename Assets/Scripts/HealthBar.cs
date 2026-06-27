using UnityEngine;
using UnityEngine.UI;

// 挂在血条的Slider物体上，订阅CharacterStats的血量变化事件实时更新UI
public class HealthBar : MonoBehaviour
{
    [SerializeField] private CharacterStats targetStats; // 要追踪的角色属性组件

    private Slider slider; // 血条Slider组件

    void Start()
    {
        slider = GetComponent<Slider>();

        if (targetStats == null) return;

        // 初始化Slider范围与初始值
        slider.minValue = 0f;
        slider.maxValue = targetStats.MaxHP;
        slider.value    = targetStats.CurrentHP;

        // 订阅血量变化事件，血量改变时自动调用OnHealthChanged
        targetStats.OnHealthChanged += OnHealthChanged;
    }

    void OnDestroy()
    {
        // 取消订阅，防止目标角色被销毁后引发空引用
        if (targetStats != null)
        {
            targetStats.OnHealthChanged -= OnHealthChanged;
        }
    }

    // 收到血量变化通知时更新Slider，并在角色死亡后隐藏血条
    private void OnHealthChanged(float current, float max)
    {
        slider.value = current;

        if (current <= 0f)
        {
            gameObject.SetActive(false); // 血量归零，隐藏血条
        }
    }
}

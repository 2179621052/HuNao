// 必须添加System命名空间（事件参数依赖EventArgs）
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private GameObject hasProgressGameObject; // 挂载了IHasProgress接口的物体
    [SerializeField] private Image barImage;

    private IHasProgress hasProgress;

    private void Start()
    {

        // 核心修复：从指定物体获取IHasProgress接口实例
        hasProgress = hasProgressGameObject.GetComponent<IHasProgress>();

        // 空值保护：防止未赋值或未挂载接口导致报错
        if (hasProgress == null)
        {
            Debug.LogError($"[{gameObject.name}] 目标物体 {hasProgressGameObject.name} 未挂载实现IHasProgress接口的脚本！");
            return;
        }
        if (barImage == null)
        {
            Debug.LogError($"[{gameObject.name}] 未赋值barImage（进度条Image组件）！");
            return;
        }

        // 订阅进度变更事件（空值保护）
        hasProgress.OnProgressChanged += HasProgress_OnProgressChanged;

        // 初始化进度条状态
        barImage.fillAmount = 0f;
        Hide();
    }

    // 进度变更回调
    private void HasProgress_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        // 确保barImage不为空
        if (barImage == null) return;

        barImage.fillAmount = e.progressNormalized;

        // 根据进度显示/隐藏进度条
        if (Mathf.Approximately(e.progressNormalized, 0f) || Mathf.Approximately(e.progressNormalized, 1f))
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    // 显示进度条
    private void Show()
    {
        gameObject.SetActive(true);
    }

    // 隐藏进度条
    private void Hide()
    {
        gameObject.SetActive(false);
    }

    // 关键：取消事件订阅，避免内存泄漏/空引用报错
    private void OnDestroy()
    {
        if (hasProgress != null)
        {
            hasProgress.OnProgressChanged -= HasProgress_OnProgressChanged;
        }
    }
}

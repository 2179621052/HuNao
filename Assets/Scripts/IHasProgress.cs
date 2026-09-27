// 必须添加这行：引入System命名空间（包含EventArgs/EventHandler<T>）
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasProgress
{
    // 泛型EventHandler依赖System命名空间，参数为自定义的事件参数类
    event EventHandler<OnProgressChangedEventArgs> OnProgressChanged;

    // 嵌套的事件参数类（继承自EventArgs，符合.NET事件规范）
    public class OnProgressChangedEventArgs : EventArgs
    {
        // 标准化进度值（0~1）
        public float progressNormalized;
    }
}
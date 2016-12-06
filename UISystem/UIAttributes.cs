using System;

namespace Arthas.Client.UI
{
    /// <summary>
    /// 顶层窗口特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIHeaderAttribute : Attribute
    {
        /// <summary>
        /// 是否为独占窗口,是则隐藏其他窗口
        /// </summary>
        public bool Exclusive = true;

        /// <summary>
        /// 是否忽略 隐藏或者显示事件
        /// </summary>
        public bool AlwaysShow = false;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class UIOrderAttribute : Attribute
    {
        /// <summary>
        /// UI顺序的索引
        /// </summary>
        public byte OrderIndex = 0;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIStartAttribute : Attribute { }
}
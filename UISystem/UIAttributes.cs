using System;

namespace Arthas.Client.UI
{
    /// <summary>
    /// 独占窗口
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIExclusiveAttribute : Attribute {}

    /// <summary>
    /// 顶层窗口
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIHeaderAttribute : Attribute {}

    /// <summary>
    /// 排序
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class UIOrderAttribute : Attribute
    {
        /// <summary>
        /// UI顺序的索引
        /// </summary>
        public byte OrderIndex = 0;
    }

    /// <summary>
    /// 开始窗口
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIStartAttribute : Attribute {}
}
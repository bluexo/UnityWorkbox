using System;

namespace Arthas.Client.UI
{
    /// <summary>
    /// 顶层窗口特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIHeaderAttribute : Attribute {     }

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
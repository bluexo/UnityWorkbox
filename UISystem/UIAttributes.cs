using System;

namespace Arthas.Client.UI
{
    /// <summary>
    /// 独占窗口
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIExclusiveAttribute : Attribute { }

    /// <summary>
    /// 通用顶层UI
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIOrderAttribute : Attribute
    {
        public byte OrderIndex { get; set; }
    }

    /// <summary>
    /// 通用顶层UI
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIHeaderAttribute : Attribute { }

    /// <summary>
    /// 开始窗口
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIStartAttribute : Attribute { }
}
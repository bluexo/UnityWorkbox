using System;
using System.Collections.Generic;

namespace UnityWorkbox.UI
{
    /// <summary>
    /// When Exclusive shown will be hide other windows (exclusive of UIHeader)
    /// 当独占窗口显示时将会隐藏其他窗口 (不包括顶层窗口)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIExclusiveAttribute : Attribute { }

    /// <summary>
    /// Specify UIOrder.SortOrder for window , than window will be show order by SortOrder All ui has a default SortOrder = 0 
    /// 为窗口指定排列顺序，将会按照指定的顺序显示， 所有的UI都有一个默认排序为 0 , 这里的Order会覆盖在编辑器中指定的Order
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIOrderAttribute : Attribute
    {
        public UIOrderAttribute() { }
        public UIOrderAttribute(int order) { SortOrder = order; }
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Anyway the UIHeader window always show on top , if has multiple UIheader window , order by <see cref="UIOrder.SortOrder"/> 
    /// 任何时候,顶层窗口都会显示在最前面，如果有多个顶层窗口，则按照 UIOrder.SortOrder 依次显示
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIFloatingAttribute : Attribute { }

    /// <summary>
    ///  Anyway , mustbe has a StartUI as First shown window 
    ///  任何时候都必须要有一个开始窗口来第一个显示
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIStartAttribute : Attribute { }

    /// <summary>
    /// When parent window shown , will be show its all child windows
    /// 当一个父窗口显示时，将会显示所有的子窗口
    /// </summary>
    public class UIChildAttribute : Attribute
    {
        public List<Type> Childs { get; private set; }

        public UIChildAttribute(params Type[] names)
        {
            Childs = new List<Type>(names);
        }
    }
}
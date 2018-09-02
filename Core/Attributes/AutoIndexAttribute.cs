using System;
using UnityEngine;

namespace Arthas
{
    public class AutoIndexAttribute : PropertyAttribute
    {
        public int StartIndex { get; set; }
    }
}

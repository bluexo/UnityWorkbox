using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arthas.Common
{
    [DisallowMultipleComponent]
    public abstract class BaseScriptableInvoker : MonoBehaviour
    {
        public abstract void Initialize();

        public abstract object[] InvokeScript(string funcName, params object[] parameters);

        public abstract void Dispose();
    }
}
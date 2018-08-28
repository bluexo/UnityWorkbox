using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arthas.Common
{
    [DisallowMultipleComponent]
    public abstract class BaseScriptableInvoker : MonoBehaviour
    {
        public abstract void Initialize();

        public virtual object InvokeScript(string funcName, params object[] parameters)
        {
            throw new System.NotImplementedException();
        }

        public abstract void Dispose();
    }
}
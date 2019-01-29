using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arthas.Common
{
    [DisallowMultipleComponent]
    public abstract class BaseScriptableInvoker : MonoBehaviour
    {
        public abstract void Initialize();

        public virtual void Invoke(string funcName)
        {
            throw new System.NotImplementedException();
        }

        public virtual object Invoke(string funcName, params object[] parameters)
        {
            throw new System.NotImplementedException();
        }

        public virtual object InvokeStatic(string funcName, params object[] param)
        {
            throw new System.NotImplementedException();
        }

        public abstract void Dispose();
    }
}
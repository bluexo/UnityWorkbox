using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if USE_ILRUNTIME

using ILRuntime;
using ILRuntime.Runtime;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Reflection;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Stack;
using ILRuntime.CLR.Utils;
using ILRAppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

using Arthas;

public static class VisualConfigILRuntimeRedirection
{
    public unsafe static void Register(ILRAppDomain appDomain)
    {
        var methods = typeof(GeneralVisualConfig).GetMethods();
        foreach (var m in methods)
        {
            var parameters = m.GetParameters();
            if (m.Name.Equals("GetItems")
                && m.IsGenericMethod
                && m.IsPublic)
            {
                appDomain.RegisterCLRMethodRedirection(m, GetItemsInternal);
                break;
            }
        }
    }

    private static unsafe StackObject* GetItemsInternal(ILIntepreter intp,
        StackObject* esp,
        IList<object> mStack,
        CLRMethod method,
        bool isNewObj)
    {
        var type = method.GenericArguments[0].ReflectionType as ILRuntimeType;
        var fields = type.GetFields();
        var config = mStack.First() as GeneralVisualConfig;
        var array = new object[config.Items.Length];
        for (var i = 0; i < config.Items.Length; i++)
        {
            var instance = type.ILType.Instantiate();
            var item = config.Items[i];
            for (var j = 0; j < fields.Length; j++)
            {
                var field = fields[j];
                if (!item.fields.ContainsKey(field.Name)) continue;
                var obj = item.fields[field.Name].GetObject();
                field.SetValue(instance, obj);
            }
            array[i] = instance;
        }
        return ILIntepreter.PushObject(esp, mStack, array);
    }
}

#endif
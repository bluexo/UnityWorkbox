﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arthas
{
    public static class ReflectionExtensions
    {
        public static Type[] GetAllRuntimeTypes()
        {
            var uiTypes = new List<Type>();
#if UNITY_2017_3_OR_NEWER && UNITY_EDITOR
            var uasms = UnityEditor.Compilation.CompilationPipeline.GetAssemblies();
            foreach (var uasm in uasms)
            {
                var dllPath = uasm.outputPath.ToSystemPath();
                var dll = System.Reflection.Assembly.LoadFile(dllPath);
                uiTypes.AddRange(dll.GetTypes());
            }
            return uiTypes.ToArray();
#else
            return typeof(ReflectionExtensions).Assembly.GetTypes();
#endif
        }
    }
}

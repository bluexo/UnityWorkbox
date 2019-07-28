using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityWorkbox
{

    public static class UnityEditorUtility
    {
        public const string RuntimeObjRefNamePrefix = "PPtr<$";

        public static string TrimPointerName(string pointerName)
        {
            if (!pointerName.Contains(RuntimeObjRefNamePrefix)) return pointerName;
            return pointerName.Substring(RuntimeObjRefNamePrefix.Length).TrimEnd('>');
        }
    }
}

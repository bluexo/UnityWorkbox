using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityEditorExtension
{
    public static void SetPosition(this Transform current, float? x = null, float? y = null, float? z = null)
    {
        current.position = new Vector3(x ?? 0, y ?? 0, z ?? 0);
    }

    public static void AddPosition(this Transform current, float? x = null, float? y = null, float? z = null)
    {
        current.position += new Vector3(x ?? 0, y ?? 0, z ?? 0);
    }
}

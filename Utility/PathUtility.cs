using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arthas
{
    public static class PathUtility
    {
        readonly static string ProjectPath = Application.dataPath.Replace("Assets", "").TrimEnd('/').TrimEnd('\\');

        public static string ToSystemPath(this string orgin)
        {
            var newPath = orgin.ToString();
            if (!newPath.Contains("Assets/")
                && !newPath.Contains("/Assets")
                && !newPath.Contains("\\Assets")
                && !newPath.Contains("Assets\\"))
                throw new UnassignedReferenceException("Invalid system path !!!");
            newPath = ProjectPath + "/" + newPath;
            return newPath;
        }

        public static string ToAssetsPath(this string orgin)
        {
            var newPath = orgin.ToString();
            newPath.Replace(ProjectPath, "");
            newPath.TrimStart('\\').TrimStart('/');
            return newPath;
        }
    }
}

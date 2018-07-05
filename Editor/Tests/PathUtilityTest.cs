using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace Arthas
{
    public class PathUtilityTest
    {

        [Test]
        public void PathUtilityTestSimplePasses()
        {
            // Use the Assert class to test conditions.
            var testPath = "Resources/Configs/";
            var assetPath = "Assets/" + testPath;
            var systemPath = Application.dataPath + "/" + testPath;

            Assert.AreEqual(assetPath, systemPath.ToAssetsPath());
            Assert.AreEqual(systemPath, assetPath.ToSystemPath());
        }

        // A UnityTest behaves like a coroutine in PlayMode
        // and allows you to yield null to skip a frame in EditMode
        [UnityTest]
        public IEnumerator PathUtilityTestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // yield to skip a frame
            yield return null;
        }
    }
}

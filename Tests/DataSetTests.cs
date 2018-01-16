using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace Arthas.Tests
{

    public class DataSetTests
    {

        [Test]
        public void DataSetTestsSimplePasses()
        {

        }


        [UnityTest]
        public IEnumerator DataSetTestsWithEnumeratorPasses()
        {
            var file = Application.persistentDataPath + "wtf.txt";
            yield return null;
        }
    }
}

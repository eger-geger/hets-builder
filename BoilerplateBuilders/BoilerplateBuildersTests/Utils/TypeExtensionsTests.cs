using System;
using System.Collections;
using System.Collections.Generic;
using BoilerplateBuilders.Utils;
using NUnit.Framework;

namespace BoilerplateBuildersTests.Utils
{
    public class TypeExtensionsTests
    {
        [TestCase(typeof(object[]))]
        [TestCase(typeof(List<string>))]
        [TestCase(typeof(IList<string>))]
        [TestCase(typeof(Dictionary<string, string>))]
        public void IsAssignableToEnumerableTests(Type type)
        {
            Assert.IsTrue(type.IsCollection());
        }

        [TestCase(typeof(ISet<string>))]
        [TestCase(typeof(HashSet<string>))]
        public void IsAssignableToSetTests(Type type)
        {
            Assert.IsTrue(type.IsGenericSet());
        }
        
    }
}
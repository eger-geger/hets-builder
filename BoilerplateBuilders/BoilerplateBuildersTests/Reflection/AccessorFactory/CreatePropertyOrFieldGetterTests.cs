using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BoilerplateBuildersTests.Models;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using static BoilerplateBuilders.Reflection.AccessorFactory;

namespace BoilerplateBuildersTests.Reflection.AccessorFactory
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "ArgumentsStyleLiteral")]
    public class AccessorFactoryTests
    {

        private static IEnumerable<ITestCaseData> CreateGetterCases
        {
            get
            {
                yield return new TestCaseData(
                    new Account(id: 42), 
                    typeof(int), 
                    nameof(Account.Id)
                ).Returns(42);
                
                yield return new TestCaseData(
                    new Account(name: "Jack"), 
                    typeof(string), 
                    nameof(Account.Name)
                ).Returns("Jack");
                
                yield return new TestCaseData(
                    new Account(name: "Jack"), 
                    typeof(string), 
                    nameof(Account.Name).ToLower()
                ).Returns("Jack");
                
                yield return new TestCaseData(
                    new Account(name: "Jack"), 
                    typeof(string), 
                    nameof(Account.Name).ToUpper()
                ).Returns("Jack");
                
                yield return new TestCaseData(
                    new Account(), 
                    typeof(string), 
                    nameof(Account.Name)
                ).Returns(null);
            }
        }
        
        [TestCaseSource(nameof(CreateGetterCases))]
        public object ShouldCreateGetter(object target, Type memberType, string memberName)
        {
            return CreatePropertyOrFieldGetter(target.GetType(), memberType, memberName)(target);
        }

        private static IEnumerable<ITestCaseData> InvalidMemberTestCases
        {
            get
            {
                yield return new TestCaseData(typeof(Account), typeof(int), nameof(Account.Name));
                yield return new TestCaseData(typeof(Account), typeof(decimal), nameof(Account.Id));
                yield return new TestCaseData(typeof(Account), typeof(string[]), "Addresses");
                yield return new TestCaseData(typeof(Account), typeof(int), "");
                yield return new TestCaseData(typeof(Account), typeof(int), null);
            }
        }

        [TestCaseSource(nameof(InvalidMemberTestCases))]
        public void ShouldRiseArgumentException(Type targetType, Type memberType, string memberName)
        {
            Assert.Throws<ArgumentException>(() => CreatePropertyOrFieldGetter(targetType, memberType, memberName));
        }

        private static IEnumerable<ITestCaseData> NullArgumentTestCases
        {
            get
            {
                yield return new TestCaseData(typeof(Account), null, nameof(Account.Name));
                yield return new TestCaseData(null, typeof(string), nameof(Account.Name));
            }
        }
        
        [TestCaseSource(nameof(NullArgumentTestCases))]
        public void ShouldRiseNullArgumentException(
            Type targetType, Type memberType, string memberName
        )
        {
            Assert.Throws<ArgumentNullException>(() => CreatePropertyOrFieldGetter(targetType, memberType, memberName));
        }
    }
}
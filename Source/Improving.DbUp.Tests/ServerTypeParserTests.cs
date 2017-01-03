using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Improving.DbUp.Tests
{
    [TestClass]
    public class ServerTypeParserTests
    {
        [TestMethod]
        public void NullShouldBeUndefined()
        {
            AssertStringParsesAsExpected(null, ServerType.Undefined);
        }
        
        [TestMethod]
        public void EmptyStringShouldBeUndefined()
        {
            AssertStringParsesAsExpected(string.Empty, ServerType.Undefined);
        }

        [TestMethod]
        public void DeveloperShouldBeDeveloper()
        {
            AssertStringParsesAsExpected("Developer", ServerType.Developer);
        }

        [TestMethod]
        public void ShouldNotBeCaseSensitive()
        {
            AssertStringParsesAsExpected("developer", ServerType.Developer);
        }

        private void AssertStringParsesAsExpected(string value, ServerType expected)
        {
            var actual = ServerTypeParser.Parse(value);
            Assert.AreEqual(expected, actual);
        }
    }

    
}

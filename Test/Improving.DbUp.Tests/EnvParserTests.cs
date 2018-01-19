using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Improving.DbUp.Tests
{
    [TestClass]
    public class EnvParserTests
    {
        [TestMethod]
        public void NullShouldBeUndefined()
        {
            AssertStringParsesAsExpected(null, Env.Undefined);
        }
        
        [TestMethod]
        public void EmptyStringShouldBeProd()
        {
            AssertStringParsesAsExpected(string.Empty, Env.Prod);
        }

        [TestMethod]
        public void PRODShouldBePROD()
        {
            AssertStringParsesAsExpected("PROD", Env.Prod);
        }
        
        [TestMethod]
        public void DEVShouldBeDEV()
        {
            AssertStringParsesAsExpected("DEV", Env.DEV);
        }

        [TestMethod]
        public void ShouldNotBeCaseSensitive()
        {
            AssertStringParsesAsExpected("dev", Env.DEV);
        }

        private void AssertStringParsesAsExpected(string value, Env expected)
        {
            var actual = EnvParser.Parse(value);
            Assert.AreEqual(expected, actual);
        }
    }

    
}

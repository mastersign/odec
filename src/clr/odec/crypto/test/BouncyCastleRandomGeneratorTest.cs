using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace de.mastersign.odec.crypto.test
{
    [TestFixture]
    class BouncyCastleRandomGeneratorTest
    {
        [Test]
        public void CreateRandomGeneratorTest()
        {
            var rng = new BouncyCastleRandomGenerator();
        }

        [Test]
        public void GenerateRandomDataTest()
        {
            IRandomGeneratorTest.GenerateRandomDataTest(new BouncyCastleRandomGenerator());
        }

        [Test]
        public void AutoInitializationTest()
        {
            var data1 = new byte[64];
            var data2 = new byte[64];
            var rng1 = new BouncyCastleRandomGenerator();
            var rng2 = new BouncyCastleRandomGenerator();
            rng1.GenerateRandomData(data1);
            rng2.GenerateRandomData(data2);
            Assert.That(data1, Is.Not.EqualTo(data2));
        }
    }
}

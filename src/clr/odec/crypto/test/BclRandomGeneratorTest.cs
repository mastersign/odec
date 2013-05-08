﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace de.mastersign.odec.crypto.test
{
    [TestFixture]
    class BclRandomGeneratorTest
    {
        [Test]
        public void CreateRandomGeneratorTest()
        {
            var rng = new BclRandomGenerator();
        }

        [Test]
        public void GenerateRandomDataTest()
        {
            IRandomGeneratorTest.GenerateRandomDataTest(new BclRandomGenerator());
        }

        [Test]
        public void AutoInitializationTest()
        {
            var data1 = new byte[64];
            var data2 = new byte[64];
            var rng1 = new BclRandomGenerator();
            var rng2 = new BclRandomGenerator();
            rng1.GenerateRandomData(data1);
            rng2.GenerateRandomData(data2);
            Assert.That(data1, Is.Not.EqualTo(data2));
        }
    }
}

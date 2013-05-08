using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace de.mastersign.odec.crypto.test
{
    internal static class IRandomGeneratorTest
    {
        public static void GenerateRandomDataTest(IRandomGenerator rng)
        {
            Assert.Throws<ArgumentNullException>(
                () => rng.GenerateRandomData(null));

            var data = new byte[0];
            rng.GenerateRandomData(data);

            data = new byte[1];
            rng.GenerateRandomData(data);

            data = new byte[1025];
            rng.GenerateRandomData(data);
            Assert.IsTrue(data.Any(v => v != 0));

            var data2 = new byte[data.Length];
            rng.GenerateRandomData(data2);
            Assert.That(data2, Is.Not.EqualTo(data));
        }
    }
}

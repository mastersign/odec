using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.test.res
{
    internal static class DemoFileHashes
    {
        public static readonly byte[] SHA1 = TestHelper.HexToByteArray("123e4b5d0ff6a3eaf893b719dcfb6bbb4580f9a7");
        public static readonly byte[] MD5 = TestHelper.HexToByteArray("d7ea940cb3e94da131371a57209429af");
        public static readonly byte[] CRC32 = TestHelper.HexToByteArray("314abd53");
    }
}
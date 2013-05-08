using System.IO;

namespace de.mastersign.odec.utils
{
    internal static class StreamUtils
    {
        public static void CopyTo(this Stream source, Stream target)
        {
            int read;
            var buffer = new byte[256];
            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                target.Write(buffer, 0, read);
            }
        }

        public static byte[] ToArray(this Stream source)
        {
            using (var ms = new MemoryStream())
            {
                //source.Position = 0L;
                source.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
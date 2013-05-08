using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using de.mastersign.odec.Properties;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// This class supports the reading of PEM encoded certificate and key files
    /// in a way that the read certificate or key objects are compatible 
    /// with cryptographic providers based on the crypto library .
    /// </summary>
    public class BclKeyHelper
    {
        internal static byte[] DecodePem(string type, string pem)
        {
            var header = string.Format("-----BEGIN {0}-----", type);
            var footer = string.Format("-----END {0}-----", type);
            var start = pem.IndexOf(header) + header.Length;
            var end = pem.IndexOf(footer, start);
            if (end < 0)
            {
                throw new ArgumentException(
                    Resources.BclKeyHelper_DecodePem_ArgumentException_TypeNotFound, "type");
            }
            var base64 = pem.Substring(start, (end - start));
            return Convert.FromBase64String(base64);
        }

        internal static string EncodePem(string type, byte[] data)
        {
            var pem = Convert.ToBase64String(data, 0, data.Length, Base64FormattingOptions.InsertLineBreaks);
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("-----BEGIN {0}-----", type));
            sb.AppendLine(pem);
            sb.AppendLine(string.Format("-----END {0}-----", type));
            return sb.ToString();
        }

        /// <summary>
        /// Loads a X509 certificate file in a <see cref="X509Certificate"/> object.
        /// </summary>
        /// <param name="file">The absolute path to the certificate file.</param>
        /// <returns>The read certificate as a <see cref="X509Certificate"/> object.</returns>
        public static X509Certificate2 LoadCertificateFile(string file)
        {
            var pem = File.ReadAllText(file, Encoding.ASCII);
            return LoadCertificate(pem);
        }

        /// <summary>
        /// Loads a <see cref="X509Certificate2"/> from PEM encoded data.
        /// </summary>
        /// <param name="pemCert">The encoded certificate as a string.</param>
        /// <returns>A <see cref="X509Certificate2"/> instance.</returns>
        public static X509Certificate2 LoadCertificate(string pemCert)
        {
            var decoded = DecodePem("CERTIFICATE", pemCert);
            return new X509Certificate2(decoded);
        }

        /// <summary>
        /// Encodes the given X509 certificate in PEM style.
        /// </summary>
        /// <param name="cert">The certificate.</param>
        /// <returns>A string with the PEM encoded certificate.</returns>
        public static string EncodeCertificate(X509Certificate2 cert)
        {
            return EncodePem("CERTIFICATE", cert.Export(X509ContentType.Cert));
        }

        /// <summary>
        /// Retrieves the public key parameters from a given certificate.
        /// </summary>
        /// <param name="cert">The certificate.</param>
        /// <returns>The <see cref="RSAParameters"/> with the public key.</returns>
        public static RSAParameters GetPublicKey(X509Certificate2 cert)
        {
            var rsa = (RSACryptoServiceProvider)cert.PublicKey.Key;
            return rsa.ExportParameters(false);
        }

        /// <summary>
        /// Loads a private key file in a <see cref="RSAParameters"/> object.
        /// </summary>
        /// <param name="file">The absolute path to the key file.</param>
        /// <returns>The read key as a <see cref="RSAParameters"/> object.</returns>
        public static RSAParameters LoadPrivateKeyFile(string file)
        {
            var pem = File.ReadAllText(file, Encoding.ASCII);
            return LoadPrivateKey(pem);
        }

        /// <summary>
        /// Loads the private RSA key from PEM encoded data.
        /// </summary>
        /// <param name="pem">The encoded private key.</param>
        /// <returns>A <see cref="RSAParameters"/> instance with the private key.</returns>
        public static RSAParameters LoadPrivateKey(string pem)
        {
            var decoded = DecodePem("RSA PRIVATE KEY", pem);
            return DecodeRSAPrivateKey(decoded);
        }

        /// <summary>
        /// Parses binary ans.1 RSA private key; returns RSACryptoServiceProvider
        /// </summary>
        /// <remarks>
        /// http://www.jensign.com/opensslkey/index.html
        /// </remarks>
        /// <param name="privkey">The data of the private key.</param>
        /// <returns>The private key as <see cref="RSAParameters"/>.</returns>
        private static RSAParameters DecodeRSAPrivateKey(byte[] privkey)
        {
            byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

            // Set up stream to decode the asn.1 encoded RSA private key
            var mem = new MemoryStream(privkey);
            var binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            try
            {
                ushort twobytes;
                twobytes = binr.ReadUInt16();
                //data read as little endian order (actual data order for Sequence is 30 81)
                if (twobytes == 0x8130)
                {
                    //advance 1 byte
                    binr.ReadByte();
                }
                else if (twobytes == 0x8230)
                {
                    //advance 2 bytes
                    binr.ReadInt16();
                }
                else
                {
                    throw new FormatException();
                }

                twobytes = binr.ReadUInt16();
                // version number
                if (twobytes != 0x0102)
                {
                    throw new FormatException();
                }
                byte bt = binr.ReadByte();
                if (bt != 0x00)
                {
                    throw new FormatException();
                }

                // All private key components are Integer sequence
                int elems;
                elems = GetIntegerSize(binr);
                MODULUS = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                E = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                D = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                P = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                Q = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DP = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DQ = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                IQ = binr.ReadBytes(elems);

                // return the RSAParameters struct
                return new RSAParameters
                           {
                               Modulus = MODULUS,
                               Exponent = E,
                               D = D,
                               P = P,
                               Q = Q,
                               DP = DP,
                               DQ = DQ,
                               InverseQ = IQ
                           };
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    Resources.BclKeyHelper_DecodeRSAPrivateKey_FormatException_ErrorParsingPrivateKey, ex);
            }
            finally
            {
                binr.Close();
            }
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
            {
                //expect integer
                return 0;
            }
            bt = binr.ReadByte();

            if (bt == 0x81)
            {
                // data size in next byte
                count = binr.ReadByte();
            }
            else if (bt == 0x82)
            {
                // data size in next 2 bytes
                byte highbyte = binr.ReadByte();
                byte lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                // we already have the data size
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {	//remove high order zeros in data
                count -= 1;
            }

            //last ReadByte wasn't a removed zero, so back up a byte
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }
    }
}

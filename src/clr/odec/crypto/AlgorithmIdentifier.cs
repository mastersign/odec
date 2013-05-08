using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// A static class with constants for cryptographic algorithm identifier.
    /// </summary>
    public static class AlgorithmIdentifier
    {
        #region Hash Algorithms

        /// <summary>
        /// Identifies the MD5 hash algorithm.
        /// </summary>
        /// <seealso href="http://tools.ietf.org/html/rfc1321"/>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2001/04/xmldsig-more#md5"</c>.</remarks>
        public const string MD5 = "http://www.w3.org/2001/04/xmldsig-more#md5";

        /// <summary>
        /// Identifies the SHA-1 hash algorithm.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2000/09/xmldsig#sha1"</c>.</remarks>
        public const string SHA1 = "http://www.w3.org/2000/09/xmldsig#sha1";

        /// <summary>
        /// Identifies the SHA-224 hash algorithm.
        /// </summary>
        /// <seealso href="http://tools.ietf.org/html/rfc3874"/>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2001/04/xmldsig-more#sha224"</c>.</remarks>
        public const string SHA224 = "http://www.w3.org/2001/04/xmldsig-more#sha224";

        /// <summary>
        /// Identifies the SHA-256 hash algorithm.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2001/04/xmlenc#sha256"</c>.</remarks>
        public const string SHA256 = "http://www.w3.org/2001/04/xmlenc#sha256";

        /// <summary>
        /// Identifies the SHA-384 hash algorithm.
        /// </summary>
        /// <seealso href="http://tools.ietf.org/html/rfc4051#ref-FIPS-180-2"/>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2001/04/xmldsig-more#sha384"</c>.</remarks>
        public const string SHA384 = "http://www.w3.org/2001/04/xmldsig-more#sha384";

        /// <summary>
        /// Identifies the SHA-512 hash algorithm.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2001/04/xmlenc#sha512"</c>.</remarks>
        public const string SHA512 = "http://www.w3.org/2001/04/xmlenc#sha512";

        /// <summary>
        /// Identifies the Whirlpool hash algorithm.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2007/05/xmldsig-more#whirlpool"</c>.</remarks>
        public const string WHIRLPOOL = "http://www.w3.org/2007/05/xmldsig-more#whirlpool";

        /// <summary>
        /// Identifies the RipeMD-160 hash algorithm.
        /// </summary>
        /// <remarks>
        /// The identifying string is <c>"X-RipeMD-160"</c>.
        /// This string is not standardized.
        /// </remarks>
        public const string RIPEMD160 = "X-RipeMD-160";

        /// <summary>
        /// Identifies the RipeMD-256 hash algorithm.
        /// </summary>
        /// <remarks>
        /// The identifying string is <c>"X-RipeMD-256"</c>.
        /// This string is not standardized.
        /// </remarks>
        public const string RIPEMD256 = "X-RipeMD-256";

        /// <summary>
        /// Identifies the RipeMD-320 hash algorithm.
        /// </summary>
        /// <remarks>
        /// The identifying string is <c>"X-RipeMD-320"</c>.
        /// This string is not standardized.
        /// </remarks>
        public const string RIPEMD320 = "X-RipeMD-320";

        #endregion

        #region Signature Algorithms

        //public const string DSA = "X-DSA";

        //public const string ECDSA = "X-ECDSA";

        //public const string RSA = "X-RSA";

        /// <summary>
        /// Identifies the signature algorithm combination of DSA and SHA-1.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2000/09/xmldsig#dsa-sha1"</c>.</remarks>
        public const string DSA_SHA1 = "http://www.w3.org/2000/09/xmldsig#dsa-sha1";

        /// <summary>
        /// Identifies the signature algorithm combination of eliptic curve DSA and SHA-1.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha1"</c>.</remarks>
        public const string ECDSA_SHA1 = "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha1";

        /// <summary>
        /// Identifies the signature algorithm combination of eliptic curve DSA and SHA-224.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha224"</c>.</remarks>
        public const string ECDSA_SHA224 = "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha224";

        /// <summary>
        /// Identifies the signature algorithm combination of eliptic curve DSA and SHA-256.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha265"</c>.</remarks>
        public const string ECDSA_SHA265 = "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha265";

        /// <summary>
        /// Identifies the signature algorithm combination of eliptic curve DSA and SHA-384.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha384"</c>.</remarks>
        public const string ECDSA_SHA384 = "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha384";

        /// <summary>
        /// Identifies the signature algorithm combination of eliptic curve DSA and SHA-512.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha512"</c>.</remarks>
        public const string ECDSA_SHA512 = "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha512";
        
        /// <summary>
        /// Identifies the signature algorithm combination of RSA and MD5.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2001/04/xmldsig-more#rsa-md5"</c>.</remarks>
        public const string RSA_MD5 = "http://www.w3.org/2001/04/xmldsig-more#rsa-md5";

        /// <summary>
        /// Identifies the signature algorithm combination of RSA and SHA-1.
        /// This is the signature scheme of PKCS #1 (RFC2437).
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2000/09/xmldsig#rsa-sha1"</c>.</remarks>
        public const string RSA_SHA1 = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";

        /// <summary>
        /// Identifies the signature algorithm combination of RSA and SHA-256.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"</c>.</remarks>
        public const string RSA_SHA256 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

        /// <summary>
        /// Identifies the signature algorithm combination of RSA and SHA-384.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2001/04/xmldsig-more#rsa-sha384"</c>.</remarks>
        public const string RSA_SHA384 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384";

        /// <summary>
        /// Identifies the signature algorithm combination of RSA and SHA-512.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2001/04/xmldsig-more#rsa-sha512"</c>.</remarks>
        public const string RSA_SHA512 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512";

        /// <summary>
        /// Identifies the signature algorithm combination of RSA and Wirlpool.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2007/05/xmldsig-more#rsa-whirlpool"</c>.</remarks>
        public const string RSA_WHIRLPOOL = "http://www.w3.org/2007/05/xmldsig-more#rsa-whirlpool";

        /// <summary>
        /// Identifies the signature algorithm combination of RSA and RIPEMD160.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2001/04/xmldsig-more/rsa-ripemd160"</c>.</remarks>
        public const string RSA_RIPEMD160 = "http://www.w3.org/2001/04/xmldsig-more/rsa-ripemd160";

        #endregion

        #region Canonicalization Algorithms

        /// <summary>
        /// Identifies the C14N XML canonicalization algorithm version 1.0 without comments.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/TR/2001/REC-xml-c14n-20010315"</c>.</remarks>
        public const string C14N = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";

        /// <summary>
        /// Identifies the C14N XML canonicalization algorithm version 1.1 without comments.
        /// </summary>
        /// <remarks>The identifying string is <c>"http://www.w3.org/2006/12/xml-c14n11"</c>.</remarks>
        public const string C14N11 = "http://www.w3.org/2006/12/xml-c14n11";
    
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// A data structre to store meta-data about a certificate.
    /// </summary>
    public class CertificateInfo
    {
        /// <summary>
        /// Gets or sets the distiguished name of the subject.
        /// </summary>
        /// <value>The distiguished name of the subject.</value>
        public string SubjectDistinguishedName { get; set; }

        /// <summary>
        /// Gets or sets the distingushed name of the issuer.
        /// </summary>
        /// <value>The distingushed name of the issuer.</value>
        public string IssuerDistingushedName { get; set; }

        /// <summary>
        /// Gets or sets the serial number.
        /// </summary>
        /// <value>The serial number.</value>
        public string SerialNumber { get; set; }

        /// <summary>
        /// Gets or sets the signature algorithm.
        /// </summary>
        /// <value>The signature algorithm.</value>
        public string SignatureAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the earliest time since the certificate is valid.
        /// </summary>
        /// <value>The earliest valid time.</value>
        public DateTime NotBefore { get; set; }

        /// <summary>
        /// Gets or sets the latest time until the certificate is valid.
        /// </summary>
        /// <value>The latest valid time.</value>
        public DateTime NotAfter { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="linePrefix">The line prefix.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string linePrefix)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0}Subject Name:        {1}", linePrefix, SubjectDistinguishedName));
            sb.AppendLine(string.Format("{0}Issuer Name:         {1}", linePrefix, IssuerDistingushedName));
            sb.AppendLine(string.Format("{0}Serial Number:       {1}", linePrefix, SerialNumber));
            sb.AppendLine(string.Format("{0}Signature Algorithm: {1}", linePrefix, SignatureAlgorithm));
            sb.AppendLine(string.Format("{0}Version:             {1}", linePrefix, Version));
            sb.AppendLine(string.Format("{0}Valid From:          {1}", linePrefix, NotBefore));
            sb.AppendLine(string.Format("{0}Valid Until:         {1}", linePrefix, NotAfter));

            return sb.ToString();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString("");
        }
    }
}

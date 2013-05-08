using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.crypto
{
    /// <summary>
    /// Settings to specify the validation process of certificates.
    /// </summary>
    public class CertificateValidationRules
    {
        /// <summary>
        /// Gets or sets the date validation scheme for chain of certificates.
        /// </summary>
        public DateValidationScheme DateValidationScheme { get; set; }

        /// <summary>
        /// Gets or sets a value defining if a self signed certificate is valid.
        /// <c>true</c>, if a self signed certificate is valid; otherwise <c>false</c>.
        /// </summary>
        public bool AllowSelfSignedCertificate { get; set; }

        /// <summary>
        /// Gets a new instance of <see cref="CertificateValidationRules"/> 
        /// with the following default settings:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Setting</term>
        ///         <description>Value</description>
        ///     </listheader>
        ///     <item>
        ///         <term><see cref="DateValidationScheme"/></term>
        ///         <description><see cref="crypto.DateValidationScheme.ModifiedPemShell"/></description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="AllowSelfSignedCertificate"/></term>
        ///         <description><c>true</c></description>
        ///     </item>
        /// </list>
        /// </summary>
        public static CertificateValidationRules Default
        {
            get
            {
                return new CertificateValidationRules
                    {
                        DateValidationScheme = DateValidationScheme.ModifiedPemShell,
                        AllowSelfSignedCertificate = true,
                    };
            }
        }
    }

    /// <summary>
    /// The enumeration of possible date validation schemes for certificate chains.
    /// </summary>
    /// <remarks>http://www.informatik.tu-darmstadt.de/BS/Lehre/Sem98_99/T11/#tth_sEc4.2</remarks>
    public enum DateValidationScheme
    {
        /// <summary>
        /// The PEM shell scheme requires all participating certificates to be valid in the moment of the validation.
        /// </summary>
        /// <remarks>This scheme is conform to the RFC1422.</remarks>
        PemShell,

        /// <summary>
        /// The modified PEM shell scheme requires all participating certificates to be valid in the moment of
        /// using the leaf certificate (signing the data).
        /// </summary>
        ModifiedPemShell,

        /// <summary>
        /// The chain scheme requires a signing certificate to be valid in the moment of using it (signing another certificate
        /// or in case of the leaf certificate signing the data).
        /// </summary>
        Chain,
    }
}

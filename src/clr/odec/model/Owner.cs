using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using de.mastersign.odec.Properties;
using de.mastersign.odec.utils;

namespace de.mastersign.odec.model
{
    /// <summary>
    /// This class is representing the XML type, describing the owner of an edition.
    /// The name of the XML type is <c>OWNER</c> and it resides in the namespace <see cref="Model.ContainerNamespace"/>.
    /// </summary>
    public class Owner : IXmlStorable, IEquatable<Owner>
    {
        #region Static

        /// <summary>
        /// Creates an owner.
        /// </summary>
        /// <param name="instituteName">Name of the institute.</param>
        /// <param name="operatorName">Name of the operator.</param>
        /// <param name="emailContact">An email contact.</param>
        /// <returns>An <see cref="Owner"/> object.</returns>
        /// <exception cref="ArgumentNullException">
        ///     if <paramref name="instituteName"/> or <paramref name="operatorName"/> 
        ///     or <paramref name="emailContact"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     if <paramref name="instituteName"/> or <paramref name="operatorName"/> 
        ///     or <paramref name="emailContact"/> is empty or whitespace.
        /// </exception>
        public static Owner Create(string instituteName, string operatorName, string emailContact)
        {
            if (instituteName == null) throw new ArgumentNullException("instituteName");
            if (operatorName == null) throw new ArgumentNullException("operatorName");
            if (emailContact == null) throw new ArgumentNullException("emailContact");
            if (string.IsNullOrWhiteSpace(instituteName))
            {
                throw new ArgumentException(Resources.ArgumentException_EmptyString, "instituteName");
            }
            if (string.IsNullOrWhiteSpace(operatorName))
            {
                throw new ArgumentException(Resources.ArgumentException_EmptyString, "operatorName");
            }
            if (string.IsNullOrWhiteSpace(emailContact))
            {
                throw new ArgumentException(Resources.ArgumentException_EmptyString, "emailContact");
            }
            return new Owner { Institute = instituteName, Operator = operatorName, Email = emailContact };
        }

        #endregion

        /// <summary>
        /// Gets or sets the name of the institute.
        /// </summary>
        public string Institute { get; set; }

        /// <summary>
        /// Gets or sets the name of the operator.
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// Gets or sets the role of the operator.
        /// </summary>
        /// <value>The role of the operator or <c>null</c>.</value>
        public string Role { get; set; }

        /// <summary>
        /// Gets or sets an email address as contact to the operator.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the PEM encoded X509 certificate as a string, 
        /// which can be used to verify the signatures attached, while
        /// creating the associated edition, including the master signature.
        /// </summary>
        public string X509Certificate { get; set; }

        /// <summary>
        /// Checks the validity of this instance.
        /// The given <see cref="ValidationHandler"/> is called while the validation process,
        /// to display errors, warnings and informal messages.
        /// </summary>
        /// <param name="messageHandler">A message handler.</param>
        /// <returns><c>true</c> if this instance is valid; otherwise <c>false</c>.</returns>
        public bool Validate(ValidationHandler messageHandler)
        {
            var result = true;
            if (string.IsNullOrWhiteSpace(Institute))
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure, 
                    Resources.Owner_Validate_NoInstitute);
                result = false;
            }
            if (string.IsNullOrWhiteSpace(Operator))
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure, 
                    Resources.Owner_Validate_NoOperator);
                result = false;
            }
            if (string.IsNullOrWhiteSpace(Email))
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure, 
                    Resources.Owner_Validate_NoEmail);
                result = false;
            }
            if (string.IsNullOrWhiteSpace(X509Certificate))
            {
                messageHandler.Error(ValidationMessageClass.ContainerStructure, 
                    Resources.Owner_Validate_NoCertificate);
                result = false;
            }

            if (result)
            {
                messageHandler.Success(ValidationMessageClass.ContainerStructure, 
                    Resources.Owner_Validate_ValidStructure, Institute + ", " + Operator);
            }

            return result;
        }

        #region Implementation of IXmlStorable

        /// <summary>
        /// Loads the state of the object from an XML source.
        /// </summary>
        /// <param name="e">The <see cref="XmlElement"/> used as source.</param>
        public void ReadFromXml(XmlNode e)
        {
            if (e == null) throw new ArgumentNullException("e");

            Institute = e.ReadElementString("c:Institute", Institute);
            Operator = e.ReadElementString("c:Operator", Operator);
            Role = e.ReadElementString("c:Role", null);
            Email = e.ReadElementString("c:Email", Email);
            X509Certificate = e.ReadElementString("c:X509Certificate", X509Certificate);
        }

        /// <summary>
        /// Writes the state of the object to an XML target.
        /// </summary>
        /// <param name="w">The <see cref="XmlWriter"/> used as target.</param>
        public void WriteToXml(XmlWriter w)
        {
            if (w == null) throw new ArgumentNullException("w");
            if (!IsValid)
            {
                throw new InvalidOperationException(Resources.XmlStorable_WriteToXml_ThisInstanceIsInvalid);
            }

            w.WriteElementString("Institute", Model.ContainerNamespace, Institute);
            w.WriteElementString("Operator", Model.ContainerNamespace, Operator);
            if (Role != null)
            {
                w.WriteElementString("Role", Model.ContainerNamespace, Role);
            }
            w.WriteElementString("Email", Model.ContainerNamespace, Email);
            w.WriteElementString("X509Certificate", Model.ContainerNamespace, X509Certificate);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// The instance is valid, if a call to <see cref="WriteToXml"/>
        /// produces schema conform XML.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid
        {
            get
            {
                return
                    !string.IsNullOrWhiteSpace(Institute) &&
                    !string.IsNullOrWhiteSpace(Operator) &&
                    !string.IsNullOrWhiteSpace(Email) &&
                    !string.IsNullOrWhiteSpace(X509Certificate);
            }
        }

        #endregion

        #region Implementation of IEquatable<OwnerType>

        /// <summary>
        /// Indicates whether the current object 
        /// is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(Owner other)
        {
            if (other == null) return false;
            if (!string.Equals(Institute, other.Institute)) return false;
            if (!string.Equals(Operator, other.Operator)) return false;
            if (!string.Equals(Role, other.Role)) return false;
            if (!string.Equals(Email, other.Email)) return false;
            if (!string.Equals(X509Certificate, other.X509Certificate)) return false;
            return true;
        }

        /// <summary>
        /// Determines wether the specified <see cref="Object"/> instances are considered equal.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Object"/>. </param>
        /// <returns><c>true</c> if the specified Object is equal to the current Object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is Owner && Equals((Owner)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>A hash code for the current <see cref="Object"/>.</returns>
        public override int GetHashCode()
        {
            return 7
                   * (Institute != null ? Institute.GetHashCode() + 23 : 1)
                   * (Operator != null ? Operator.GetHashCode() + 23 : 1)
                   * (Role != null ? Role.GetHashCode() + 23 : 1)
                   * (Email != null ? Email.GetHashCode() + 23 : 1)
                   * (X509Certificate != null ? X509Certificate.GetHashCode() + 23: 1);
        }

        #endregion

    }
}
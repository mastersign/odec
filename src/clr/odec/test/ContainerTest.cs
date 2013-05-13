using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using de.mastersign.odec.crypto;
using de.mastersign.odec.model;
using NUnit.Framework;

namespace de.mastersign.odec.test
{
    [TestFixture]
    internal class ContainerTest : AssertionHelper
    {
        [Test, Category("Workflow")]
        public void CompleteWorkflowDefault()
        {
            CompleteWorkflow(CompatibilityFlags.DefaultFlags);
        }

        [Test, Category("Workflow")]
        public void CompleteWorkflowCompatibilityMode()
        {
            var cf = new CompatibilityFlags
                         {
                             SuppressStructureXmlCanonicalization = true,
                             WriteXmlSignatureCanonicalized = true,
                         };
            CompleteWorkflow(cf);
        }

        private void CompleteWorkflow(CompatibilityFlags compatibilityFlags)
        {
            // Specify the ZIP file for the container
            var targetPath = Path.Combine(
                Path.GetTempPath(),
                "ContainerTestWorkflow_" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".zip");

            // Create some temporary random files for values
            var tmpFile0 = TestHelper.CreateTempRandomFile(256); // big file, used twice
            var tmpFile1 = TestHelper.CreateTempRandomFile(10);
            var tmpFile2 = TestHelper.CreateTempRandomFile(10);

            // Identify the owner
            var owner = Owner.Create(
                "Forensic Corporation",
                "John Smith",
                "john.smith@forensic.com");

            // Identify the initial edition
            var edition = EditionElement.Create(
                "TestBench", "Unknown", "1.0", owner,
                "Copyright © Forensic Coporation. All rights reserved.",
                null);

            // Loading cryptographic information
            var ca = Configuration.CryptoFactory.CreateRSAProviderFromCertificateFile(
                TestHelper.GetResFilePath("ca.crt"));
            var privateKey = Configuration.CryptoFactory.CreateRSAProviderFromPrivateKeyFile(
                TestHelper.GetResFilePath("Test.key"), null);
            var certificate = Configuration.CryptoFactory.CreateRSAProviderFromCertificateFile(
                TestHelper.GetResFilePath("Test.crt"));

            // Start the initialization of the new container
            var container = Container.CreateZip(
                targetPath, edition, null, privateKey, certificate, compatibilityFlags);

            // Describe the random generator as provenance
            var randProvenance = new ProvenanceElement { Guid = new Guid("5e751295-1728-445a-96c7-2c64a9d5a1d7") };

            // Create the first entity
            var entity0 = container.NewEntity(
                new Guid("c3f3e35a-7432-4655-8343-0c76e2473695"),
                randProvenance, "base");

            // Add a value to the first entity
            entity0.AddValue(
                "testvalue",
                new Guid("f1a9aaa0-cb36-417b-935c-7e5ddd28cade"),
                ValueAppearance.plain,
                tmpFile0);

            // Write the entity with all values to the container
            entity0.Close();

            // Seal the container with a master signature
            container.FinishInitialization();

            // Dispose the container handle
            container.Dispose();

            // Validate the container
            Console.WriteLine("### First Validation:");
            container = Container.OpenZip(targetPath, Console.WriteLine, compatibilityFlags);
            Expect(container.VerifyEntityValueSignatures(Console.WriteLine));
            Expect(container.ValidateCertificates(
                new CertificationAuthorityDirectory(ca),
                new CertificateValidationRules { AllowSelfSignedCertificate = true },
                Console.WriteLine));

            // Identify the second edition
            var secondEdition = EditionElement.Create(
                "TestBench", "Unknown", "1.0", owner,
                "Open Source.",
                "The data in this container is free to use.");

            // Begin a transformation phase for the second edition
            container.StartTransformation(secondEdition, null, privateKey, certificate);

            // Create a second entity 
            var entity1 = container.NewEntity(
                new Guid("54d04893-99a4-4bf7-a463-06a078dcc2f2"),
                randProvenance,
                null,
                entity0.Id);

            // Add a value with data that allready exists in the container
            // to check the compression capabilities
            entity1.AddValue(
                "testValue",
                new Guid("f1a9aaa0-cb36-417b-935c-7e5ddd28cade"),
                ValueAppearance.plain,
                tmpFile0);

            // Add another value
            entity1.AddValue(
                "smallValue1",
                new Guid("f1a9aaa0-cb36-417b-935c-7e5ddd28cade"),
                ValueAppearance.plain,
                tmpFile1);

            // Add a suppressed value
            entity1.AddValue(
                "smallValue2",
                new Guid("f1a9aaa0-cb36-417b-935c-7e5ddd28cade"),
                ValueAppearance.suppressed,
                tmpFile2);

            // Write the entity with all values to the container
            entity1.Close();

            // Seal the container with a new master signature
            container.FinishTransformation();

            // Dispose the container handle
            container.Dispose();

            // Validate the tranformed container
            Console.WriteLine("### Second Validation:");
            container = Container.OpenZip(targetPath, Console.WriteLine, compatibilityFlags);
            Expect(container.VerifyEntityValueSignatures(Console.WriteLine));
            Expect(container.ValidateCertificates(
                new CertificationAuthorityDirectory(ca),
                new CertificateValidationRules {AllowSelfSignedCertificate = true},
                Console.WriteLine));

            // Dispose the container handle
            container.Dispose();

            // Delete temporary random files
            File.Delete(tmpFile0);
            File.Delete(tmpFile1);
            File.Delete(tmpFile2);

            // Delete test container
            File.Delete(targetPath);
        }
    }
}

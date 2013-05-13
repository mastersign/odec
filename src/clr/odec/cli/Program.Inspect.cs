using System;
using System.IO;
using System.Linq;
using de.mastersign.odec.crypto;
using de.mastersign.odec.model;

namespace de.mastersign.odec.cli
{
    partial class Program
    {
        private static int Inspect()
        {
            var ok = true;

            if (StartupInfo.ContainerPath == null)
            {
                WriteWarning(Resources.Warning_NoContainerPathGiven);
                ok = false;
            }
            if (StartupInfo.ValidateProfile && StartupInfo.ProfileFile == null)
            {
                WriteWarning(Resources.Warning_NoProfileFile);
                ok = false;
            }

            if (!ok)
            {
                WriteHelpHint();
                return ERR_ARGUMENT_MISSING;
            }

            if (!File.Exists(StartupInfo.ContainerPath) &&
                !Directory.Exists(StartupInfo.ContainerPath))
            {
                WriteWarning(Resources.Warning_ConatinerNotFound,
                    StartupInfo.ContainerPath);
                return ERR_CONTAINER_MISSING;
            }

            Container container;
            var errC = OpenAndValidate(out container);

            if (errC != OK)
            {
                if (container != null)
                {
                    container.Dispose();
                }
                return errC;
            }

            Console.WriteLine("======================================================================");
            Console.WriteLine("Container Inspection");
            Console.WriteLine("======================================================================");
            Console.WriteLine("Summary");
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine("GUID:             {0}", container.CurrentEdition.Guid);
            Console.WriteLine("Profile:          {0}, {1}", container.CurrentEdition.Profile, container.CurrentEdition.Version);
            Console.WriteLine("Timestamp:        {0}", container.CurrentEdition.Timestamp);
            Console.WriteLine("Entity Count:     {0}", container.EntityCount);
            Console.WriteLine("History Size:     {0}", container.HistoryEditionCount);
            Console.WriteLine();
            
            Console.WriteLine("Current Edition");
            Console.WriteLine("----------------------------------------------------------------------");
            InspectEdition(container.CurrentEdition, Configuration.CryptoFactory);
            Console.WriteLine();
            
            for (int i = 0; i < container.HistoryEditionCount; i++)
            {
                Console.WriteLine("Past Edition {0}", i);
                Console.WriteLine("----------------------------------------------------------------------");
                InspectEdition(container.GetHistoryEdition(i), Configuration.CryptoFactory);
                Console.WriteLine();
            }

            foreach (var entityId in container.GetEntityIds())
            {
                Console.WriteLine("Entity {0}", entityId);
                Console.WriteLine("----------------------------------------------------------------------");
                var entity = container.GetEntity(entityId);
                InspectEntity(entity);
                Console.WriteLine();
            }
            container.Dispose();
            return errC;
        }

        private static void InspectEdition(EditionElement edition, ICryptoFactory cryptoFactory)
        {
            Console.WriteLine("GUID:             {0}", edition.Guid.ToString("D"));
            Console.WriteLine("Salt:             {0}", edition.SaltState);
            Console.WriteLine("Profile:          {0}", edition.Profile);
            Console.WriteLine("Profile-Version:  {0}", edition.Version);
            Console.WriteLine("Software:         {0}", edition.Software);
            Console.WriteLine("Timestamp:        {0}", edition.Timestamp);
            Console.WriteLine("New Entities:     {0}", string.Join(", ", 
                edition.NewEntities.Select(v => v.ToString()).ToArray()));
            Console.WriteLine("Removed Entities: {0}", string.Join(", ", 
                edition.RemovedEntities.Select(v => v.ToString()).ToArray()));
            Console.WriteLine("Owner:");
            Console.WriteLine("\tInstitute: {0}", edition.Owner.Institute);
            Console.WriteLine("\tOperator:  {0}", edition.Owner.Operator);
            Console.WriteLine("\tRole:      {0}", edition.Owner.Role ?? "<none>");
            Console.WriteLine("\tEmail:     {0}", edition.Owner.Email);
            
            Console.WriteLine("Certificate:");
            var cert = cryptoFactory.CreateRSAProviderFromPemEncodedCertificate(edition.Owner.X509Certificate);
            Console.Write(cert.GetCertificateInfo().ToString("\t"));

            if (!string.IsNullOrEmpty(edition.Copyright))
            {
                Console.WriteLine("Copyright:\n\t{0}", edition.Copyright.Replace("\n", "\n\t"));
            }
            if (!string.IsNullOrEmpty(edition.Comments))
            {
                Console.WriteLine("Comments:\n\t{0}", edition.Comments.Replace("\n", "\n\t"));
            }
        }

        private static void InspectEntity(Entity entity)
        {
            Console.WriteLine("ID:               {0}", entity.Id);
            Console.WriteLine("Label:            {0}", entity.Label);
            Console.WriteLine("Type:             {0}", entity.Type);
            Console.WriteLine("Provenance:       {0}", entity.Provenance.Guid.ToString("D"));
            Console.WriteLine("Predecessors:     {0}",
                string.Join(", ", entity.Predecessors.Select(v => v.ToString()).ToArray()));
            Console.WriteLine("Values:");
            foreach (var name in entity.ValueNames)
            {
                var v = entity.GetValue(name);
                Console.WriteLine("\t{0} ({1}): {2}", v.Name, v.Appearance, v.Type.ToString("D"));
            }
        }
    }
}

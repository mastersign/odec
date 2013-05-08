using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.cli
{
    partial class Program
    {
        private static int Reinstate()
        {
            var ok = true;

            if (StartupInfo.ContainerPath == null)
            {
                WriteWarning(Resources.Warning_NoContainerPathGiven);
                ok = false;
            }

            var targetId = Guid.Empty;
            try
            {
                targetId = new Guid(StartupInfo.TargetEdition);
            }
            catch (Exception)
            {
                WriteWarning(Resources.Warning_NoValidTargetEditionId);
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
                return errC;
            }

            if (!HasHistoryEdition(container, targetId))
            {
                container.Dispose();
                WriteWarning(Resources.Warning_EditionIdNotFound);
                return ERR_ARGUMENT_INVALID;
            }

            string message;
            if (!container.CanReinstateHistoryEdition(targetId, out message))
            {
                container.Dispose();
                WriteWarning(Resources.Warning_ReinstatingImpossible + "\n\t" + message);
                return ERR_REINSTATE_EDITION_IMPOSSIBLE;
            }
            try
            {
                container.ReinstateHistoryEdition(targetId);

                PostNewEdition(container);
            }
            catch (Exception ex)
            {
                WriteError(Resources.Error_UnexpectedReinstateError + "\n" + ex);
                return ERR_REINSTATE_EDITION_FAILED;
            }
            finally
            {
                container.Dispose();
            }
            return OK;
        }

        private static bool HasHistoryEdition(Container container, Guid editionId)
        {
            for (int i = 0; i < container.HistoryEditionCount; i++)
            {
                if (container.GetHistoryEdition(i).Guid == editionId)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

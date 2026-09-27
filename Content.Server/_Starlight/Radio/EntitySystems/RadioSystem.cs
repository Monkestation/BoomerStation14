using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.PDA;
using Robust.Shared.Utility;

namespace Content.Server.Radio.EntitySystems;

public sealed partial class RadioSystem
{
    [Dependency] private AccessReaderSystem _accessReader = default!;

    private (string, string) GetJobIcon(EntityUid messageSource)
    {
        var iconId = "JobIconNoId";
        var jobName = "";

        if (_accessReader.FindAccessItemsInventory(messageSource, out var items))
        {
            foreach (var item in items)
            {
                // ID Card
                if (TryComp<IdCardComponent>(item, out var id))
                {
                    iconId = id.JobIcon;
                    jobName = id.LocalizedJobTitle;
                    break;
                }

                // PDA
                if (TryComp<PdaComponent>(item, out var pda)
                    && pda.ContainedId != null
                    && TryComp(pda.ContainedId, out id))
                {
                    iconId = id.JobIcon;
                    jobName = id.LocalizedJobTitle;
                    break;
                }
            }
        }

        if (HasComp<BorgChassisComponent>(messageSource) || HasComp<BorgBrainComponent>(messageSource))
        {
            iconId = "JobIconBorg";
            jobName = Loc.GetString("job-name-borg");
        }

        if (HasComp<StationAiHeldComponent>(messageSource))
        {
            iconId = "JobIconStationAi";
            jobName = Loc.GetString("job-name-station-ai");
        }

        jobName ??= "";

        jobName = FormattedMessage.EscapeStringParameter(jobName); // Prevent markup injection

        return (iconId, jobName);
    }
}
using System.Collections.Generic;
using Kavita.ReleaseTools.Models;

namespace Kavita.ReleaseTools.Api;

public interface IStageConfiguration: IHasEnvironmentValues
{
    /// <summary>
    /// Disable the stage, while keeping the configuraiton
    /// </summary>
    public bool Disabled {  get; init; }

    /// <summary>
    /// If set, stage will only run for the specified <see cref="ReleaseTypes"/>
    /// </summary>
    public List<ReleaseType> ReleaseTypes {  get; init; }
}

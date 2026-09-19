namespace Kavita.ReleaseTools.Api;

public interface IStageConfiguration: IHasEnvironmentValues
{
    /// <summary>
    /// Disable the stage, while keeping the configuraiton
    /// </summary>
    public bool Disabled {  get; init; }
}

namespace Kavita.ReleaseTools.Api;

public interface IStageConfiguration
{
    /// <summary>
    /// Disable the stage, while keeping the configuraiton
    /// </summary>
    public bool Disable {  get; init; }
}

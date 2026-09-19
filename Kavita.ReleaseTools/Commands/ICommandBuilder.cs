using Kavita.ReleaseTools.Api;

namespace Kavita.ReleaseTools.Commands;

public interface ICommandBuilder
{
    ICommand Build();
}

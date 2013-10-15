using System.Collections.ObjectModel;
using Melee.Me.Models;

namespace Melee.Me.Infrastructure
{
    public interface IConnectionRepository : IRepository<ConnectionModel>
    {
        ConnectionModel Add(int userId, string connectionName, string accessToken);
        Collection<ConnectionModel> Get(int id);
        bool HasConnection(int userId, string connectionName);
        bool Delete(int userId, int id);
    }
}

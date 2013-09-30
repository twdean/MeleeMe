using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Melee.Me.Models;

namespace Melee.Me.Infrastructure
{
    public interface IConnectionRepository : IRepository<ConnectionModel>
    {
        ConnectionModel Add(int userId, string connectionName, string accessToken);
        void Delete(ConnectionModel entity);
        Collection<ConnectionModel> Get(string id);
        bool HasConnection(int userId, string connectionName);

    }
}

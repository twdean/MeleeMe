using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Melee.Me.Models;
using MeleeMeDatabase;

namespace Melee.Me.Infrastructure.Repository
{
    public class ConnectionRepository : IRepository<ConnectionModel>
    {
        public IQueryable<ConnectionModel> Find(Expression<Func<ConnectionModel, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public bool HasConnection(int userId, string connectionName)
        {
            bool hasConnection = false;

            var dbContext = new MeleeMeEntities();

            using (dbContext)
            {
                var val = (from uc in dbContext.m_UserConnections
                                               .Where(x => x.UserId == userId)
                                               .Where(x => x.ConnectionId == x.m_Connections.ConnectionId)
                                               .Where(x => x.m_Connections.ConnectionName == connectionName)
                           select uc).FirstOrDefault();


                if (val != null)
                    hasConnection = true;
            }

            return hasConnection;
        }

        public static Collection<ConnectionModel> Get(int userId)
        {
            var dbContext = new MeleeMeEntities();
            var connections = new Collection<ConnectionModel>();

            var result = (from cx in dbContext.m_Connections
                          join ucx in dbContext.m_UserConnections on cx.ConnectionId equals ucx.ConnectionId
                          join u in dbContext.m_User on ucx.UserId equals u.UserId
                          select cx).ToList();

            foreach (var conn in result)
            {
                var c = new ConnectionModel
                {
                    ConnectionName = conn.ConnectionName,
                    ConnectionId = conn.ConnectionId,
                    ConnectionIcon = conn.ConnectionIcon
                };

                connections.Add(c);

            }

            return connections;
        }

        public static ConnectionModel Add(int userId, string connectionName, string accessToken)
        {
            var dbContext = new MeleeMeEntities();

            using (dbContext)
            {
                try
                {
                    var conn = (from cx in dbContext.m_Connections
                                where cx.ConnectionName == connectionName
                                select cx).FirstOrDefault();

                    var c = new m_UserConnections()
                    {
                        UserId = userId,
                        ConnectionId = conn.ConnectionId,
                        AccessToken = accessToken
                    };

                    var cm = new ConnectionModel
                    {
                        ConnectionName = connectionName,
                        ConnectionId = conn.ConnectionId,
                        UserHasConnection = true
                    };

                    dbContext.m_UserConnections.Add(c);
                    dbContext.SaveChanges();

                    return cm;

                }
                catch (Exception ex)
                {
                    var msg = ex.ToString();
                }
            }

            return null;
        }
    }
}
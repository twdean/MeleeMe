using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MeleeMeDatabase;

namespace Melee.Me.Models
{
    public class ConnectionModel
    {
        public int ConnectionId { get; set; }
        public string ConnectionName { get; set; }
        public string ConnectionIcon { get; set; }
        public string ConnectionUrl { get; set; }
        public bool UserHasConnection { get; set; }

        public static Collection<ConnectionModel> GetUserConnections(int userId)
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

            }

            return connections;
        }

        public static ConnectionModel AddConnection(int userId, string connectionName, string accessToken)
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
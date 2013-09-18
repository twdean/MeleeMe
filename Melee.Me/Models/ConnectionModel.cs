using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using MeleeMeDatabase;

namespace Melee.Me.Models
{
    public class ConnectionModel
    {
        public string ConnectionName { get; set; }
        public string ConnectionIcon { get; set; }
        public string ConnectionUrl { get; set; }
        public bool UserHasConnection { get; set; }

        public static IEnumerable<ConnectionModel> GetUserConnections(int userId)
        {
            var dbContext = new MeleeMeEntities();
            IEnumerable<ConnectionModel> connections = new Collection<ConnectionModel>();

            var result = (from cx in dbContext.m_Connections
                          join ucx in dbContext.m_UserConnections on cx.ConnectionId equals ucx.ConnectionId
                          join u in dbContext.m_User on ucx.UserId equals u.UserId
                          select cx).ToList();

            foreach (var conn in result)
            {
                

            }

            return connections;
        }

        public static bool AddConnection(int userId, string connectionName, string accessToken)
        {
            var dbContext = new MeleeMeEntities();
            var result = false;

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

                    dbContext.m_UserConnections.Add(c);
                    dbContext.SaveChanges();

                    result = true;
                }
                catch (Exception ex)
                {
                    string msg = ex.ToString();
                }
            }

            return result;
        }
    }

}
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Melee.Me.Models;
using MeleeMeDatabase;

namespace Melee.Me.Infrastructure.Repository
{
    public class ConnectionRepository : IConnectionRepository
    {
        public IQueryable<ConnectionModel> Find(Expression<Func<ConnectionModel, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public bool Delete(int userId, int id)
        {
            var dbContext = new MeleeMeEntities();

            using (dbContext)
            {
                var userConn =
                    (from uc in dbContext.m_UserConnections 
                     where uc.UserId == userId && uc.ConnectionId == id 
                     select uc).FirstOrDefault();

                dbContext.m_UserConnections.Remove(userConn);
                dbContext.SaveChanges();
            }

            return true;
        }

        public Collection<ConnectionModel> Get(int id)
        {
            var dbContext = new MeleeMeEntities();
            var connections = new Collection<ConnectionModel>();

            var result = (from cx in dbContext.m_Connections
                          join ucx in dbContext.m_UserConnections on cx.ConnectionId equals ucx.ConnectionId
                          join u in dbContext.m_User on ucx.UserId equals u.UserId
                          where u.UserId == id
                          select cx).ToList();

            foreach (var conn in result)
            {
                var c = new ConnectionModel
                {
                    ConnectionName = conn.ConnectionName,
                    ConnectionId = conn.ConnectionId,
                    ConnectionIcon = conn.ConnectionIcon,
                    AccessToken = GetAccessToken(dbContext, id, conn.ConnectionId),
                    RefreshToken = GetRefreshToken(dbContext, id, conn.ConnectionId),
                    OAuthToken = GetOAuthToken(dbContext, id, conn.ConnectionId),
                    ConnectionProvider = LoadConnectionProvider(conn.ConnectionProvider)
                };

                connections.Add(c);

            }

            return connections;
        }

        public ConnectionModel Add(int userId, string connectionName, string accessToken, string refreshToken)
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
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    };

                    var cm = new ConnectionModel
                    {
                        ConnectionName = connectionName,
                        ConnectionId = conn.ConnectionId,
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        ConnectionProvider = LoadConnectionProvider(connectionName + "Connection")
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

        private static IConnection LoadConnectionProvider(string connectionName)
        {
            const string typeNamespace = "Melee.Me.Infrastructure.Connection";
            var type = Type.GetType(String.Format("{0}.{1}", typeNamespace, connectionName), true);
            var provider = (IConnection)Activator.CreateInstance(type);

            return provider;
        }

        public static string GetAccessToken(MeleeMeEntities dbContext, int userId, int connectionId)
        {
            var userConn = (from uc in dbContext.m_UserConnections
                               where uc.UserId == userId && uc.ConnectionId == connectionId
                               select uc).Single();

            return userConn.AccessToken;
        }

        public static string GetRefreshToken(MeleeMeEntities dbContext, int userId, int connectionId)
        {
            var userConn = (from uc in dbContext.m_UserConnections
                            where uc.UserId == userId && uc.ConnectionId == connectionId
                            select uc).Single();

            return userConn.RefreshToken;
        }

        public static string GetOAuthToken(MeleeMeEntities dbContext, int userId, int connectionId)
        {
            var userConn = (from uc in dbContext.m_UserConnections
                            where uc.UserId == userId && uc.ConnectionId == connectionId
                            select uc).Single();

            return userConn.OAuthToken;
        }



        public static Collection<ConnectionModel> Get(string twitterUserId)
        {
            var dbContext = new MeleeMeEntities();
            var connections = new Collection<ConnectionModel>();

            var result = (from cx in dbContext.m_Connections
                          join ucx in dbContext.m_UserConnections on cx.ConnectionId equals ucx.ConnectionId
                          join u in dbContext.m_User on ucx.UserId equals u.UserId
                          where u.TwitterUserId == twitterUserId
                          select cx).ToList();

            foreach (var conn in result)
            {
                var c = new ConnectionModel
                {
                    ConnectionName = conn.ConnectionName,
                    ConnectionId = conn.ConnectionId,
                    ConnectionIcon = conn.ConnectionIcon,
                    ConnectionProvider = LoadConnectionProvider(conn.ConnectionProvider)
                };

                connections.Add(c);

            }

            return connections;
        }

    }
}
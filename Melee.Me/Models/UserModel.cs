using System;
using System.Collections.ObjectModel;
using System.Linq;
using Melee.Me.Infrastructure;
using Melee.Me.Types;
using MeleeMeDatabase;

namespace Melee.Me.Models
{
    public class UserModel : IEntity<UserModel>
    {
        public int UserId { get; set; }
        public string ImageUrl { get; set; }
        public string ScreenName { get; set; }
        public string TwitterUserId { get; set; }
        public string AccessToken { get; set; }
        public MeleeStatisticsModel Stats { get; set; }
        public Collection<ConnectionModel> Connections { get; set; }


        public UserModel Add(string twitterUserId, string accessToken)
        {
            var newUser = null as UserModel;
            var dbContext = new MeleeMeEntities();


            using (dbContext)
            {
                var mUser = dbContext.m_User.FirstOrDefault(mu => mu.TwitterUserId == twitterUserId);

                if (mUser != null) return new UserModel
                {
                    TwitterUserId = twitterUserId,
                    UserId = mUser.UserId,
                    AccessToken = mUser.m_Credentials.Select(at => at.AccessToken).ToString(),
                    Stats = MeleeStatisticsModel.GetMeleeStats(twitterUserId, UserType.Challenger),
                    Connections = ConnectionModel.GetUserConnections(mUser.UserId)
                };


                try
                {
                    var u = new m_User
                        {
                            TwitterUserId = twitterUserId
                        };


                    dbContext.m_User.Add(u);
                    AddUserCredentials(dbContext, u, accessToken);

                    Save(dbContext);

                    newUser = new UserModel
                        {
                            TwitterUserId = twitterUserId,
                            AccessToken = accessToken,
                            UserId = u.UserId,
                            Stats = new MeleeStatisticsModel()
                        };
                }
                catch (Exception)
                {
                    //TODO: Propogate message and log
                }
            }

            return newUser;
        }

        public UserModel Get(string twitterUserId)
        {
            var meleeUser = null as UserModel;
            var _dbContext = new MeleeMeEntities();

            using (_dbContext)
            {
                var mUser = (from c in _dbContext.m_Credentials
                             where c.m_User.TwitterUserId == twitterUserId
                             select new { c.AccessToken, c.UserId }).FirstOrDefault();

                if (mUser != null)
                {
                    meleeUser = new UserModel
                    {
                        TwitterUserId = twitterUserId,
                        UserId = mUser.UserId,
                        AccessToken = mUser.AccessToken,
                        Stats = MeleeStatisticsModel.GetMeleeStats(twitterUserId, UserType.Challenger),
                        Connections = ConnectionModel.GetUserConnections(mUser.UserId)
                    };
                }
            }

            return meleeUser;
        }

        public void Save(MeleeMeEntities dbContext)
        {
            dbContext.SaveChanges();
        }

        public void Delete(UserModel entity)
        {
            throw new NotImplementedException();
        }

        public IQueryable Find(System.Linq.Expressions.Expression<Func<UserModel, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        private static void AddUserCredentials(MeleeMeEntities dbContext, m_User u, string accessToken)
        {
            var c = new m_Credentials
            {
                UserId = u.UserId,
                AccessToken = accessToken
            };

            dbContext.m_Credentials.Add(c);
        }

        public bool GetConnection(int userId, string connectionName)
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

    }
}
﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Melee.Me.Models;
using Melee.Me.Types;
using MeleeMeDatabase;

namespace Melee.Me.Infrastructure.Repository
{
    public class MeleeUserRepository : IUserRepository
    {
        public UserModel Add(string id, string token)
        {
            var newUser = null as UserModel;
            var dbContext = new MeleeMeEntities();


            using (dbContext)
            {
                var mUser = dbContext.m_User.FirstOrDefault(mu => mu.TwitterUserId == id);

                if (mUser != null) return new UserModel
                {
                    TwitterUserId = id,
                    UserId = mUser.UserId,
                    AccessToken = mUser.m_Credentials.Select(at => at.AccessToken).ToString(),
                    Stats = MeleeRepository.GetMeleeStats(id, UserType.Challenger),
                    Connections = new ConnectionRepository().Get(mUser.UserId)
                };


                try
                {
                    var u = new m_User
                    {
                        TwitterUserId = id
                    };


                    dbContext.m_User.Add(u);
                    AddUserCredentials(dbContext, u, token);
                    AddConnection(dbContext, u, token);

                    newUser = new UserModel
                    {
                        TwitterUserId = id,
                        AccessToken = token,
                        UserId = u.UserId,
                        Stats = new MeleeStatisticsModel()
                    };

                    dbContext.SaveChanges();
                    newUser.Connections = new ConnectionRepository().Get(u.UserId);
                }
                catch (Exception)
                {
                    //TODO: Propogate message and log
                }
            }

            return newUser;
        }

        private void AddConnection(MeleeMeEntities dbContext, m_User u, string accessToken)
        {
            var conn = new m_UserConnections
                {
                    UserId = u.UserId,
                    ConnectionId = (from c in dbContext.m_Connections
                                    where c.ConnectionName == "Twitter"
                                    select c.ConnectionId).FirstOrDefault(),
                    AccessToken = accessToken
                };

            dbContext.m_UserConnections.Add(conn);
        }

        public void Delete(UserModel entity)
        {
            throw new NotImplementedException();
        }

        public UserModel Get(string id)
        {
            var meleeUser = null as UserModel;
            var dbContext = new MeleeMeEntities();

            using (dbContext)
            {
                var mUser = (from c in dbContext.m_Credentials
                             where c.m_User.TwitterUserId == id
                             select new { c.AccessToken, c.UserId }).FirstOrDefault();

                if (mUser != null)
                {
                    meleeUser = new UserModel
                    {
                        TwitterUserId = id,
                        UserId = mUser.UserId,
                        AccessToken = mUser.AccessToken,
                        Stats = MeleeRepository.GetMeleeStats(id, UserType.Challenger),
                        Connections = new ConnectionRepository().Get(mUser.UserId)
                    };
                }
            }

            return meleeUser;
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

        IQueryable<UserModel> IRepository<UserModel>.Find(Expression<Func<UserModel, bool>> predicate)
        {
            return null;
        }
    }
}
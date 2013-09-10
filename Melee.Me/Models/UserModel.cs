using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Melee.Me.Types;
using MeleeMeDatabase;

namespace Melee.Me.Models
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string ImageUrl { get; set; }
        public string ScreenName { get; set; }
        public string TwitterUserId { get; set; }
        public string AccessToken { get; set; }
        public MeleeStatisticsModel Stats { get; set; }

        public static UserModel GetUser(string twitterUserId)
        {
            var meleeUser = new UserModel();

            var dbContext = new MeleeMeEntities();

            using (dbContext)
            {
                var mUser = (from c in dbContext.m_Credentials
                             where c.m_User.TwitterUserId == twitterUserId
                             select new { c.AccessToken, c.UserId }).FirstOrDefault();

                if (mUser != null)
                {
                    meleeUser.TwitterUserId = twitterUserId;
                    meleeUser.UserId = mUser.UserId;
                    meleeUser.AccessToken = mUser.AccessToken;
                    meleeUser.Stats = MeleeStatisticsModel.GetMeleeStats(twitterUserId, UserType.Challenger);
                }


            }

            return meleeUser;
        }

        public static UserModel AddUser(string twitterUserId, string accessToken)
        {
            var mUser = GetUser(twitterUserId);

            if (mUser != null) return mUser;

            var dbContext = new MeleeMeEntities();

            using (dbContext)
            {
                try
                {
                    var u = new m_User
                        {
                            TwitterUserId = twitterUserId,
                        };


                    dbContext.m_User.Add(u);
                    AddUserCredentials(dbContext, u, accessToken);

                    dbContext.SaveChanges();

                    mUser = new UserModel
                        {
                            TwitterUserId = twitterUserId,
                            AccessToken = accessToken,
                            UserId = mUser.UserId,
                            Stats = new MeleeStatisticsModel()
                        };
                }
                catch (Exception ex)
                {
                    string msg = ex.ToString();
                }
            }

            return mUser;
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
    }
}
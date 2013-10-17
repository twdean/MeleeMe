using System;
using System.Linq;
using System.Linq.Expressions;
using Melee.Me.Models;
using Melee.Me.Types;
using MeleeMeDatabase;

namespace Melee.Me.Infrastructure.Repository
{
    public class MeleeRepository : IMeleeRepository
    {
        public void Add(UserModel currentUser, UserModel opponent, string winner, string loser)
        {
            var dbContext = new MeleeMeEntities();

            using (dbContext)
            {
                var m = new m_Melee
                    {
                        challenger = currentUser.TwitterUserId,
                        opponent = opponent.TwitterUserId,
                        timestamp = DateTime.Now
                    };

                dbContext.m_Melee.Add(m);
                AddMeleeStatistics(dbContext, m, winner, loser);
                dbContext.SaveChanges();

            }
        }

        public MeleeModel Get(string id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<MeleeModel> Find(Expression<Func<MeleeModel, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public void Delete(int userId, int id)
        {
            throw new NotImplementedException();
        }

        public static MeleeStatisticsModel GetMeleeStats(string twitterUserId, UserType userType)
        {
            var dbContext = new MeleeMeEntities();
            IQueryable<m_MeleeStats> result = null;
            MeleeStatisticsModel meleeStats = null;

            using (dbContext)
            {
                switch (userType)
                {
                    case UserType.Challenger:
                        result = from s in dbContext.m_MeleeStats
                                 where s.m_Melee.challenger == twitterUserId
                                 select s;
                        break;
                    case UserType.Opponent:
                        result = from s in dbContext.m_MeleeStats
                                 where s.m_Melee.opponent == twitterUserId
                                 select s;
                        break;
                }

                meleeStats = new MeleeStatisticsModel
                {
                    BattleWins = result.Count(s => s.meleeWinner == twitterUserId),
                    BattleLosses = result.Count(s => s.meleeLoser == twitterUserId)
                    //LastMelee =  (DateTime)result.OrderBy(ts => ts.m_Melee.timestamp).Select(m => m.m_Melee.timestamp).FirstOrDefault()
                };
            }

            return meleeStats;
        }

        public static void AddMeleeStatistics(MeleeMeEntities dbContext, m_Melee m, string winner, string loser)
        {
            var ms = new m_MeleeStats
                {
                    meleeId = m.meleeId,
                    meleeWinner = winner,
                    meleeLoser = loser
                };

            dbContext.m_MeleeStats.Add(ms);
        }
    }
}
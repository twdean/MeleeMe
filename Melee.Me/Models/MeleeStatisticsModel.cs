using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MeleeMeDatabase;
using Melee.Me.Types;

namespace Melee.Me.Models
{
    public class MeleeStatisticsModel
    {
        public int UserId { get; set; }
        public string TwitterUserId { get; set; }
        public int BattleWins { get; set; }
        public int BattleLosses { get; set; }
        public DateTime LastMelee { get; set; }

        public MeleeStatisticsModel()
        {
            BattleLosses = 0;
            BattleWins = 0;
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

                meleeStats = new MeleeStatisticsModel()
                {
                    BattleWins = result.Count(s => s.meleeWinner == twitterUserId),
                    BattleLosses = result.Count(s => s.meleeLoser == twitterUserId)
                    //LastMelee =  (DateTime)result.OrderBy(ts => ts.m_Melee.timestamp).Select(m => m.m_Melee.timestamp).FirstOrDefault()
                };


            }


            return meleeStats;

        }
    }
}
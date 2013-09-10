using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MeleeMeDatabase;

namespace Melee.Me.Models
{
    public class MeleeModel
    {
        public UserModel Challenger { get; set; }
        public UserModel Competitor { get; set; }


        public static void AddMelee(string challenger, string opponent, string winner, string loser)
        {
            var _dbContext = new MeleeMeEntities();
            var meleeModel = new MeleeModel();

            using (_dbContext)
            {
                var m = new m_Melee()
                    {
                        challenger = challenger,
                        opponent = opponent,
                        timestamp = DateTime.Now
                    };

                _dbContext.m_Melee.Add(m);
                AddMeleeStatistics(_dbContext, m, winner, loser);
                _dbContext.SaveChanges();

            }
        }

        public static void AddMeleeStatistics(MeleeMeEntities dbContext, m_Melee m, string winner, string loser)
        {
            var ms = new m_MeleeStats()
            {
                meleeId = m.meleeId,
                meleeWinner = winner,
                meleeLoser = loser
            };

            dbContext.m_MeleeStats.Add(ms);
        }
    }
}
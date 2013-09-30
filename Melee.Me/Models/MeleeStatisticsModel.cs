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

    }
}
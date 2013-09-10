using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Melee.Me.Models
{
    public class MeleeResultModel
    {
        public string Winner { get; set; }
        public string WinnerLogo { get; set; }


        public MeleeResultModel(string winner, string logo)
        {
            Winner = winner;
            WinnerLogo = logo;
        }
    }
}
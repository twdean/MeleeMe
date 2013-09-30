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
    }
}
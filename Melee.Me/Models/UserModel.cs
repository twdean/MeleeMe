using System;
using System.Collections.ObjectModel;
using System.Linq;
using Melee.Me.Infrastructure;
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
        public Collection<ConnectionModel> Connections { get; set; }
    }
}
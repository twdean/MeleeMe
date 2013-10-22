using System.Collections.Generic;
using System.Linq;
using Facebook;
using Melee.Me.Models;

namespace Melee.Me.Infrastructure.Connection
{
    public class GoogleConnection : IConnection
    {
        public string AccessToken { get; set; }

        public double GetScore(UserModel meleeUser)
        {
            AccessToken = meleeUser.Connections.Single(x => x.ConnectionName == "Google").AccessToken;
            double score = 0.00;

            return score;
        }

    }
}
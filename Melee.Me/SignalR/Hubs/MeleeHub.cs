using System.Linq;
using Melee.Me.Infrastructure.Connection;
using Melee.Me.Infrastructure.Repository;
using Melee.Me.Models;
using Microsoft.AspNet.SignalR;

namespace Melee.Me.SignalR.Hubs
{
    public class MeleeHub : Hub
    {
        private IUserRepository _repository;

        public void Send(string meleeUser, string opponent)
        {
            Clients.Caller.broadcastMessage("The battle has begun!");

            _repository = new MeleeUserRepository();

            //if the opponent is not a registered user then only battle twitter
            UserModel currentUser = _repository.Get(meleeUser);
            UserModel opponentUser = _repository.Get(opponent);

            if (opponentUser != null)
            {

            }
            else
            {
                Clients.Caller.broadcastMessage("Battling for Twitter supremecy.");

                double userScore = currentUser.Connections.SingleOrDefault(c => c.ConnectionName == "Twitter").ConnectionProvider.GetScore(currentUser);
                double opponentScore = new TwitterConnection().GetScore(opponent);

                Clients.Caller.broadcastMessage("you: " + userScore + " " + " them: " + opponentScore);
           }
        }

    }
}
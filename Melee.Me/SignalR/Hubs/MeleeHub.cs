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
                GetMeleeData(currentUser, opponentUser);
            }
            else
            {
                Clients.Caller.broadcastMessage("Battling for Twitter supremecy.");

                double userScore = currentUser.Connections.SingleOrDefault(c => c.ConnectionName == "Twitter").ConnectionProvider.GetScore(currentUser);
                double opponentScore = new TwitterConnection().GetScore(opponent);

                Clients.Caller.broadcastMessage("you: " + userScore + " " + " them: " + opponentScore);
           }
        }

        public void GetMeleeData(UserModel challenger, UserModel competitor)
        {
            UserModel meleeWinner = null;
            double challengerScore = 0;
            double competitorScore = 0;

            try
            {
                    challengerScore +=
                        challenger.Connections.Sum(connection => connection.ConnectionProvider.GetScore(challenger));

                    if (competitor.Connections != null)
                    {
                        competitorScore +=
                            competitor.Connections.Sum(connection => connection.ConnectionProvider.GetScore(competitor));
                    }
                    else
                    {
                        competitorScore = new TwitterConnection().GetScore(competitor);
                    }
            }
            catch (HubException hEx)
            {
                
            }


        }


    }
}
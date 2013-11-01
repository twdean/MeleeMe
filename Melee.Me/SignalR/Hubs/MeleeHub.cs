﻿using System.Linq;
using Melee.Me.Infrastructure;
using Melee.Me.Infrastructure.Connection;
using Melee.Me.Infrastructure.Repository;
using Melee.Me.Models;
using Microsoft.AspNet.SignalR;

namespace Melee.Me.SignalR.Hubs
{
    public class MeleeHub : Hub
    {
        private IUserRepository _repository;
        private IMeleeRepository _meleeRepository;

        public void Send(string meleeUser, string opponent)
        {
            string resultsText = "and the winner is.....click <a href='{0}' >here</a> to find out";
            string winnerId;

            Clients.Caller.broadcastMessage("The battle has begun!");

            _repository = new MeleeUserRepository();
            _meleeRepository = new MeleeRepository();

            //if the opponent is not a registered user then only battle twitter
            UserModel currentUser = _repository.Get(meleeUser);
            UserModel opponentUser = _repository.Get(opponent);

            double userScore = 0.00;
            double opponentScore = 0.00;

            if (opponentUser != null)
            {

                //battle twitter
                Clients.Caller.broadcastMessage("Battling for Twitter supremecy.");
                var currentUserTwitter = currentUser.Connections.SingleOrDefault(c => c.ConnectionName == "Twitter");
                if (currentUserTwitter != null)
                    userScore = currentUserTwitter.ConnectionProvider.GetScore(currentUser);
                var currentOpponentTwitter = currentUser.Connections.SingleOrDefault(c => c.ConnectionName == "Twitter");
                if (currentOpponentTwitter != null)
                    opponentScore = currentOpponentTwitter.ConnectionProvider.GetScore(currentUser);
                Clients.Caller.broadcastMessage("you: " + userScore + " " + " them: " + opponentScore);

                //battle facebook
                Clients.Caller.broadcastMessage("Battling for Facebook supremecy.");
                var currentUserFacebook = currentUser.Connections.SingleOrDefault(c => c.ConnectionName == "Facebook");
                if (currentUserFacebook != null)
                    userScore = currentUserFacebook.ConnectionProvider.GetScore(currentUser);

                var currentOpponentFacebook = currentUser.Connections.SingleOrDefault(c => c.ConnectionName == "Facebook");
                if (currentOpponentFacebook != null)
                    opponentScore = currentOpponentFacebook.ConnectionProvider.GetScore(currentUser);
                Clients.Caller.broadcastMessage("you: " + userScore + " " + " them: " + opponentScore);

                //battle google+
                Clients.Caller.broadcastMessage("Battling for Google+ supremecy.");
                var currentUserGoogle = currentUser.Connections.SingleOrDefault(c => c.ConnectionName == "Google+");
                if (currentUserGoogle != null)
                    userScore = currentUserGoogle.ConnectionProvider.GetScore(currentUser);
                var currentOpponentGoogle = currentUser.Connections.SingleOrDefault(c => c.ConnectionName == "Google+");

                if (currentOpponentGoogle != null)
                    opponentScore = currentOpponentGoogle.ConnectionProvider.GetScore(currentUser);
                Clients.Caller.broadcastMessage("you: " + userScore + " " + " them: " + opponentScore);
            }
            else
            {
                Clients.Caller.broadcastMessage("Battling for Twitter supremecy.");

                userScore = currentUser.Connections.SingleOrDefault(c => c.ConnectionName == "Twitter").ConnectionProvider.GetScore(currentUser);
                opponentScore = new TwitterConnection().GetScore(opponent);

                Clients.Caller.broadcastMessage("you: " + userScore + " " + " them: " + opponentScore);
            }

            if (userScore > opponentScore)
            {
                _meleeRepository.Add(meleeUser, opponent, meleeUser, opponent);
                winnerId = meleeUser;
            }
            else
            {
                _meleeRepository.Add(meleeUser, opponent, opponent, meleeUser);
                winnerId = opponent;
            }

            Clients.Caller.broadcastMessage(string.Format(resultsText, "/Melee/MeleeMe/" + winnerId));
        }
    }
}
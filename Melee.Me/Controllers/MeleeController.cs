using System;
using System.Linq;
using System.Web.Mvc;
using LinqToTwitter;
using Melee.Me.Infrastructure;
using Melee.Me.Infrastructure.Connection;
using Melee.Me.Infrastructure.Repository;
using Melee.Me.Models;

namespace Melee.Me.Controllers
{
    public class MeleeController : Controller
    {
        private readonly MeleeRepository _repository;

        public MeleeController()
        {
            _repository = new MeleeRepository();
        }


        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MeleeMe()
        {
            //loop through user connections and get score
            var challenger = (UserModel)Session["challenger"];
            var competitor = (UserModel)Session["competitor"];

            UserModel result = GetMeleeData(challenger, competitor);

            if (result == null)
            {
                return View("Error");
            }

            return View("Melee", result);
        }

        public UserModel GetMeleeData(UserModel challenger, UserModel competitor)
        {
            UserModel meleeWinner = null;
            double challengerScore = 0;
            double competitorScore = 0;

            try
            {
                try
                {
                    challengerScore += challenger.Connections.Sum(connection => connection.ConnectionProvider.GetScore(challenger));

                    if (competitor.Connections != null)
                    {
                        competitorScore += competitor.Connections.Sum(connection => connection.ConnectionProvider.GetScore(competitor));
                    }
                    else
                    {
                        competitorScore = new TwitterConnection().GetScore(competitor);
                    }
                }
                catch (TwitterQueryException tqEx)
                {
                    if (tqEx.ErrorCode == 88)
                    {

                    }
                }

                if (challengerScore > competitorScore)
                {
                    meleeWinner = challenger;
                    _repository.Add(challenger, competitor, challenger.TwitterUserId, competitor.TwitterUserId);

                }
                else
                {
                    meleeWinner = competitor;
                    _repository.Add(challenger, competitor, competitor.TwitterUserId, challenger.TwitterUserId);

                }

                Session["challenger"] = null;
                Session["competitor"] = null;
            }
            catch (TwitterQueryException tqEx)
            {
                if (tqEx.ErrorCode == 88)
                {

                }
            }

            return meleeWinner;
        }
    }
}

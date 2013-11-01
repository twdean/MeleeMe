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
        private readonly MeleeUserRepository _repository;

        public MeleeController()
        {
            _repository = new MeleeUserRepository();
        }


        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MeleeMe(string twitterId)
        {
            UserModel challenger = null;
            UserModel opponent = null;

            if (Session["challenger"] != null)
            {
                challenger = (UserModel)Session["challenger"];
            }


            if (Session["competitor"] != null)
            {
                opponent = (UserModel)Session["competitor"];
            }

            if (twitterId == challenger.TwitterUserId)
            {
                return View("Melee", challenger);
            }
            else
            {
                return View("Melee", opponent);
            }
        }

        //public UserModel GetMeleeData(UserModel challenger, UserModel competitor)
        //{
        //    UserModel meleeWinner = null;
        //    double challengerScore = 0;
        //    double competitorScore = 0;

        //    try
        //    {
        //        try
        //        {
        //            challengerScore += challenger.Connections.Sum(connection => connection.ConnectionProvider.GetScore(challenger));

        //            if (competitor.Connections != null)
        //            {
        //                competitorScore += competitor.Connections.Sum(connection => connection.ConnectionProvider.GetScore(competitor));
        //            }
        //            else
        //            {
        //                competitorScore = new TwitterConnection().GetScore(competitor);
        //            }
        //        }
        //        catch (TwitterQueryException tqEx)
        //        {
        //            if (tqEx.ErrorCode == 88)
        //            {

        //            }
        //        }

        //        if (challengerScore > competitorScore)
        //        {
        //            meleeWinner = challenger;
        //            _repository.Add(challenger, competitor, challenger.TwitterUserId, competitor.TwitterUserId);

        //        }
        //        else
        //        {
        //            meleeWinner = competitor;
        //            _repository.Add(challenger, competitor, competitor.TwitterUserId, challenger.TwitterUserId);

        //        }

        //        Session["challenger"] = null;
        //        Session["competitor"] = null;
        //    }
        //    catch (TwitterQueryException tqEx)
        //    {
        //        if (tqEx.ErrorCode == 88)
        //        {

        //        }
        //    }

        //    return meleeWinner;
        //}
    }
}

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

        public ActionResult MeleeMe(string screenName, string imageUrl)
        {
            var winner = new UserModel
                {
                    ScreenName = "Winner",
                    ImageUrl = ""
                };

            return View("Melee", winner);
        }
    }
}

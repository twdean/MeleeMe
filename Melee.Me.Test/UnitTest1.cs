using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LinqToTwitter;
using MeleeMeDatabase;
using Melee.Me.Models;

namespace Melee.Me.Test
{
    [TestClass]
    public class UnitTest1
    {
        public MvcAuthorizer GetAuthorizer()
        {
            IOAuthCredentials credentials = new SessionStateCredentials();
            MvcAuthorizer auth;

            if (credentials.ConsumerKey == null || credentials.ConsumerSecret == null)
            {
                credentials.ConsumerKey = "u2ULchA68sGq111YWL2foA";
                credentials.ConsumerSecret = "JcXAVK1GHaFMXtRLZASIviwDhQvtOLliaMKYfO0rY";
            }

            auth = new MvcAuthorizer
            {
                Credentials = credentials
            };

            return auth;
        }

        [TestMethod]
        public void GetUser()
        {
                       
        }
    }
}

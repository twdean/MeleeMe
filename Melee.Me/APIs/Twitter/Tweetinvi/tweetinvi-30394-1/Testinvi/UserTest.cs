﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TweetinCore.Interfaces;
using Tweetinvi;

namespace Testinvi
{
    [TestClass]
    public class UserTest
    {
        #region Constructor
        [TestMethod]
        [TestCategory("Constructor"), TestCategory("User")]
        public void User_Constructor1()
        {
            long id = -1;
            IUser user = new User(id);

            Assert.AreEqual(user.Name, null);
        }

        [TestMethod]
        [TestCategory("Constructor"), TestCategory("User")]
        public void User_Constructor2()
        {
            // Expect the Ladygaga to be retrieved from Twitter
            IUser user = new User("ladygaga", TokenSingleton.Instance);

            Assert.AreEqual(user.Id, 14230524);
        }

        [TestMethod]
        [TestCategory("Constructor"), TestCategory("User")]
        public void User_Constructor3()
        {
            // Expect the Ladygaga to be retrieved from Twitter
            IUser user = new User(14230524, TokenSingleton.Instance);

            Assert.AreEqual(user.ScreenName, "ladygaga");
        } 
        #endregion

        #region GetFriends

        [TestMethod]
        [TestCategory("GetFriends"), TestCategory("User")]
        public void UserGetFriends()
        {
            IUser u = new User("linviStevens", TokenSingleton.Instance);
            List<long> friendsIds = u.GetFriendsIds(true);
            List<IUser> friends = u.Friends;

            Assert.AreNotEqual(friendsIds.Count, 0);
            Assert.AreEqual(friendsIds.Count, friends.Count);
        }

        [TestMethod]
        [TestCategory("GetFriends"), TestCategory("User")]
        public void UserGetFriends1()
        {
            IUser u = new User("linviStevens", TokenSingleton.Instance);
            List<long> friendsIds = u.GetFriendsIds(false);
            List<IUser> friends = u.Friends;

            Assert.AreNotEqual(friendsIds.Count, 0);
            Assert.AreEqual(friends.Count, 0);
        }

        #endregion

        #region GetFollowers

        [TestMethod]
        [TestCategory("GetFollowers"), TestCategory("User")]
        public void UserGetFollowers()
        {
            IUser u = new User("linviStevens", TokenSingleton.Instance);
            List<long> followersIds = u.GetFollowers(true);
            List<IUser> followers = u.Followers;

            Assert.AreNotEqual(followersIds.Count, 0);
            Assert.AreEqual(followersIds.Count, followers.Count);
        }

        [TestMethod]
        [TestCategory("GetFollowers"), TestCategory("User")]
        public void UserGetFollowers1()
        {
            IUser u = new User("linviStevens", TokenSingleton.Instance);
            List<long> followersIds = u.GetFollowers(false);
            List<IUser> followers = u.Followers;

            Assert.AreNotEqual(followersIds.Count, 0);
            Assert.AreEqual(followers.Count, 0);
        }
        #endregion

        #region DownloadProfileImage

        [TestMethod]
        [TestCategory("Image"), TestCategory("User")]
        public void UserDownloadProfileImage()
        {
            Debug.WriteLine(Directory.GetCurrentDirectory());
            
            string userName = "ladygaga";

            if (File.Exists(string.Format("{0}_normal.jpg", userName)))
            {
                File.Delete(string.Format("{0}_normal.jpg", userName));
            }

            IUser u = new User(userName, TokenSingleton.Instance);
            string filepath = u.DownloadProfileImage();

            Assert.AreEqual(filepath, string.Format("{0}_normal.jpg", userName));

            Assert.AreNotEqual(filepath, string.Empty);

            bool fileExist = File.Exists(filepath);
            Assert.AreEqual(fileExist, true);
        }

        #endregion

        #region GetContributors

        [TestMethod]
        [TestCategory("GetContributors"), TestCategory("User")]
        public void UserGetContributors()
        {
            IUser u = new User("ladygaga", TokenSingleton.Instance);
            List<IUser> contributors = u.GetContributors(true);

            Assert.AreNotEqual(contributors, null);
            Assert.AreEqual(contributors.Count, 0);
        }

        #endregion

        #region GetContributees
        
        [TestMethod]
        [TestCategory("GetContributees"), TestCategory("User")]
        public void UserGetContributees()
        {
            IUser u = new User("ladygaga", TokenSingleton.Instance);
            List<IUser> contributees = u.GetContributees(true);

            Assert.AreNotEqual(contributees, null);
            Assert.AreEqual(contributees.Count, 0);
        }

        #endregion

        #region UserTimeline

        [TestMethod]
        [TestCategory("User Timeline"), TestCategory("User")]
        public void UserGetTimeline()
        {
            IUser u = new User("ladygaga", TokenSingleton.Instance);
            List<ITweet> tweets = u.GetUserTimeline();

            Assert.AreNotEqual(tweets, null);
            Assert.AreNotEqual(tweets.Count, 0);
        }

        #endregion
    }
}

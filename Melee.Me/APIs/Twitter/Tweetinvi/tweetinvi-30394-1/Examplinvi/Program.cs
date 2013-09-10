﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using TweetinCore.Enum;
using TweetinCore.Events;
using TweetinCore.Interfaces;
using Tweetinvi;
using TwitterToken;
using UILibrary;
using System.Windows;
using System.Threading;

namespace Examplinvi
{
    class Program
    {
        // ReSharper disable UnusedMember.Local
        #region Token

        #region Execute Query
        /// <summary>
        /// Simple function that uses ExecuteQuery to retrieve information from the Twitter API
        /// </summary>
        /// <param name="token"></param>

        static void ExecuteQuery(IToken token)
        {
            // Retrieving information from Twitter API through Token method ExecuteRequest
            dynamic timeline = token.ExecuteGETQuery("https://api.twitter.com/1/statuses/home_timeline.json");

            // Working on each different object sent as a response to the Twitter API query
            for (int i = 0; i < timeline.Length; ++i)
            {
                Dictionary<String, object> post = timeline[i];
                Console.WriteLine("{0} : {1}\n", i, post["text"]);
            }
        }

        /// <summary>
        /// Function that execute cursor query and send information for each query executed
        /// </summary>
        /// <param name="token"></param>
        static void ExecuteCursorQuery(IToken token)
        {
            // The delegate is a function that will be called for each cursor
            DynamicResponseDelegate del = delegate(dynamic jsonResponse, long previous_cursor, long next_cursor)
            {
                Console.WriteLine(previous_cursor + " -> " + next_cursor + " : " + jsonResponse.Count);
            };

            token.ExecuteCursorQuery("https://api.twitter.com/1/friends/ids.json?user_id=700562792", del);
        }
        #endregion

        #region ErrorHandling

        /// <summary>
        /// Testing the 3 ways to handle errors
        /// </summary>
        /// <param name="token"></param>
        static void TestErrorFunctions(IToken token)
        {
            integrated_error_handler(token);
            token_integrated_error_handler(token);
            execute_query_error_handler(token);
        }

        /// <summary>
        /// Initiating auto error handler
        /// You will not receive error information if handled by default error handler
        /// </summary>
        /// <param name="token"></param>
        static void integrated_error_handler(IToken token)
        {
            token.IntegratedExceptionHandler = true;

            // Error is not automatically handled

            try
            {
                // Calling a method that does not exist
                token.ExecuteGETQuery("https://api.twitter.com/1/users/contributors.json?user_id=700562792");
            }
            catch (WebException wex)
            {
                Console.WriteLine("An error occured!");
                Console.WriteLine(wex);
            }
        }

        /// <summary>
        /// When assigning an error_handler to a Token think that it will be kept alive 
        /// until you specify it does not exist anymore by specifying :
        /// 
        /// token.Integrated_Exception_Handler = false;
        /// 
        /// You can assign null value if you do not want anything to be performed for you
        /// </summary>
        /// <param name="token"></param>
        static void token_integrated_error_handler(IToken token)
        {
            token.ExceptionHandler = delegate(WebException wex)
            {
                Console.WriteLine("You received a Token generated error!");
                Console.WriteLine(wex.Message);
            };

            // Calling a method that does not exist
            token.ExecuteGETQuery("https://api.twitter.com/1/users/contributors.json?user_id=700562792");

            // Reset to basic Handler
            token.IntegratedExceptionHandler = false;
            // OR
            token.ResetExceptionHandler();
        }

        /// <summary>
        /// Uses the handler for only one query / work also for cursor queries
        /// </summary>
        /// <param name="token"></param>
        static void execute_query_error_handler(IToken token)
        {
            WebExceptionHandlingDelegate del = delegate(WebException wex)
            {
                Console.WriteLine("You received an execute_query_error!");
                Console.WriteLine(wex.Message);
            };

            token.ExecuteGETQuery("https://api.twitter.com/1/users/contributors.json?user_id=700562792", null, del);
        }

        #endregion

        #region Rate-Limit

        /// <summary>
        /// Enable you to Get all information from Token and how many query you can execute
        /// Each time a query is executed the XRateLimitRemaining is updated.
        /// To improve efficiency, the other values are NOT.
        /// If you need these please call the function GetRateLimit()
        /// </summary>
        /// <param name="token"></param>
        static void GetRateLimit(IToken token)
        {
            int remaining = token.GetRateLimit();
            Console.WriteLine("Used : " + remaining);
            Console.WriteLine("Used : " + token.XRateLimitRemaining);
            Console.WriteLine("Total per hour : " + token.XRateLimit);
            Console.WriteLine("Time before reset : " + token.XRateLimitResetTimeInSeconds);
        }

        #endregion

        #endregion

        #region Stream

        private static readonly List<Tweet> stream_list = new List<Tweet>();
        private static void processTweet(Tweet tweet, bool force = false)
        {
            if (tweet == null && !force)
                return;

            if (stream_list.Count % 125 != 124 && !force)
            {
                Console.WriteLine(tweet.Creator.Name);
                stream_list.Add(tweet);
            }
            else
            {
                Console.WriteLine("Processing data");
                stream_list.Clear();
            }
        }

        private static void StreamingExample(IToken token)
        {
            // Creating a Delegate to use processTweet function to analyze each Tweet coming from the stream
            Stream.ProcessTweetDelegate produceTweetDelegate = processTweet;
            // Creating the stream and specifying the delegate
            Stream myStream = new Stream(produceTweetDelegate);
            // Starting the stream by specifying credentials thanks to the Token
            myStream.StartStream(token);
        }
        #endregion

        #region User

        #region CreateUser
        public static void createUser(IToken token, long id = 700562792)
        {
            IUser user = new User(id, token);
            Console.WriteLine(user.ScreenName);
        }

        public static void createUser(IToken token, string screen_name = null)
        {
            IUser user = new User(screen_name, token);
            Console.WriteLine(user.Id);
        }

        public static void createUserV2(IToken token, long id = 700562792)
        {
            IUser user = new User(id);
            // Here we need to specify the token to retrieve the information
            // otherwise the information won't be filled
            user.PopulateUser(token);
        }
        #endregion

        #region Get Friends
        /// <summary>
        /// Show a the list of Friends from a userId
        /// </summary>
        /// <param name="token">Credentials</param>
        /// <param name="id">UserId to be analyzed</param>
        private static void GetFriends(IToken token, long id = 700562792)
        {
            User user = new User(id);
            var res = user.GetFriendsIds(token);
            Console.WriteLine("List of friends from " + id);

            foreach (long friend_id in res)
                Console.WriteLine(friend_id);
        }

        private static void GetFriendsUsingUsername(IToken token, string username)
        {
            IUser user = new User(username);
            var res = user.GetFriendsIds(token);
            Console.WriteLine("List of friends from " + username);

            foreach (long friend_id in res)
                Console.WriteLine(friend_id);
        }
        #endregion

        #region Get Followers
        private static void GetFollowers(IToken token, long? id = 700562792)
        {
            IUser user = new User(id);
            var res = user.GetFollowers(token);

            Console.WriteLine(res.Count());
            foreach (long follower_id in res)
            {
                Console.WriteLine(follower_id);
            }
        }

        private static void GetFollowersUsingUsername(IToken token, string username)
        {
            IUser user = new User(username);
            var res = user.GetFollowers(token);

            Console.WriteLine(res.Count());
            foreach (long follower_id in res)
            {
                Console.WriteLine(follower_id);
            }
        }
        #endregion

        #region Get Profile Image

        static void GetProfileImage(IToken token)
        {
            User ladygaga = new User("ladygaga", token);
            string filePath = ladygaga.DownloadProfileImage(ImageSize.original);

            System.Diagnostics.Process.Start(filePath);
        }

        #endregion

        #region Get Contributors
        static void GetContributors(IToken token, long? id = 700562792, string screen_name = null, bool createContributorList = false)
        {
            IUser user;

            if (id != null)
            {
                user = new User(id, token);
            }
            else
            {
                user = new User(screen_name, token);
            }

            IList<IUser> contributors = user.GetContributors(createContributorList);
            IList<IUser> contributorsAttribute = user.Contributors;
            if (createContributorList && contributors != null)
            {
                if (contributorsAttribute == null ||
                    !contributors.Equals(contributorsAttribute))
                {
                    Console.WriteLine("The object attribute should be identical to the method result");
                }
            }
            if (contributors != null)
            {
                foreach (User c in contributors)
                {
                    Console.WriteLine("contributor id = " + c.Id + " - screen_name = " + c.ScreenName);
                }
            }
        }
        #endregion

        #region Get Contributees

        static void GetContributees(IToken token, long? id = 700562792, string screen_name = null, bool createContributeeList = false)
        {
            IUser user;
            if (id != null)
            {
                user = new User(id, token);
            }
            else
            {
                user = new User(screen_name, token);
            }
            IList<IUser> contributees = user.GetContributees(createContributeeList);
            IList<IUser> contributeesAttribute = user.Contributees;
            if (createContributeeList)
            {
                if ((contributees == null && contributeesAttribute != null) ||
                    (contributees != null && contributeesAttribute == null) ||
                    (contributees != null && !contributees.Equals(contributeesAttribute)))
                {
                    Console.WriteLine("The object attribute should be identical to the method result");
                }
            }
            if (contributees != null)
            {
                foreach (User c in contributees)
                {
                    Console.WriteLine("contributee id = " + c.Id + " - screen_name = " + c.ScreenName);
                }
            }
        }
        #endregion

        #region Get Direct Messages Sent
        static void GetDirectMessagesSent(IToken token)
        {
            ITokenUser user = new TokenUser(token);
            IList<IMessage> dmSent = user.GetLatestDirectMessagesSent();
            IList<IMessage> dmSentAttribute = user.LatestDirectMessagesSent;


            if ((dmSent == null && dmSentAttribute != null) ||
                (dmSent != null && dmSentAttribute == null) ||
                (dmSent != null && !dmSent.Equals(dmSentAttribute)))
            {
                Console.WriteLine("The object's attribute should be identical to the method result");
            }

            if (dmSent != null)
            {
                foreach (Message m in dmSent)
                {
                    Console.WriteLine("message id = " + m.Id + " - text = " + m.Text);
                }
            }
        }
        #endregion

        #region Get Direct Received
        static void GetDirectMessagesReceived(IToken token)
        {
            ITokenUser user = new TokenUser(token);
            IList<IMessage> dmReceived = user.GetLatestDirectMessagesReceived();
            IList<IMessage> dmReceivedAtrribute = user.LatestDirectMessagesReceived;

            if ((dmReceived == null && dmReceivedAtrribute != null) ||
                (dmReceived != null && dmReceivedAtrribute == null) ||
                (dmReceived != null && !dmReceived.Equals(dmReceivedAtrribute)))
            {
                Console.WriteLine("The object's attribute should be identical to the method result");
            }

            if (dmReceived != null)
            {
                foreach (Message m in dmReceived)
                {
                    Console.WriteLine("message id = " + m.Id + " - text = " + m.Text);
                }
            }
        }
        #endregion

        #region GetHomeTimeline

        static void GetHomeTimeline(IToken token)
        {
            ITokenUser u = new TokenUser(token);
            IList<ITweet> homeTimeline = u.GetHomeTimeline(20, true, true);

            Console.WriteLine(u.LatestHomeTimeline.Count);

            foreach (ITweet tweet in homeTimeline)
            {
                Console.WriteLine("\n\n{0}", tweet.Text);
            }
        }

        #endregion

        #region Get Timeline
        static void GetTimeline(IToken token, long id = 700562792, bool createTimeline = false)
        {
            IUser user = new User(id, token);
            IList<ITweet> timeline = user.GetUserTimeline(createTimeline);
            IList<ITweet> timelineAttribute = user.Timeline;
            if (createTimeline)
            {
                if ((timeline == null && timelineAttribute != null) ||
                    (timeline != null && timelineAttribute == null) ||
                    (timeline != null && !timeline.Equals(timelineAttribute)))
                {
                    Console.WriteLine("The object's attribute should be identical to the method result");
                }
            }
            if (timeline != null)
            {
                foreach (Tweet t in timeline)
                {
                    Console.WriteLine("tweet id = " + t.Id + " - text = " + t.Text + " - is retweet = " + t.Retweeted);
                }
            }
        }
        #endregion

        #region Get Mentions
        static void GetMentions(IToken token)
        {
            ITokenUser user = new TokenUser(token);
            IList<IMention> mentions = user.GetLatestMentionsTimeline();
            IList<IMention> mentionsAttribute = user.LatestMentionsTimeline;


            if ((mentions == null && mentionsAttribute != null) ||
                (mentions != null && mentionsAttribute == null) ||
                (mentions != null && !mentions.Equals(mentionsAttribute)))
            {
                Console.WriteLine("The object's attribute should be identical to the method result");
            }

            if (mentions != null)
            {
                foreach (Mention m in mentions)
                {
                    Console.WriteLine("tweet id = " + m.Id + " - text = " + m.Text + " - annotations = " + m.Annotations);
                }
            }
        }

        #endregion

        #region Get Blocked users
        static void GetBlockedUsers(IToken token, bool createBlockedUsers = true, bool createdBlockedUsersIds = true)
        {
            TokenUser user = new TokenUser(token);

            IList<IUser> blockedUsers = user.GetBlockedUsers(createBlockedUsers, createdBlockedUsersIds);
            if (blockedUsers == null)
            {
                return;
            }
            if (createBlockedUsers)
            {
                if (blockedUsers != user.BlockedUsers)
                {
                    Console.WriteLine("The object's attribute should be identical to the method result");
                }
            }

            foreach (IUser bu in blockedUsers)
            {
                Console.WriteLine("user id = " + bu.Id + " - user screen name = " + bu.ScreenName);
            }
        }

        static void GetBlockedUsersIds(IToken token, bool createdBlockedUsersIds = true)
        {
            TokenUser user = new TokenUser(token);

            IList<long> ids = user.GetBlockedUsersIds(createdBlockedUsersIds);
            if ((createdBlockedUsersIds) && (ids != user.BlockedUsersIds))
            {
                Console.WriteLine("The object's attribute should be identical to the method result");
            }

            foreach (long id in ids)
            {
                Console.WriteLine("user id = " + id);
            }
        }
        #endregion

        #region Get Suggested User (list and members)
        static void GetSuggestedUserList(IToken token, bool createSuggestedUserList = true)
        {
            ITokenUser user = new TokenUser(token);

            IList<ISuggestedUserList> suggUserList = user.GetSuggestedUserList(createSuggestedUserList);
            if ((createSuggestedUserList) && (!suggUserList.Equals(user.SuggestedUserList)))
            {
                Console.WriteLine("The object's attribute should be identical to the method result");
            }

            foreach (ISuggestedUserList sul in suggUserList)
            {
                Console.WriteLine("name = " + sul.Name + " ; slug = " + sul.Slug + " ; size = " + sul.Size);
            }

        }

        static void GetSuggestedUserListDetails(IToken token, string slug)
        {
            SuggestedUserList sul = new SuggestedUserList("fake", slug, 0);
            sul.RefreshAll(token);

            Console.WriteLine("name = " + sul.Name + " ; slug = " + sul.Slug + " ; size = " + sul.Size);
            foreach (User su in sul.Members)
            {
                Console.WriteLine("Suggested user: id = " + su.Id + " ; screen name = " + su.ScreenName);
            }
        }

        static void GetSuggestedUserListMembers(IToken token, string slug)
        {
            SuggestedUserList sul = new SuggestedUserList("fake", slug, 0);
            sul.RefreshMembers(token);

            foreach (User su in sul.Members)
            {
                Console.WriteLine("Suggested user: id = " + su.Id + " ; screen name = " + su.ScreenName);
            }
        }
        #endregion

        #endregion

        #region Tweet

        #region Publish Tweet

        public static void PublishTweet(IToken token)
        {
            ITweet t = new Tweet("Hello Tweetinvi2!");
            // token.Integrated_Exception_Handler = true;
            Console.WriteLine("Tweet has{0}been published", t.Publish(token) ? " " : " not ");
        }

        public static void PublishTweetWithGeo(IToken token)
        {
            // Create Tweet locally
            ITweet tweet = new Tweet(String.Format("Hello Tweetinvi With Geo {0}", DateTime.Now));

            double latitude = 37.7821120598956;
            double longitude = -122.400612831116;

            // Send the Tweet
            Console.WriteLine("Tweet has{0}been published",
                tweet.PublishWithGeo(latitude, longitude, true, token) ? " " : " not ");
        }

        public static void PublishInReplyTo(IToken token)
        {
            // Create Tweet locally
            ITweet tweet = new Tweet(String.Format("Hello Tweetinvi {0}", DateTime.Now), token);

            // Send the Tweet
            bool result = tweet.Publish();

            if (result)
            {
                ITweet reply = new Tweet(String.Format("Nice speech Tweetinvi {0}", DateTime.Now), token);

                result &= reply.PublishInReplyTo(tweet);
            }

            Console.WriteLine(result);
        }

        #endregion

        #region Retrieve an existing Tweet
        private static void GetTweetById(IToken token)
        {
            // This tweet has classic entities
            Tweet tweet1 = new Tweet(127512260116623360, token);
            Console.WriteLine(tweet1.Text);

            // This tweet has media entity
            try
            {
                Tweet tweet2 = new Tweet(112652479837110270, token);
                Console.WriteLine(tweet2.Text);
            }
            catch (WebException)
            {
                Console.WriteLine("Tweet has not been created!");
            }
        }
        #endregion

        #region Publish Retweet

        private static void PublishAndDestroyRetweet(IToken token)
        {
            IUser tweetinviApi = new User("tweetinviApi", token);
            List<ITweet> tweets = tweetinviApi.GetUserTimeline();

            // Retweeting the last tweet of TweetinviApi
            ITweet t = tweets[0];

            // Retweet is the tweet posted on the TokenUser timeline
            ITweet retweet = t.PublishRetweet();

            // Destroying the retweet
            retweet.Destroy();
        }

        #endregion

        #region Get Retweets of Tweet

        private static void Get_retweet_of_tweet(IToken token, long id)
        {
            ITweet tweet1 = new Tweet(id, token);
            IList<ITweet> retweets = tweet1.GetRetweets();
            foreach (Tweet r in retweets)
            {
                Console.WriteLine("tweet id  = " + r.Id + " - text = " + r.Text);
            }
        }
        #endregion

        #region Favourites

        private static void CreateFavouriteTweet(IToken token)
        {
            ITweet newTweet = new Tweet(String.Format("Favouriting tweet {0}", DateTime.Now), token);
            newTweet.Publish();
            newTweet.Favourited = true;
        }

        private static void GetFavouriteTweet(IToken token)
        {
            IUser user = new User("ladygaga", token);
            List<ITweet> tweets = user.GetFavourites();

            foreach (var tweet in tweets)
            {
                Console.WriteLine(tweet);
            }
        }

        private static void GetFavouriteSince(IToken token)
        {
            string text = String.Format("Favouriting tweet {0}", DateTime.Now);

            // Create and favourite a first tweet
            ITweet tweet1 = new Tweet(text, token);
            tweet1.Publish();
            tweet1.Favourited = true;

            ITweet tweet2 = new Tweet(text + " - 2", token);
            tweet2.Publish();
            tweet2.Favourited = true;

            ITweet tweet3 = new Tweet(text + " - 3", token);
            tweet3.Publish();
            tweet3.Favourited = true;

            IUser creator = tweet1.Creator;

            List<ITweet> favouritesSinceId = creator.GetFavouritesSinceId(tweet1.Id);

            // Should return the last 2 tweets

            foreach (var tweet in favouritesSinceId)
            {
                Console.WriteLine(tweet.ToString());
            }
        }

        #endregion

        #endregion

        #region Direct Message

        #region Message creation

        // Create a message and retrieve it from Twitter
        private static void Get_message(IToken token, long messageId)
        {
            IMessage m = new Message(messageId, token);

            Console.WriteLine("message text = " + m.Text + " ; receiver = " + m.Receiver.ScreenName + " ; sender = " + m.Sender.ScreenName);
        }

        // Create a new message
        private static IMessage createNewMessage()
        {
            IUser receiver = new User(543118219);
            IMessage msg = new Message(
                String.Format("Hello from Tweetinvi! ({0})", DateTime.Now.ToShortTimeString()),
                receiver);

            return msg;
        }

        #endregion

        #region Send Message

        private static void SendMessage(IToken token)
        {
            IMessage msg = createNewMessage();
            msg.Send(token);
        }

        #endregion

        #endregion

        #region Tweetinvi API

        #endregion

        #region Search

        #region User

        private static void SearchUser(IToken token)
        {
            string searchQuery = "tweetinvi";

            IUserSearchEngine searchEngine = new UserSearchEngine(token);
            List<IUser> searchResult = searchEngine.Search(searchQuery);

            foreach (var user in searchResult)
            {
                Console.Write(user.ScreenName);
            }
        }

        #endregion

        #endregion

        // BRAND NEW - GENERATE YOUR TOKEN!
        #region Token Generator

        public static int GetCaptcha(string validationUrl)
        {
            int result = -1;

            Thread enterCaptchaThread = new Thread(() =>
            {
                Application app = new Application();
                result = app.Run(new ValidateApplicationCaptchaWindow(validationUrl));
            });

            enterCaptchaThread.SetApartmentState(ApartmentState.STA);
            enterCaptchaThread.Start();
            enterCaptchaThread.Join();

            return result;
        }

        public static void GenerateToken(IToken consumerToken)
        {
            Console.WriteLine("Starting Token Generation...");
            ITokenCreator creator = new TokenCreator(consumerToken.ConsumerKey,
                                                     consumerToken.ConsumerSecret);

            Console.WriteLine("Please enter the verifier key...");
            IToken newToken = creator.CreateToken(GetCaptcha);

            if (newToken != null)
            {
                Console.WriteLine("Token generated!");
                ITokenUser loggedUser = new TokenUser(newToken);
                Console.WriteLine("Your name is {0}!", loggedUser.ScreenName);
            }
            else
            {
                Console.WriteLine("Token could not be generated. Please login and specify your verifier key!");
            }
        } 
        #endregion

        /// <summary>
        /// Run a basic application to provide a code example
        /// </summary>
        static void Main()
        {
            // Initializing a Token with Twitter Credentials
            IToken token = new Token(
                ConfigurationManager.AppSettings["token_AccessToken"],
                ConfigurationManager.AppSettings["token_AccessTokenSecret"],
                ConfigurationManager.AppSettings["token_ConsumerKey"],
                ConfigurationManager.AppSettings["token_ConsumerSecret"]);

            // TEST IT
            GenerateToken(token);

            #region User Examples

            // User
            //createUser(token, "StevensDev");

            // Friends
            //GetFriends(token, 579529593);
            //GetFriendsUsingUsername(token, "StevensDev");

            // Followers
            //GetFollowers(token, 579529593);
            //GetFollowersUsingUsername(token, "StevensDev");

            // Contributors
            //GetContributors(token, 30973, null, true);
            //GetContributors(token, null, "Starbucks", true);
            //GetContributees(token, 15483731, null, true);
            //GetContributees(token, null, "LeeAdams", true);

            // TimeLines
            //GetTimeline(token, 579529593, true);
            //GetMentions(token);

            // Tweets
            //Get_retweet_of_tweet(token, 173198765052792833);
            //GetFavouriteSince(token);

            // List
            //GetSuggestedUserListDetails(token, "us-election-2012");
            //GetSuggestedUserListMembers(token, "us-election-2012");

            // Images
            //GetProfileImage(token);

            #endregion

            #region TokenUser Examples

            //GetHomeTimeline(token);
            //GetDirectMessagesReceived(token);
            //GetDirectMessagesSent(token);
            //GetBlockedUsers(token, true, true);
            //GetBlockedUsersIds(token, true);
            //GetSuggestedUserList(token, true);

            #endregion

            #region Tweet Examples

            //GetTweetById(token);
            //PublishTweet(token);
            //PublishTweetWithGeo(token);
            //PublishInReplyTo(token);
            //PublishAndDestroyRetweet(token);
            //CreateFavouriteTweet(token);
            //GetFavouriteTweet(token);

            #endregion

            #region Message Examples
            
            //Get_message(token, 347015339323842560);
            //SendMessage(token);
            //GetDirectMessagesSent(token);
            //GetDirectMessagesReceived(token);

            #endregion

            #region Streaming Examples

            // StreamingExample(token);

            #endregion

            #region SearchUser Examples

            // SearchUser(token);

            #endregion


            Console.WriteLine("End");
            Console.ReadKey();
        }
    }
}
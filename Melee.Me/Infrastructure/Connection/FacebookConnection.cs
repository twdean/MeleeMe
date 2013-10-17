using System.Collections.Generic;
using System.Linq;
using Facebook;
using Melee.Me.Models;

namespace Melee.Me.Infrastructure.Connection
{
    public class FacebookConnection : IConnection
    {
        public string AccessToken { get; set; }

        public double GetScore(UserModel meleeUser)
        {
            AccessToken = meleeUser.Connections.Single(x => x.ConnectionName == "Facebook").AccessToken;
            double score = (0.5 * GetFacebookFriendCount());
            score += (3 * GetFacebookLikeCount());
            score += (0.5 * GetFacebookPostCount());
            score += GetFacebookCheckInCount();
            score += GetFacebookPictureCount();

            return score;
        }

        private int GetFacebookFriendCount()
        {
            const int batchSize = 50;
            var parameters = new Dictionary<string, object>();
            var friendCount = 0;

            var client = new FacebookClient(AccessToken);

            for (long q = 0; q < 5000; q += batchSize)
            {
                parameters["limit"] = batchSize;
                parameters["offset"] = q;

                dynamic me = client.Get("me/friends", parameters);
                friendCount += ((IEnumerable<dynamic>)me.data).Count();

                if (friendCount < q)
                {
                    break;
                }
            }

            return friendCount;
        }

        private int GetFacebookLikeCount()
        {
            const int batchSize = 50;
            var parameters = new Dictionary<string, object>();
            var likesCount = 0;

            var client = new FacebookClient(AccessToken);

            for (long q = 0; q < 5000; q += batchSize)
            {
                parameters["limit"] = batchSize;
                parameters["offset"] = q;

                dynamic me = client.Get("me/likes", parameters);
                likesCount += ((IEnumerable<dynamic>)me.data).Count();

                if (likesCount < q)
                {
                    break;
                }
            }

            return likesCount;

        }

        private int GetFacebookPostCount()
        {
            const int batchSize = 50;
            var parameters = new Dictionary<string, object>();
            var postCount = 0;

            var client = new FacebookClient(AccessToken);

            for (long q = 0; q < 5000; q += batchSize)
            {
                parameters["limit"] = batchSize;
                parameters["offset"] = q;
                parameters["type"] = "post";

                dynamic me = client.Get("me/posts", parameters);
                postCount += ((IEnumerable<dynamic>)me.data).Count();

                if (postCount < q)
                {
                    break;
                }
            }

            return postCount;

        }

        private int GetFacebookCheckInCount()
        {
            const int batchSize = 50;
            var parameters = new Dictionary<string, object>();
            var checkinCount = 0;

            var client = new FacebookClient(AccessToken);

            for (long q = 0; q < 5000; q += batchSize)
            {
                parameters["limit"] = batchSize;
                parameters["offset"] = q;

                dynamic me = client.Get("me/locations", parameters);
                checkinCount += ((IEnumerable<dynamic>)me.data).Count();

                if (checkinCount < q)
                {
                    break;
                }
            }

            return checkinCount;

        }

        private int GetFacebookPictureCount()
        {
            const int batchSize = 50;
            var parameters = new Dictionary<string, object>();
            var photoCount = 0;

            var client = new FacebookClient(AccessToken);

            for (long q = 0; q < 5000; q += batchSize)
            {
                parameters["limit"] = batchSize;
                parameters["offset"] = q;

                dynamic me = client.Get("me/photos", parameters);
                photoCount += ((IEnumerable<dynamic>)me.data).Count();

                if (photoCount < q)
                {
                    break;
                }
            }

            return photoCount;

        }




    }
}
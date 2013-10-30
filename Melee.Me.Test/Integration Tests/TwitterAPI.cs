using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Melee.Me.Test.Integration_Tests
{
    public class TwitterAPI
    {

        private void CreateAuthRequest()
        {
            //Oauth Keys (Replace with values that are obtained from registering the application
            //https://dev.twitter.com/apps/new

            var oauth_consumer_key = "consumerkey goes here";
            var oauth_consumer_secret = "consumer secret here";

            //Token URL
            var oauth_url = "https://api.twitter.com/oauth2/token";


            var headerFormat = "Basic {0}";

            var authHeader = string.Format(headerFormat,
                 Convert.ToBase64String(Encoding.Unicode.GetBytes(Uri.EscapeDataString(oauth_consumer_key) + ":" +
                        Uri.EscapeDataString((oauth_consumer_secret)))
                        ));

            var postBody = "grant_type=client_credentials";

            ServicePointManager.Expect100Continue = false;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(oauth_url);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

            using (Stream stream = request.GetRequestStream())
            {
                byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
                stream.Write(content, 0, content.Length);
            }

            request.Headers.Add("Accept-Encoding", "gzip");

            WebResponse response = request.GetResponse();   //Always Getting 500 Response Error
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using LinqToTwitter;
using LitJson;
using System.Web;
using System.Threading;

namespace TwitterRT
{
    class TwitterAPI
    {
        /* vars */
        private string twitterConsumerKey;
        private string twitterConsumerSecret;
        private string twitterOAuthToken;
        private string twitterAccessToken;
        private TwitterContext twtCtx;
        private ITwitterAuthorizer auth;

        public TwitterAPI(string twitterConsumerKey, string twitterConsumerSecret, string twitterOAuthToken, string twitterAccessToken)
        {
            this.twitterConsumerKey = twitterConsumerKey;
            this.twitterConsumerSecret = twitterConsumerSecret;
            this.twitterOAuthToken = twitterOAuthToken;
            this.twitterAccessToken = twitterAccessToken;

            this.DoSingleUserAuth();
        }

        private void DoSingleUserAuth()
        {
            // configure the OAuth object
            this.auth = new SingleUserAuthorizer
            {
                Credentials = new InMemoryCredentials
                {
                    ConsumerKey = this.twitterConsumerKey,
                    ConsumerSecret = this.twitterConsumerSecret,
                    OAuthToken = this.twitterOAuthToken,
                    AccessToken = this.twitterAccessToken
                }
            };

            // do not call authorize
            // auth.Authorize();
            this.twtCtx = new TwitterContext(this.auth);
        }

        public void StreamData(List<string> follow, Action<string,string,string> processAction)
        {
            if (this.auth == null) return;
            StreamContent strmCont = null;
            this.twtCtx.Timeout = 3000;
            this.twtCtx.AuthorizedClient.UseCompression = false;
            Console.WriteLine("\nStreaming Content: \n");

            (from strm in this.twtCtx.UserStream
             where strm.Type == UserStreamType.User //&&
                 //strm.With == "followings" &&
                   /*strm.Follow == follow , "16761255"*/
             select strm)
            .StreamingCallback(strm =>
            {
                if (strm.Status == TwitterErrorStatus.RequestProcessingException)
                {
                    Console.WriteLine(strm.Error.ToString());
                    return;
                }

                var json = JsonMapper.ToObject(strm.Content);
                var jsonDict = json as IDictionary<string, JsonData>;

                if (jsonDict != null && jsonDict.ContainsKey("id_str") && jsonDict.ContainsKey("user"))
                {
                    foreach (string followUser in follow)
                    {
                        if (json["user"]["screen_name"].ToString() == followUser)
                        {
                            string id_tweet = json["id_str"].ToString();
                            string statusText = json["text"].ToString();
                            statusText = HttpUtility.HtmlDecode(statusText);
                            string statusUsername = json["user"]["screen_name"].ToString(); 
                            Console.WriteLine("TWEET ID: " + id_tweet);
                            Console.WriteLine("USER: " + statusUsername);
                            Console.WriteLine(statusText + "\n");
                            if (processAction != null) processAction(id_tweet, statusText, statusUsername);
                            break;
                        }
                    }
                }
            })
            .SingleOrDefault();

            while (strmCont == null)
            {
                //Console.WriteLine("Waiting on stream to initialize.");

                Thread.Sleep(10000);
            }

            //Console.WriteLine("Stream is initialized. Now closing...");
            strmCont.CloseStream();
        }

        public string Follow(string username)
        {
            var user = this.twtCtx.CreateFriendship(string.Empty, username, true);

            Console.WriteLine(
                "User Name: {0}\nLast status: {1}\nAt: {2}\n\n",
                user.Name,
                user.Status.Text,
                user.Status.CreatedAt);

            return user.Identifier.UserID;
        }
    }
}

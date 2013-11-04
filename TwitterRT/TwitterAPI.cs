using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using LinqToTwitter;
using LitJson;
using System.Threading;
using System.Net;

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
        public static int timeoutMSRateLimit = 900000; //15 minutes

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

            try
            {
                // do not call authorize
                // auth.Authorize();
                this.twtCtx = new TwitterContext(this.auth);
            }
            catch (TwitterQueryException ex)
            {
                System.Console.WriteLine("\nCannot login with those credentials: {0}", ex.Message);
                throw;
            }
        }

        public bool Favorite(string id)
        {
            /*
            while (this.returnRateLimitLeft("favorite") < 1)
            {
                Console.WriteLine("\n\nRate limit reached waiting {0}", this.timeoutMSRateLimit.ToString());
                Thread.Sleep(this.timeoutMSRateLimit);
            }*/

            bool success = true;

            try
            {
                var status = this.twtCtx.CreateFavorite(id);
            }
            catch (TwitterQueryException ex)
            {
                success = false;/*
                // TwitterQueryException will always reference the original
                // WebException, so the check is redundant but doesn't hurt
                var webEx = ex.InnerException as WebException;
                if (webEx == null) throw ex;

                // The response holds data from Twitter
                var webResponse = webEx.Response as HttpWebResponse;
                if (webResponse == null) throw ex;

                if (webResponse.StatusCode == HttpStatusCode)
                {
                    //Console.WriteLine("User: {0}, Tweet: {1}", status.User.Name, status.Text);
                }*/
            }

            return success;
            //Console.WriteLine("User: {0}, Tweet: {1}",status.User.Name, status.Text);
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

        public bool GetTweetFromId(string id)
        {
            while (this.returnRateLimitLeft("statuses", "/statuses/show/:id") < 1)
            {
                Console.WriteLine("\n\nRate limit reached waiting {0}", TwitterAPI.timeoutMSRateLimit.ToString());
                Thread.Sleep(TwitterAPI.timeoutMSRateLimit);
            }

            try
            {
                var tweetsList =
                    from tweet in this.twtCtx.Status
                    where tweet.Type == StatusType.Show &&
                          tweet.ID == id
                    select tweet;

                Console.WriteLine("\nRequested Tweet: \n");
                foreach (var tweet in tweetsList)
                {
                    Program.tweetIdVerified = true;

                    Console.WriteLine(
                        "User: " + tweet.User.Name +
                        "\nTweet: " + tweet.Text +
                        "\nTweet ID: " + tweet.ID + "\n");

                    return true;
                }
            }
            catch (TwitterQueryException ex)
            {
                Console.WriteLine("\nERROR: tweet not found! {0}\n", ex.Message);
            }

            return false;
        }

        public int returnRateLimitLeft(string endPointResource, string resource)
        {
            var helpResult =
                (from help in this.twtCtx.Help
                 where help.Type == HelpType.RateLimits &&
                 help.Resources == endPointResource
                 select help)
                .SingleOrDefault();

            foreach (var category in helpResult.RateLimits)
            {
                foreach (var limit in category.Value)
                {
                    if (limit.Resource != resource) continue;
                    
                    Console.WriteLine(
                        "\n  Resource: {0}\n    Remaining: {1}\n    Reset: {2}\n    Limit: {3}",
                        limit.Resource, limit.Remaining, limit.Reset, limit.Limit);

                    return limit.Limit;
                }
            }

            return 1;
        }
    }
}

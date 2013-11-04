using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwitterRT
{
    class TweetBot
    {
        public string tweet;
        public string statusUpdate;
        public string statusTweetId;
        public string statusUsername;
        public string proxyhost;
        public int proxyport;
        public string proxyusername;
        public string proxypassword;
        public string username;
        public string password;

        //tmp vars
        public string twitterCreds;
        public string proxy;

        public TweetBot(string tweet, string statusUpdate, string proxy, string twitterCreds, string statusUsername, string statusTweetId)
        {
            //instantiate vars
            this.proxyusername = "";
            this.proxypassword = "";
            this.proxyhost = "";
            //default proxy port
            this.proxyport = 1136;

            this.tweet = tweet;
            this.statusUpdate = statusUpdate;
            this.statusUsername = statusUsername;
            this.statusTweetId = statusTweetId;
            this.twitterCreds = twitterCreds;
            this.proxy = proxy;
        }

        public void setUp()
        {
            string[] strproxy = this.proxy.Split('|');

            //proxy host

            //proxy username
            if (strproxy.Length > 1)
            {
                this.proxyusername = strproxy[1];
            }

            //proxy password
            if (strproxy.Length > 2)
            {
                this.proxypassword = strproxy[2];
            }
        }

        public void threadRun()
        {
            TwitterHTTP twitterRequest = new TwitterHTTP(this.proxyhost, this.proxyport, this.proxyusername, this.proxypassword);

            this.tweet = this.tweet.Replace("{tweetUsername}", this.statusUsername);
            this.tweet = this.tweet.Replace("{random}", Helpers.RandomString(4));
            this.tweet = this.tweet.Replace("{tweet}", this.statusUpdate);

            this.tweet = Helpers.Spintax(this.tweet);

            bool success = twitterRequest.doLogin(this.username, this.password);
            if (success) success = twitterRequest.postTweet(this.tweet, this.statusTweetId);
            if (!success)
            {
                Console.WriteLine("An error occurred posting. check httpSessionLog.txt");
            }
            Console.WriteLine("done.\n");
        }
    }
}

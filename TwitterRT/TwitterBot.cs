using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwitterRT
{
    class TwitterBot
    {
        /* protected vars */
        protected string username;
        protected string password;
        protected string proxyhost;
        protected int proxyport;
        protected string proxyusername;
        protected string proxypassword;
        protected TwitterHTTP twitterRequest;

        /* public vars */
        public string tweetPost;
        public string tweetId;

        /* tmp vars */
        public string tmp_proxycreds;
        public string tmp_twittercreds;

        public TwitterBot(string proxycreds, string twittercreds, string tweetPost, string tweetId)
        {
            this.tmp_proxycreds = proxycreds;
            this.tmp_twittercreds = twittercreds;
            this.tweetPost = tweetPost;
            this.tweetId = tweetId;

            //default vars
            this.proxyport = 11368;
        }

        public void runThread()
        {
            //setup
            this.setup();

            bool success = this.twitterRequest.doLogin(this.username, this.password);
            if (success) success = this.twitterRequest.postTweet(this.tweetPost, this.tweetId);
            if (!success)
            {
                Console.WriteLine("An error occurred posting. check httpSessionLog.txt");
            }

            return;
        }

        protected void setup()
        {


            this.twitterRequest = new TwitterHTTP(this.proxyhost, this.proxyport, this.proxyusername, this.proxypassword);
                
            return;
        }
    }
}

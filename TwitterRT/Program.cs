using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TwitterRT
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;

            try
            {
                string[] configLines = System.IO.File.ReadAllLines("tweetbot.config.txt");

                if (configLines.Length < 11)
                {
                    Console.WriteLine("ERROR loading config file");
                    Program.exit();
                }

                Console.WriteLine("##################################");
                Console.WriteLine("####  NAMBOKINATOR TWEET BOT  ####");
                Console.WriteLine("##################################\n\n");
                string proxy = configLines[0];
                Console.WriteLine("Proxy server: " + proxy);
                int proxyPort = int.Parse(configLines[1]);
                Console.WriteLine("Proxy server port: " + proxyPort);
                string proxyUsername = configLines[2];
                Console.WriteLine("Proxy username: " + proxyUsername);
                string proxyPassword = configLines[3];
                Console.WriteLine("Proxy password: " + proxyPassword);
                string username = configLines[4];
                Console.WriteLine("Username: " + username);
                string password = configLines[5];
                Console.WriteLine("Password: " + password);
                string tweet = configLines[6];
                Console.WriteLine("Tweet: " + tweet);
                string userfollow = configLines[7];
                Console.WriteLine("Follow User: " + userfollow);
                string twitterConsumerKey = configLines[8];
                Console.WriteLine("twitterConsumerKey: " + twitterConsumerKey);
                string twitterConsumerSecret = configLines[9];
                Console.WriteLine("twitterConsumerSecret: " + twitterConsumerSecret);
                string twitterOAuthToken = configLines[10];
                Console.WriteLine("twitterOAuthToken: " + twitterOAuthToken);
                string twitterAccessToken = configLines[11];
                Console.WriteLine("twitterAccessToken: " + twitterAccessToken + "\n");
                Console.WriteLine("\n=================================\n\n");
                
                //get a list of usernames to follow
                List<string> userfollowList = userfollow.Split('|').ToList<string>();
                
                TwitterAPI twitterAPI = new TwitterAPI(twitterConsumerKey, twitterConsumerSecret, twitterOAuthToken, twitterAccessToken);
                    
                foreach (string itemUser in userfollowList) // Loop through List with foreach
                {
                    Console.WriteLine("Following user " + itemUser + "....");
                    string userFollowId = twitterAPI.Follow(itemUser);

                    if (userFollowId == null || userFollowId == String.Empty)
                    {
                        Console.WriteLine("ERROR: userid not found for " + itemUser);
                        continue;
                    }
                }

                twitterAPI.StreamData(userfollowList, delegate(string tweetId, string statusUpdate, string statusUsername)
                {
                    string tweetPost = tweet;
                    if (statusUpdate.Length > 90) statusUpdate = statusUpdate.Substring(0, 85) + "...";

                    tweetPost = tweetPost.Replace("{tweetUsername}", statusUsername);
                    tweetPost = tweetPost.Replace("{random}", Helpers.RandomString(4));
                    tweetPost = tweetPost.Replace("{tweet}", statusUpdate);

                    tweetPost = Helpers.Spintax(tweetPost);

                    Console.WriteLine("TWEET RECEIVED ID: " + tweetId);
                    Console.WriteLine("POST STATUS UPDATE: " + tweetPost);
                    TwitterHTTP twitterRequest = new TwitterHTTP(proxy, proxyPort, proxyUsername, proxyPassword);
                    bool success = twitterRequest.doLogin(username, password);
                    if (success) success = twitterRequest.postTweet(tweetPost, tweetId);
                    if (!success)
                    {
                        Console.WriteLine("An error occurred posting. check httpSessionLog.txt");
                        //Program.exit();
                    }
                    Console.WriteLine("done.\n");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }

            Program.exit();
        }

        static void exit()
        {
            Console.Write("\n\nPress any key to end program . . . ");
            Console.ReadKey(true);
            Environment.Exit(0);
        }
    }
}

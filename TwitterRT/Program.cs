using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Chilkat;

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

                if (configLines.Length != 10)
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
                string username = configLines[2];
                Console.WriteLine("Username: " + username);
                string password = configLines[3];
                Console.WriteLine("Password: " + password);
                string tweet = configLines[4];
                Console.WriteLine("Tweet: " + tweet);
                string userid = configLines[5];
                Console.WriteLine("Follow Uid: " + userid);
                string twitterConsumerKey = configLines[6];
                Console.WriteLine("twitterConsumerKey: " + twitterConsumerKey);
                string twitterConsumerSecret = configLines[7];
                Console.WriteLine("twitterConsumerSecret: " + twitterConsumerSecret);
                string twitterOAuthToken = configLines[8];
                Console.WriteLine("twitterOAuthToken: " + twitterOAuthToken);
                string twitterAccessToken = configLines[9];
                Console.WriteLine("twitterAccessToken: " + twitterAccessToken + "\n");
                Console.WriteLine("\n=================================\n\n");
                bool success;

                TwitterAPI twitterAPI = new TwitterAPI(twitterConsumerKey,twitterConsumerSecret,twitterOAuthToken,twitterAccessToken);
                Console.WriteLine("Following user....");
                twitterAPI.Follow(username);
                twitterAPI.StreamData(userid, delegate(string tweetId) { 
                    Console.WriteLine("TWEET RECEIVED ID: " + tweetId);
                    TwitterHTTP twitterRequest = new TwitterHTTP(proxy, proxyPort);
                    success = twitterRequest.doLogin(username, password);
                    if( success) success = twitterRequest.postTweet(tweet, tweetId);
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

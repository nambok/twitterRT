using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Data;
using System.Threading;

namespace TwitterRT
{
    class Program
    {
        static string twitterConsumerKey;
        static string twitterConsumerSecret;
        static string tweetID;
        static int usersLimit;
        static int minutesInactivity;
        static int errorCountLimit;
        public static int errorRequestCountLimit;
        public static int errorTotalCount;
        public static bool tweetIdVerified;

        static void Main(string[] args)
        {
            System.Console.ForegroundColor = System.ConsoleColor.DarkGreen;
            
            //app settings
            twitterConsumerKey = "v3mwfdCQMRbRlbcymw640Q";
            twitterConsumerSecret = "cJVM08RRvD0gubz88M967eht6x5EU2MSLVskFEP40";
            usersLimit = 50;
            minutesInactivity = 15;
            errorCountLimit = 5;
            errorRequestCountLimit = 10;
            Worker.spinInterval = 5000; //5s

            //db settings
            string server = "10.0.0.4";
            string user = "root";
            string password = "mobo0800";
            string database = "followtrain";

            //read params
            for (int i = 0; i < args.Length; i++)
            {
                switch(args[i])
                {
                    case "-twitterConsumerKey":
                        twitterConsumerKey = args[i + 1];
                        break;

                    case "-twitterConsumerSecret":
                        twitterConsumerSecret = args[i + 1];
                        break;
                
                    case "-usersLimit":
                        usersLimit = System.Int32.Parse( args[i + 1] );
                        break;

                    case "-minutesInactivity":
                        minutesInactivity = System.Int32.Parse(args[i + 1]);
                        break;

                    case "-errorCountLimit":
                        errorCountLimit = System.Int32.Parse(args[i + 1]);
                        break;

                    case "-errorRequestCountLimit":
                        errorRequestCountLimit = System.Int32.Parse(args[i + 1]);
                        break;

                    case "-spinInterval":
                        Worker.spinInterval = System.Int32.Parse(args[i + 1]);
                        break;

                    case "-server":
                        server = args[i + 1];
                        break;

                    case "-user":
                        user = args[i + 1];
                        break;

                    case "-password":
                        password = args[i + 1];
                        break;

                    case "-database":
                        database = args[i + 1];
                        break;

                    case "-tweetID":
                        tweetID = args[i + 1];
                        break;
                }

                i++;
            }

            System.Console.WriteLine("##################################");
            System.Console.WriteLine("#### NAMBOKINATOR TWEET BOT ####");
            System.Console.WriteLine("##################################\n\n");
            System.Console.WriteLine("twitterConsumerKey: " + twitterConsumerKey);
            System.Console.WriteLine("twitterConsumerSecret: " + twitterConsumerSecret);
            System.Console.WriteLine("User limit: " + usersLimit);
            System.Console.WriteLine("Error count limit: " + errorCountLimit);
            System.Console.WriteLine("Inactivity limit: " + minutesInactivity);
            System.Console.WriteLine("Spin interval: " + Worker.spinInterval);
            System.Console.WriteLine("_________________________________\n");

            //connect to db
            DBConnect.SetServerPrefences(server, user, password, database);
            
            startPool();
        }

        static void startPool()
        {
            //reset errors
            errorTotalCount = 0;

            while (System.String.IsNullOrWhiteSpace(tweetID))
            {
                System.Console.Write("\n\nEnter the tweet ID: ");
                tweetID = System.Console.In.ReadLine();
            }

            System.Console.WriteLine("\nTweet ID: " + tweetID);

            string twitterOAuthToken = "";
            string twitterAccessToken = "";
            int rowid = 0;

            double minutesInactivityVar = Helpers.ConvertToTimestamp() - ( minutesInactivity * 60 );

            try
            {
                //read records
                DataTable results = DBConnect.Select(System.String.Format("SELECT id, consumer_key, consumer_secret_key FROM users WHERE last_update < {0} AND error_count < {1} LIMIT {2}", minutesInactivityVar, errorCountLimit, usersLimit), null);

                tweetIdVerified = false;

                foreach (DataRow row in results.Rows)
                {
                    //set tokens
                    twitterOAuthToken = row["consumer_key"].ToString();   //Access token
                    twitterAccessToken = row["consumer_secret_key"].ToString();   //Access token secret
                    rowid = System.Int32.Parse( row["id"].ToString() ); 

                    if (System.String.IsNullOrWhiteSpace(twitterAccessToken) ||
                        System.String.IsNullOrWhiteSpace(twitterOAuthToken)  )
                    {
                        System.Console.WriteLine("\nInvalid OUATH data");
                        continue;
                    }

                    if ( !tweetIdVerified )
                    {
                        if( Worker.runThread(rowid, twitterConsumerKey, twitterConsumerSecret, twitterOAuthToken, twitterAccessToken, tweetID, true) ){
                            tweetIdVerified = true;
                        }
                        Thread.Sleep(Worker.spinInterval);
                    }
                    else{
                        Worker.addItem(rowid, twitterConsumerKey, twitterConsumerSecret, twitterOAuthToken, twitterAccessToken, tweetID, false);
                    }
                }

                if (tweetIdVerified)
                {
                    Worker.start();

                    // Wait for the sort to complete.
                    Worker.waitComplete();
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("\n\nApplication Error: {0}", e.Message);
            }

            System.Console.WriteLine("All done. Total errors {0}", errorTotalCount);

            Program.menu();
        }

        static void menu()
        {
            System.Console.WriteLine("\n\n====== MENU =====");
            System.Console.WriteLine("=====================");
            System.Console.WriteLine("1. RUN AGAIN");
            System.Console.WriteLine("2. QUIT");

            string option = null;
            bool invalidOption = true;

            while ( invalidOption )
            {
                System.Console.Write("\nSelect your option: ");
                option = System.Console.In.ReadLine();

                switch (option)
                {
                    case "1":
                        invalidOption = false;
                        startPool();
                        break;
                    
                    case "2":
                        invalidOption = false;
                        Program.exit();
                        break;
                }
            }
            
        }

        static public void exit()
        {
            System.Environment.Exit(0);
        }
    }
}

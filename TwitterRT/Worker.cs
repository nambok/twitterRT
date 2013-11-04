using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Amib.Threading;

namespace TwitterRT
{
    static public class Worker
    {
        static string instanceName = "Nambokinator SmartThreadPool";

        static bool _running = false;
        static bool _paused = false;
        static SmartThreadPool smartThreadPool;

        static public Func<long> _getActiveThreads;
        static public Func<long> _getInUseThreads;
        static public Func<long> _getQueuedWorkItems;
        static public Func<long> _getCompletedWorkItems;

        static int IdleTimeoutConfig = 30000; //30 seconds
        static public int spinInterval = 300; //500ms

        static Worker()
        {
            STPStartInfo stpStartInfo = new STPStartInfo();

            stpStartInfo.IdleTimeout = Worker.IdleTimeoutConfig * 1000;
            stpStartInfo.PerformanceCounterInstanceName = Worker.instanceName;
            stpStartInfo.MaxWorkerThreads = 300;

            Worker.smartThreadPool = new SmartThreadPool(stpStartInfo);

            _getActiveThreads = () => (long)smartThreadPool.ActiveThreads;
            _getInUseThreads = () => (long)smartThreadPool.InUseThreads;
            _getQueuedWorkItems = () => (long)smartThreadPool.CurrentWorkItemsCount;
            _getCompletedWorkItems = () => (long)smartThreadPool.WorkItemsProcessed;
        }

        static public bool addItem(int rowid, string twitterConsumerKey, string twitterConsumerSecret, string twitterOAuthToken, string twitterAccessToken, string tweetID, bool verifyTweetID)
        {
            bool success = true;

            try
            {
                IWorkItemResult<bool> wir = smartThreadPool.QueueWorkItem(new Func<int, string, string, string, string, string, bool, bool>(Worker.runThread), rowid, twitterConsumerKey, twitterConsumerSecret, twitterOAuthToken, twitterAccessToken, tweetID, verifyTweetID);

            }
            catch (System.ObjectDisposedException e)
            {
                e.GetHashCode();
                System.Console.WriteLine("ERROR adding item {0}", e.Message);
                success = false;
            }

            Thread.Sleep(spinInterval);

            return success;
        }

        static public void start()
        {
            if (!_running)
            {
                Worker.smartThreadPool.Start();
            }
        }

        static public void stop()
        {
            Worker.smartThreadPool.Shutdown(false, 2000);
        }

        static public void pause()
        {
            _paused = !_paused;
        }

        static public void waitComplete()
        {
            Worker.smartThreadPool.WaitForIdle();
            Worker.stop();
        }

        static public bool runThread(int rowid, string twitterConsumerKey, string twitterConsumerSecret, string twitterOAuthToken, string twitterAccessToken, string tweetID, bool verifyTweetID)
        {
            if (Program.errorTotalCount > 0 && Program.errorTotalCount % Program.errorRequestCountLimit == 0)
            {
                System.Console.WriteLine("Error limit reached, waiting {0}", TwitterAPI.timeoutMSRateLimit);
                Thread.Sleep( TwitterAPI.timeoutMSRateLimit );
            }

            bool success = false;
            TwitterAPI twitterAPI;

            try
            {
                twitterAPI = new TwitterAPI(twitterConsumerKey, twitterConsumerSecret, twitterOAuthToken, twitterAccessToken);

                if (verifyTweetID && !twitterAPI.GetTweetFromId(tweetID))
                {
                    return false;
                }
            }
            catch(System.Exception ex)
            {
                Program.errorTotalCount++;

                try
                {
                    DBConnect.ExecuteQuery(System.String.Format("UPDATE users SET error_count = error_count+ 1, last_update = {0} WHERE id = {1}", Helpers.ConvertToTimestamp(), rowid), null);
                }
                catch (System.Exception e)
                {

                }
                
                return false;
            }

            try{
                success = twitterAPI.Favorite(tweetID);

                System.Console.WriteLine("\n___________________________");
                System.Console.WriteLine("In use threads: {0}", Worker._getInUseThreads());
                System.Console.WriteLine("Queued threads: {0}", Worker._getQueuedWorkItems());
                System.Console.WriteLine("Completed threads: {0}", Worker._getCompletedWorkItems());
            }
            catch (System.Exception ex)
            {
                Program.errorTotalCount++;

                System.Console.WriteLine("\nTwitter API error: {0}", ex.Message);
                success = false;
            }

            try
            {
                DBConnect.ExecuteQuery(System.String.Format("UPDATE users SET last_update = {0} WHERE id = {1}", Helpers.ConvertToTimestamp(), rowid), null);
            }
            catch (System.Exception ex)
            {
            }

            return success;
        }
    }
}

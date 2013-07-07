using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Chilkat;

namespace TwitterRT
{
    public class TwitterHTTP
    {
        /* twitter urls */
        private string twitterMainUrl = "https://twitter.com/";
        private string twitterLoginPath = "/sessions";
        private string twitterPostUrl = "/i/tweet/create";
        private string twitterHost = "twitter.com";
        private int port = 443;
        private bool use_ssl = true;

        /* twitter token */
        private string authenticity_token = "";

        /* vars */
        private bool is_loggedin = false;
        private string username = "";
        private string password = "";

        /* chilkat request */
        private Chilkat.Http http;
        private Chilkat.HttpRequest req;
        private Chilkat.HttpResponse resp;

        public TwitterHTTP(string proxyDomain, int proxyPort, string proxyUsername, string proxyPassword)
        {
            this.http = new Chilkat.Http();
            this.req = new Chilkat.HttpRequest();

            if ( ! http.UnlockComponent("Anything for 30-day trial") )
            {
                Console.WriteLine(http.LastErrorText);
                return;
            }

            if (!Directory.Exists("sessions"))
            {
                DirectoryInfo di = Directory.CreateDirectory("sessions");
            }

            if(proxyDomain != "false") this.http.ProxyDomain = proxyDomain;
            if (proxyPort != 0) this.http.ProxyPort = proxyPort;
            if (proxyUsername != "false") this.http.ProxyLogin = proxyUsername;
            if (proxyPassword != "false") this.http.ProxyPassword = proxyPassword;
        }

        public bool doLogin(string username, string password)
        {
            this.username = username;
            this.password = password;

            if (!Directory.Exists(@"sessions\" + username))
            {
                DirectoryInfo di = Directory.CreateDirectory(@"sessions\" + username);
            }

            File.Delete(@"sessions\" + username + @"\httpSessionLog.txt");
            this.http.SessionLogFilename = @"sessions\" + username + @"\httpSessionLog.txt";
            this.http.CookieDir = @"sessions\" + username;
            this.http.SetCookieXml(this.twitterHost, this.http.GetCookieXml(this.twitterHost) );
            this.http.SendCookies = true;
            this.http.SaveCookies = true;

            if (!this.getAuthenticityToken()) return false;
            
            if (this.is_loggedin) return true;

            Console.WriteLine("Logging in.....");

            //  Build an HTTP POST request to login
            this.req.RemoveAllParams();
            this.req.UsePost();
            this.req.Path = this.twitterLoginPath;

            //add params
            this.req.AddParam("session[username_or_email]", username);
            this.req.AddParam("session[password]", password);
            this.req.AddParam("authenticity_token", this.authenticity_token);
            this.req.AddParam("scribe_log", "");
            this.req.AddParam("redirect_after_login", "/" + username);

            this.req.AddHeader("Referer", this.twitterMainUrl);

            //  Send the HTTP POST and get the response.  Note: This is a blocking call.
            //  The method does not return until the full HTTP response is received.
            this.resp = this.http.SynchronousRequest(this.twitterHost, this.port, this.use_ssl, this.req);
            if (this.resp == null)
            {
                Console.WriteLine("ERROR: " + this.http.LastErrorText + "\n");
                return false;
            }

            //  Is this a 302 redirect?
            if (this.resp.StatusCode == 302 || this.resp.StatusCode == 200)
            {
                //  Get the redirect URL:
                string redirectUrl = resp.GetHeaderField("location");

                string html = String.Empty;

                //Console.WriteLine(resp.Header);
                //Console.WriteLine("Location: " + redirectUrl);

                if (redirectUrl != null && redirectUrl != String.Empty)
                {
                    if ( redirectUrl.Contains("https://twitter.com/login/error") ||
                         redirectUrl.Contains("https://twitter.com/login/captcha")
                        )
                    {
                        return false;
                    }
                    html = http.QuickGetStr(redirectUrl);
                }
                else
                {
                    html = this.resp.BodyStr;
                }

                //this.getAuthenticityToken(html);
            }
            else
            {
                //Console.WriteLine("ERROR: " + this.resp.BodyStr + "\n");
                Console.WriteLine("ERROR: " + this.resp.StatusCode);
                return false;
            }

            this.is_loggedin = true;
            return true;
        }

        public bool postTweet(string status, string reply_id)
        {
            if ( ! this.is_loggedin ) return false;

            Console.WriteLine("Posting message.....");

            //  Build an HTTP POST request to login
            this.req.RemoveAllParams();
            this.req.UsePost();
            this.req.Path = this.twitterPostUrl;

            //add params
            this.req.AddParam("authenticity_token", this.authenticity_token);
            this.req.AddParam("place_id", "");
            this.req.AddParam("status", status);
            if(reply_id != "" && reply_id != String.Empty) this.req.AddParam("in_reply_to_status_id", reply_id);
            this.req.AddHeader("Referer", this.twitterMainUrl);

            //  Send the HTTP POST and get the response.  Note: This is a blocking call.
            //  The method does not return until the full HTTP response is received.
            this.resp = this.http.SynchronousRequest(this.twitterHost, this.port, this.use_ssl, this.req);
            if (this.resp == null)
            {
                Console.WriteLine("ERROR: " + this.http.LastErrorText + "\n");
                return false;
            }

            //  Is this a 200
            if (this.resp.StatusCode != 200)
            {
                Console.WriteLine("ERROR: " + this.resp.StatusCode + "\n");
                this.is_loggedin = false;
                return false;
            }

            return true;
        }

        private bool getAuthenticityToken(string html = "")
        {
            if (html == String.Empty)
            {
                html = this.http.QuickGetStr(this.twitterMainUrl);
            }

            if (html == null || html == String.Empty) return false;

            //we need to get the following value <input type="hidden" name="authenticity_token" value="">
            Match match = Regex.Match(html, "<input type=\"hidden\" name=\"authenticity_token\" value=\"(.+?)\">", RegexOptions.IgnoreCase);

            try
            {
                this.authenticity_token = match.Groups[1].Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message + "\n");
                return false;
            }

            if (html.Contains(this.username)) this.is_loggedin = true;

            Console.WriteLine("TOKEN: " + this.authenticity_token + "\n");

            if (this.authenticity_token == String.Empty) return false;

            return true;
        }
    }
}

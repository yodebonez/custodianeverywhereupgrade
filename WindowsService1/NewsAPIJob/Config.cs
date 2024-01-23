using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService1.NewsAPIJob
{
    public static class Config
    {
        public const string DEFAULT_BASE_URL = "https://api-football-v1.p.rapidapi.com/v2";
        public static string BASE_URL
        {
            get
            {
                string base_url = ConfigurationManager.AppSettings["BASE_URL"];
                if (!string.IsNullOrEmpty(base_url?.Trim()))
                {
                    if (!base_url.Trim().EndsWith("/"))
                    {
                        return base_url;
                    }
                    else
                    {
                        return base_url.Remove(base_url.Length - 1, 1);
                    }
                }
                else
                {
                    return DEFAULT_BASE_URL;
                }
            }
        }

        public static string Authorization_Header
        {
            get
            {
                string header = ConfigurationManager.AppSettings["AUTH_HEADER"];
                if (!string.IsNullOrEmpty(header))
                {
                    return header;
                }
                else
                {
                    //default value
                    return "73a1a7d816mshb9d8052704e2be1p12cedcjsn425c7ba3c59b";
                }

            }
        }


        public static int GetID
        {
            get
            {
                string Id = ConfigurationManager.AppSettings["LeagueID"];
                if (!string.IsNullOrEmpty(Id))
                {
                    return Convert.ToInt32(Id);
                }
                else
                {
                    return 3;
                }
            }
        }
    }
}

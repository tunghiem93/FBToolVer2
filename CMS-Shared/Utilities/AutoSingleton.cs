using CMS_Shared.Keyword;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace CMS_Shared.Utilities
{
    public sealed class AutoSingleton
    {
        private static AutoSingleton instance = null;
        private static readonly object padlock = new object();
        
        AutoSingleton()
        {
        }

        public static AutoSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new AutoSingleton();

                            /* wait for next date */
                            WaitForNextDate();

                            /* auto run with timer */
                            AutoFunction();
                        }
                    }
                }
                return instance;
            }
        }
        public static void WaitForNextDate()
        {
            try
            {
                var dateTimeNow = DateTime.UtcNow;

                /* time in VN: GMT+7*/
                dateTimeNow.AddHours(7);

                /* time span from begin date */
                var timeSpan = dateTimeNow - dateTimeNow.Date;

                /* time wait for next date */
                var timeWaitForNextDate = (24 * 60 * 60 - timeSpan.TotalSeconds) ;// insecond 

                /* time start at next date */
                var nextHour = Commons.TimerStartAt;
                timeWaitForNextDate += (nextHour * 60 * 60);
                
                /* wait for next day */
                Thread.Sleep((int)timeWaitForNextDate* 1000); 
            }
            catch(Exception ex) { };
        }
        public static void AutoFunction()
        {
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = Commons.TimerInterval*1000; /* (second*1000) = milisecond */
            aTimer.Enabled = true;
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                /* crawl data */
                AutoCrawlAll();
            }
            catch(Exception ex) { };

            /* log data */
            LogHelper.WriteLogs("Timer", "");
        }

        private static void AutoCrawlAll()
        {
            try
            {
                var keyFac = new CMSKeywordFactory();
                var msg = "";
                keyFac.CrawlAllKeyWords("AutoCrawl", ref msg);
            }
            catch (Exception ex) { };
        }
    }
}

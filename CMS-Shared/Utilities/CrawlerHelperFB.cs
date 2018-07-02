using CMS_DTO.CMSCrawler;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace CMS_Shared.Utilities
{
    public static class CrawlerHelperFB
    {
        public static bool Get_Tagged_Pins(ref CMS_CrawlerModels model, string search_str, int limit = 1, string bookmarks_str = null, int page = 1)
        {
            var url = "https://www.facebook.com/lifewithsunshine/ads/?ref=page_internal&dpr=1&ajaxpipe=1&ajaxpipe_token=AXh9WxHKM0f06Rq9&country=1&path=%2Flifewithsunshine%2Fads%2F&__user=100003324695675&__a=1&__dyn=5V4cjLx2ByK5A9UkKHqAyqomzFE9XG8GAdyeGDirWqF1G7UnGdwIhEnUF7yWCHxCEjCyEnyo88ObGubyRUC48G5WAxamjDK7GgPwzxuFS58-ER2KdyU8p94jUXVoS48nVV8Gicx2q5o4OmayrBy8GudAx6cw_xle9xmjx2Qm3GE-qp3FK4bUCaxKh1e5pVkdxCi78SaCCy89ooKHVohxyhu9K9BmFpEBq8IHGfio8l8imEggmKbKqify4cXJ2oS3m6ogUK8GE_WUWiUd9azEKiEDyp8ymaVeaDU8fiAx2miQhxdyopBAyEN4yprypVUV1bCxe9yEgy8LzU9FWDz8a8Z112HJ7VVHAyEsyUlzF8WEKU&__req=jsonp_3&__be=1&__pc=PHASED%3ADEFAULT&__rev=4066324&__spin_r=4066324&__spin_b=trunk&__spin_t=1530514908&__adt=3";
            getDataPinterest(url, model, "", ref bookmarks_str);

            return false;
        }

        public static CMS_CrawlerModels getDataPinterest(string url, CMS_CrawlerModels model, string pinId, ref string bookmarks)
        {
            try
            {
                Uri uri = new Uri(url);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);

                /* request need cookie & user agent */
                httpWebRequest.Headers["Cookie"] = "fr=0HZLfh0cIOmtmNqCq.AWXR_MW9yNog0CyLSTuvJhhdnGM.BajqQf.RE.AAA.0.0.BbOi7n.AWUh9bO8; sb=Cs05WwnlYymkzEg6Xn32mzc8; wd=1366x654; datr=Js05W_jbAaa1Ij5CurtBJmwC; locale=en_GB; c_user=100003324695675; xs=23%3AVia9gvMSQtiufw%3A2%3A1530514908%3A467%3A6165; pl=n; spin=r.4066324_b.trunk_t.1530514908_s.1_v.2_; act=1530541156947%2F6; presence=EDvF3EtimeF1530541148EuserFA21B03324695675A2EstateFDutF1530541148851CEchFDp_5f1B03324695675F4CC; x-src=%2Fpg%2Flifewithsunshine%2Fads%2F%7Ccontent_container; pnl_data2=eyJhIjoib25hZnRlcmxvYWQiLCJjIjoiWFBhZ2VzUHJvZmlsZUhvbWVDb250cm9sbGVyIiwiYiI6ZmFsc2UsImQiOiIvbGlmZXdpdGhzdW5zaGluZS9hZHMvIiwiZSI6W119";
                httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0";

                httpWebRequest.Timeout = 100000;
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var answer = streamReader.ReadToEnd();
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(answer);

                    /* get list scripts */
                    var scripts = htmlDoc.DocumentNode.Descendants("script").ToList();

                    var listData = new List<string>();
                    int i = 0;
                    foreach (var script in scripts)
                    {
                        i++;
                        if (i == 3) /* ERROR IN 3TH SCRIPT */
                            break;

                        /* find pay load element */
                        var res = findElement(script.InnerHtml, "payload", 0);
                        if (!string.IsNullOrEmpty(res))
                        {
                            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                            dynamic dobj = jsonSerializer.Deserialize<dynamic>(res);
                            var htmlData = dobj["content"];
                            if (htmlData != null)
                            {
                                var xmlData = htmlData["content"];
                                if (xmlData != null)
                                {
                                    /* get list tag a */
                                    htmlDoc.LoadHtml(xmlData);
                                    var lstA = htmlDoc.DocumentNode.Descendants("a").Where(n => n.GetAttributeValue("rel", "") == "theater").ToList();
                                    foreach (var tagA in lstA)
                                    {
                                        /* GET DATA MODEL */
                                        var href = tagA.GetAttributeValue("href", "");
                                        model.Pins.Add(new PinsModels()
                                        {
                                            Link = href,
                                        });
                                    }
                                }
                            }
                        }
                    }
                    streamReader.Close();
                    streamReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("ErrorgetDataPinterest" + "\n url: " + url + "\nBookmarks:" + bookmarks, ex);
            }
            return model;
        }

        public static string findElement(string _input, string key, int start)
        {
            var ret = "";
            start = _input.IndexOf(key);
            if (start > 0)
            {
                var countLeftBreak = 0;
                var iEnd = 0;
                start = _input.IndexOf('{', start);
                for (int i = start; i < _input.Length; i++)
                {
                    char ch = _input[i];
                    if (ch == '}')
                    {
                        countLeftBreak--;
                        if (countLeftBreak == 0)
                        {
                            iEnd = i;
                            break;
                        }
                    }
                    else if (ch == '{')
                        countLeftBreak++;
                }

                if (iEnd > start)
                    ret = _input.Substring(start, iEnd - start + 1);
            }

            return ret;
        }

    }
}

using CMS_DTO.CMSCrawler;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CMS_Shared.Utilities
{
    public class CrawlerFbHelpers
    {
        public static string Cookies { get; set; }
        public static void CrawlerFb(string url, ref CMS_CrawlerModels pins)
        {
            try
            {
                url = url + "ads/?country=1&ref=page_internal";
                Uri uri = new Uri(url);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);

                /* request need cookie & user agent */
                //httpWebRequest.Headers["Cookie"] = "fr=0g932KaBNIHkPNSHd.AWUyWBwpX4_A_YKA4NhvmupYBkk.BbMcZu.uD.Fs5.0.0.BbOtXK.AWXncoeT; sb=NmI3W-ffluEtyFHleEWSjhBl; wd=1920x943; datr=NmI3WwtbosYtTwDtslqJtXZd; c_user=100003727776485; xs=136%3Au6XG_yUasjTeFQ%3A2%3A1530356294%3A6091%3A726; pl=n; spin=r.4066192_b.trunk_t.1530495666_s.1_v.2_; act=1530582591458%2F0; presence=EDvF3EtimeF1530582595EuserFA21B03727776485A2EstateFDutF1530582595488CEchFDp_5f1B03727776485F2CC";
                //httpWebRequest.Headers["Cookie"] = "fr=0g932KaBNIHkPNSHd.AWXxf95lRMddtf2MRVvraujAkc0.BbMcZu.uD.Fs5.0.0.BbPiEI.AWVEgjKc; sb=NmI3W-ffluEtyFHleEWSjhBl; wd=1366x631; datr=NmI3WwtbosYtTwDtslqJtXZd; c_user=100003727776485; xs=136%3Au6XG_yUasjTeFQ%3A2%3A1530356294%3A6091%3A726; pl=n; spin=r.4072650_b.trunk_t.1530773112_s.1_v.2_; presence=EDvF3EtimeF1530798371EuserFA21B03727776485A2EstateFDutF1530798371621Et3F_5b_5dElm3FA2user_3a150588258893142A2Eutc3F1530798370407CEchFDp_5f1B03727776485F2CC; act=1530798384170%2F5";
                httpWebRequest.Headers["Cookie"] = Cookies;
                httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0";
                httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

                httpWebRequest.Timeout = 100000;
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var html = streamReader.ReadToEnd();
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);
                    List<HtmlNode> nodeHtml = doc.DocumentNode.Descendants().Where
                                                (x => (x.Name == "div" && x.Attributes["class"] != null &&
                                                   x.Attributes["class"].Value.Contains("_5pbx"))).ToList();
                    if (nodeHtml != null && nodeHtml.Count > 0)
                    {
                        
                    }

                    // fb_id
                    var nodeFb_Id = doc.DocumentNode.Descendants().Where
                                    (
                                        x => (x.Name == "div" && x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("_5pcp _5lel _2jyu _232_"))
                                    ).ToList();
                    List<string> fb_ids = new List<string>();
                    if(nodeFb_Id != null && nodeFb_Id.Count > 0)
                    {
                        foreach(var item in nodeFb_Id)
                        {
                            var strfb_id = item.GetAttributeValue("id", "");
                            if(!string.IsNullOrEmpty(strfb_id))
                            {
                                var split = strfb_id.Split(';').ToList();
                                if(split != null && split.Count > 1)
                                {
                                    var fb_id = split[1];
                                    fb_ids.Add(fb_id);
                                }
                                else
                                {
                                    fb_ids.Add("");
                                }
                            }
                        }
                    }
                    LogHelper.WriteLogs("ListID: " , JsonConvert.SerializeObject(fb_ids));

                   // node html image 
                   List <HtmlNode> nodeHtmlImage = doc.DocumentNode.Descendants().Where
                                                      (x => (x.Name == "div" && x.Attributes["class"] != null &&
                                                      x.Attributes["class"].Value.Contains("mtm"))).ToList();

                    if (nodeHtmlImage != null && nodeHtmlImage.Count > 0)
                    {
                        var index = 0;
                        foreach (var item in nodeHtmlImage)
                        {
                            List<string> fb_id = new List<string>();
                            var nodeChildImage = item.Descendants("a").ToList();
                            if (nodeChildImage != null && nodeChildImage.Count > 0)
                            {
                                foreach(var itemImage in nodeChildImage)
                                {
                                    var _image = itemImage.GetAttributeValue("data-ploi", "");
                                    var _apiDetail = itemImage.GetAttributeValue("href", "");
                                    if (!string.IsNullOrEmpty(_image))
                                    {
                                        _image = _image.Replace("amp;", "");
                                    }

                                    var Pin = new PinsModels();
                                    if (!string.IsNullOrEmpty(_image) && !string.IsNullOrEmpty(_apiDetail))
                                    {
                                        var Splits = _apiDetail.Split('/').ToList();
                                        if (Splits != null && Splits.Count >= 5)
                                            fb_id.Add(Splits[4]);

                                        if (fb_ids != null && fb_ids.Count >= index /*&& nodeChildImage.Count == 1*/)
                                        {
                                            if(!string.IsNullOrEmpty(fb_ids[index]))
                                            {
                                                fb_id.Add(fb_ids[index]);
                                            }
                                        }
                                        CrawlerFBDetail(_apiDetail, fb_id, ref Pin);
                                        Pin.ImageURL = _image;
                                        pins.Pins.Add(Pin);
                                    }
                                }
                            }
                            index++;
                        }
                    }
                    LogHelper.WriteLogs("ListPin: ", JsonConvert.SerializeObject(pins));
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogs("ErrorCrawlerFB: "+ url, JsonConvert.SerializeObject(ex));

                NSLog.Logger.Error("Crawler Fb: ", ex);
            }
            LogHelper.WriteLogs("FinishCrawlerFB", url);
        }

        public static void CrawlerFBDetail(string Url, List<string> fb_id, ref PinsModels pin)
        {
            try
            {
                Url = "https://www.facebook.com" + Url+ "";
                Uri uri = new Uri(Url);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);

                //httpWebRequest.Headers["Cookie"] = "fr=0g932KaBNIHkPNSHd.AWXxf95lRMddtf2MRVvraujAkc0.BbMcZu.uD.Fs5.0.0.BbPiEI.AWVEgjKc; sb=NmI3W-ffluEtyFHleEWSjhBl; wd=1366x631; datr=NmI3WwtbosYtTwDtslqJtXZd; c_user=100003727776485; xs=136%3Au6XG_yUasjTeFQ%3A2%3A1530356294%3A6091%3A726; pl=n; spin=r.4072650_b.trunk_t.1530773112_s.1_v.2_; presence=EDvF3EtimeF1530798371EuserFA21B03727776485A2EstateFDutF1530798371621Et3F_5b_5dElm3FA2user_3a150588258893142A2Eutc3F1530798370407CEchFDp_5f1B03727776485F2CC; act=1530798384170%2F5";
                httpWebRequest.Headers["Cookie"] = Cookies;
                httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0";
                httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                
                httpWebRequest.Timeout = 100000;
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var html = streamReader.ReadToEnd();
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    var script = doc.DocumentNode.Descendants()
                             .Where(n => n.Name == "script").ToList();

                    var listScript = script.Where(o=> !string.IsNullOrEmpty(o.InnerText) && o.InnerText.Contains("require") && o.InnerText.Contains("UFIController")).Select(o => o.InnerText).ToList();
                    foreach (var innerScript in listScript)
                    {
                        if (innerScript.Contains("feedbacktarget"))
                        {
                            if (findNode(innerScript, "feedbacktarget", 0, fb_id, ref pin))
                            {
                                LogHelper.WriteLogs("CrawlerFBDetail Success: ", JsonConvert.SerializeObject(fb_id));
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogs("ErrorCrawlerFBDetail: ", JsonConvert.SerializeObject(ex));
                
                NSLog.Logger.Error("CrawlerFB Detail", ex);
            }
        }

        public static bool findNode(string input, string key, int start,List<string> fb_id, ref PinsModels pin)
        {
            var jsonfeedbacktarget = findElement(input, "feedbacktarget", 0);
            if (string.IsNullOrEmpty(jsonfeedbacktarget))
                return false;
            try
            {
                var dobj = JsonConvert.DeserializeObject<JsonObject>(jsonfeedbacktarget);
                if (dobj != null)
                {
                    if (fb_id.Contains(dobj.entidentifier))
                    {
                        pin.commentTotalCount = dobj.commentcount;
                        pin.sharecount = dobj.sharecount;
                        pin.reactioncount = dobj.reactioncount;
                        pin.ID = dobj.entidentifier;

                        return true;
                    }
                    else
                    {
                        jsonfeedbacktarget = "\"feedbacktarget\":" + jsonfeedbacktarget;
                        input = input.Replace(jsonfeedbacktarget, "");
                        return findNode(input, key, start, fb_id, ref pin);
                    }
                }
            }
            catch(Exception ex)
            {

            }
            return false;
        }

        public static bool findNodeV2(string input, string key, int start, string fb_id, ref PinsModels pin)
        {
            LogHelper.WriteLogs("findNodeV2"+ fb_id, "");
            var count = 0;

            try
            {
                while (input.Contains("feedbacktarget"))
                {
                    count++;
                    var jsonfeedbacktarget = findElement(input, "feedbacktarget", 0);
                    LogHelper.WriteLogs("findNodeV2"+ fb_id, jsonfeedbacktarget);
                    
                    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                    dynamic dobj = jsonSerializer.Deserialize<dynamic>(jsonfeedbacktarget);
                    var dictionary = dobj as Dictionary<string, dynamic>;
                    if (dictionary.ContainsKey("entidentifier"))
                    {
                        var _fb_id = dictionary["entidentifier"];
                        if (fb_id.Equals(_fb_id))
                        {
                            if (dictionary.ContainsKey("commentTotalCount"))
                            {
                                pin.commentTotalCount = Convert.ToInt16(dictionary["commentTotalCount"]);
                            }
                            if (dictionary.ContainsKey("reactioncount"))
                            {
                                pin.reactioncount = Convert.ToInt16(dictionary["reactioncount"]);
                            }
                            if (dictionary.ContainsKey("sharecount"))
                            {
                                pin.sharecount = Convert.ToInt16(dictionary["sharecount"]);
                            }

                            pin.ID = fb_id;
                            return true;
                        }
                        else
                        {
                            jsonfeedbacktarget = "\"feedbacktarget\":" + jsonfeedbacktarget;
                            input = input.Replace(jsonfeedbacktarget, "");
                        }
                    }
                };
            }
            catch(Exception ex)
            {
                LogHelper.WriteLogs("ErrorfindNodeV2: " + fb_id, JsonConvert.SerializeObject(ex));
            };
            LogHelper.WriteLogs("EndfindNodeV2"+ fb_id, count.ToString());

            return false;
            
        }
        
           
        public static string findElement(string _input, string key, int start)
        {
            var ret = "";
            try
            {
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
            }
            catch (Exception ex) { };
            

            return ret;
        }

        public static string findFbId(string input)
        {
            var ret = "";
            try
            {
                var iStart = input.IndexOf("fbid");
                var iEnd = input.IndexOf("&amp;", iStart);
                if (iEnd > iStart)
                {
                    iStart += 5;
                    ret = input.Substring(iStart, iEnd - iStart);
                }
            }
            catch (Exception ex) { };
            return ret;
        }
    }

    public class JsonObject
    {
        public Int16 commentcount { get; set; }
        public Int16 reactioncount { get; set; }
        public Int16 sharecount { get; set; }
        public string entidentifier { get; set; }
    }
}

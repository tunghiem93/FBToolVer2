using CMS_DTO.CMSCrawler;
using HtmlAgilityPack;
using Newtonsoft.Json;
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
    public class CrawlerFbHelpers_v2
    {
        public static string Cookies { get; set; }

        public static string findFbPageId(string input)
        {
            var ret = "";
            try
            {
                var iStart = input.IndexOf("?page_id=");
                var iEnd = input.Length;
                iStart = input.IndexOf("=", iStart);

                ret = input.Substring(iStart + 1, (iEnd - iStart) - 1);
            }
            catch (Exception ex) { }
            return ret;
        }

        public static string findFbHash(string input)
        {
            var ret = "";
            try
            {
                var iStart = input.IndexOf("_nc_hash=");
                var iEnd = input.Length;
                iStart = input.IndexOf("=", iStart);

                ret = input.Substring(iStart + 1, (iEnd - iStart) - 1);
            }
            catch (Exception ex) { }
            return ret;
        }

        public static string findFbId_v3(string input, string subString, string last, ref string charecter)
        {
            var ret = "";
            try
            {
                var iStart = input.IndexOf(subString);
                var iEnd = 0;
                iStart = input.IndexOf(last, iStart);
                for (int i = iStart + 1; i < input.Length; i++)
                {
                    char key = input[i];
                    if (key == ':')
                    {
                        charecter = ":";
                        iEnd = i;
                        break;
                    }
                    else if (key == ';')
                    {
                        charecter = ";";
                        iEnd = i;
                        break;
                    }
                }
                ret = input.Substring(iStart + 1, (iEnd - iStart) - 1);
            }
            catch (Exception ex) { }
            return ret;
        }

        public static string findFbOh(string input)
        {
            var ret = "";
            try
            {
                var iEnd = input.IndexOf("_n.");
                var iStart = 0;
                iEnd = input.IndexOf("_", iEnd);
                for (int i = iEnd - 1; i > 0; i--)
                {
                    char key = input[i];
                    if (key == '_')
                    {
                        iStart = i;
                        break;
                    }
                }
                ret = input.Substring(iStart + 1, (iEnd - iStart) - 1);
            }
            catch (Exception ex) { }
            return ret;
        }

        public static void CrawlerDataFacebook(string strHtml, bool IsNextPage, ref CMS_CrawlerModels pins, ref string _pageId)
        {
            try
            {
                if (!string.IsNullOrEmpty(strHtml))
                {
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(strHtml);

                    //find page id of fan page
                    if(!IsNextPage)
                    {
                        var nodePageId = htmlDoc.DocumentNode.Descendants().Where
                                                                       (x => (x.Name == "div" && x.Attributes["class"] != null &&
                                                                        x.Attributes["class"].Value.Contains("_3qn7 _61-0 _2fyi _3qnf _3-95"))).ToList();
                        if (nodePageId != null && nodePageId.Count > 0)
                        {
                            foreach (var item in nodePageId)
                            {
                                var _LstPageId = item.Descendants("a").ToList();
                                if (_LstPageId != null && _LstPageId.Count > 0)
                                {
                                    var Page = _LstPageId[0].GetAttributeValue("ajaxify", "");
                                    _pageId = findFbPageId(Page);
                                }
                            }
                        }
                    }

                    List<HtmlNode> nodeHtml = htmlDoc.DocumentNode.Descendants().Where
                                        (x => (x.Name == "div" && x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("_1dwg _1w_m _q7o"))).ToList();
                    if (nodeHtml != null && nodeHtml.Count > 0)
                    {
                        var description = "";
                        var OwnerName = "";
                        List<string> fb_ids = null;
                        foreach (var itemHtml in nodeHtml)
                        {
                            var _node = itemHtml.Descendants("div").ToList();
                            foreach (var item in _node)
                            {
                                fb_ids = new List<string>();
                                var _Html = item.InnerHtml;
                                if (!string.IsNullOrEmpty(_Html))
                                {
                                    var _Doc = new HtmlDocument();
                                    _Doc.LoadHtml(_Html);
                                    // Description
                                    var _des = _Doc.DocumentNode.Descendants().Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("_5pbx userContent _3576")).ToList();
                                    if (_des != null && _des.Count > 0)
                                    {
                                        description = _des[0].InnerText;
                                        if (!string.IsNullOrEmpty(description))
                                            description = description.Replace("&quot;", "");
                                    }
                                    // Owner name
                                    var _ownerName = _Doc.DocumentNode.Descendants().Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("_6a _5u5j _6b")).ToList();
                                    if (_ownerName != null && _ownerName.Count > 0)
                                    {
                                        foreach (var itemOwner in _ownerName)
                                        {
                                            var NodeName = itemOwner.Descendants("a").ToList();
                                            if (NodeName != null)
                                            {
                                                OwnerName = NodeName[0].InnerText;
                                                if (!string.IsNullOrEmpty(OwnerName))
                                                    OwnerName = OwnerName.Replace("&quot;", "");
                                                else
                                                {
                                                    pins.ErrorStatus = (byte)Commons.EErrorStatus.AccBlocked;
                                                }
                                            }
                                        }
                                    }

                                    // fb_id
                                    var _FbId = _Doc.DocumentNode.Descendants().Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("_5pcp _5lel _2jyu _232_")).ToList();
                                    if (_FbId != null && _FbId.Count > 0)
                                    {
                                        foreach (var itemFbId in _FbId)
                                        {
                                            var strfb_id = itemFbId.Id;
                                            if (!string.IsNullOrEmpty(strfb_id))
                                            {
                                                var charecter = "";
                                                var fb_id = findFbId_v3(strfb_id, "subtitle_", "_", ref charecter);
                                                if (!string.IsNullOrEmpty(fb_id))
                                                    fb_ids.Add(fb_id);
                                                if (!string.IsNullOrEmpty(charecter) && charecter.Equals(";"))
                                                {
                                                    fb_id = findFbId_v3(strfb_id, ";", ";", ref charecter);
                                                    if (!string.IsNullOrEmpty(fb_id))
                                                        fb_ids.Add(fb_id);
                                                }
                                            }
                                        }
                                    }

                                    //  Image 
                                    var _Image = _Doc.DocumentNode.Descendants().Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("mtm")).ToList();

                                    if (_Image != null && _Image.Count > 0)
                                    {
                                        foreach (var itemImg in _Image)
                                        {
                                            // post normal
                                            var nodeChildImage = item.Descendants("a").ToList();
                                            if (nodeChildImage != null && nodeChildImage.Count > 0)
                                            {
                                                foreach (var itemImage in nodeChildImage)
                                                {
                                                    var fb_id = new List<string>();
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
                                                        if (fb_ids != null && fb_ids.Count > 0)
                                                            fb_id.AddRange(fb_ids);

                                                        CrawlerFBDetail(_apiDetail, fb_id, ref Pin);
                                                        Pin.ImageURL = _image;
                                                        Pin.OwnerName = OwnerName;
                                                        Pin.Description = description;
                                                        if (!string.IsNullOrEmpty(Pin.ID))
                                                            pins.Pins.Add(Pin);
                                                    }
                                                }
                                            }
                                            //post dynamic
                                            var nodeChildDynamic = itemImg.Descendants("ul").ToList();
                                            if (nodeChildDynamic != null && nodeChildDynamic.Count > 0)
                                            {

                                                var _doc = new HtmlDocument();
                                                _doc.LoadHtml(nodeChildDynamic[0].InnerHtml);
                                                var nodeLI = _doc.DocumentNode.Descendants().Where(
                                                                     x => (x.Name == "li" && x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("_5ya"))).ToList();
                                                if (nodeLI != null && nodeLI.Count > 0)
                                                {
                                                    foreach (var itemLI in nodeLI)
                                                    {
                                                        var Pin = new PinsModels();
                                                        var nodeLIImage = itemLI.Descendants("img").ToList();
                                                        if (nodeLIImage != null && nodeLIImage.Count > 0)
                                                        {
                                                            var _image = nodeLIImage[0].GetAttributeValue("src", "");
                                                            if (!string.IsNullOrEmpty(_image))
                                                            {
                                                                _image = _image.Replace("amp;", "");
                                                                Pin.ImageURL = _image;
                                                                var PinId = findFbOh(_image);
                                                                if (!string.IsNullOrEmpty(PinId))
                                                                    Pin.ID = PinId + "_" + fb_ids[0];
                                                                else
                                                                {
                                                                    PinId = findFbHash(_image);
                                                                    if (!string.IsNullOrEmpty(PinId))
                                                                        Pin.ID = PinId + "_" + fb_ids[0];
                                                                    else
                                                                        Pin.ID = Guid.NewGuid().ToString();
                                                                }
                                                                    
                                                            }
                                                        }
                                                        var nodeLink = itemLI.Descendants("a").ToList();
                                                        if (nodeLink != null && nodeLink.Count > 0)
                                                        {
                                                            var _link = nodeLink[0].GetAttributeValue("href", "");
                                                            Pin.Link = _link;
                                                        }

                                                        //description
                                                        var nodeLIDescription = itemLI.Descendants("div").ToList();
                                                        if (nodeLIDescription != null && nodeLIDescription.Count > 0)
                                                        {
                                                            var _description = nodeLIDescription.Where(x => x.LastChild.Name.Equals("#text")).FirstOrDefault();
                                                            if (_description != null)
                                                                Pin.Description = _description.InnerText;
                                                        }
                                                        Pin.OwnerName = OwnerName;

                                                        if (!string.IsNullOrEmpty(Pin.ID))
                                                        {
                                                            pins.Pins.Add(Pin);
                                                        }
                                                    }
                                                }

                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }

        public static void CrawlerFb(string url, ref CMS_CrawlerModels pins, ref string _pageId)
        {
            int _port = 0;
            string _proxy = CommonHelper.RamdomProxy(ref _port);
            Uri uri = new Uri(url);
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Proxy = new WebProxy(_proxy, _port);
            /* request need cookie & user agent */
            httpWebRequest.Headers["Cookie"] = Cookies;
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0";
            httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            httpWebRequest.Timeout = 9000000;
            using (HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                try
                {
                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var html = streamReader.ReadToEnd();
                            CrawlerDataFacebook(html, false, ref pins, ref _pageId);
                            streamReader.Close();
                            streamReader.Dispose();
                        }
                    }
                }
                catch(Exception ex)
                {
                    if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                        CrawlerFb(url, ref pins, ref _pageId);

                    LogHelper.WriteLogs("ErrorCrawlerFB: " + url, JsonConvert.SerializeObject(ex));
                    NSLog.Logger.Error("Crawler Fb: ", ex);
                }

            }
        }

        public static void CrawlerNextPage(string pageId, string userId, int cursor, string referer, ref CMS_CrawlerModels pins)
        {
            int _port = 0;
            string _proxy = CommonHelper.RamdomProxy(ref _port);
            var url = "https://www.facebook.com/pages/ads/more/?cursor=" + cursor + "&surface=www_page_ads&unit_count=" + cursor + "&country=1&dpr=1&__user=" + userId + "&__a=1&__req=v&__be=1&__pc=PHASED%3ADEFAULT&__rev=4075583&__spin_r=4075583&__spin_b=trunk&__spin_t=1530846023&page_id=" + pageId + "";
            Uri uri = new Uri(url);
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Proxy = new WebProxy(_proxy, _port);
            /* request need cookie & user agent */
            httpWebRequest.Headers["Cookie"] = Cookies;
            httpWebRequest.Referer = referer;
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0";
            httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            httpWebRequest.Timeout = 9000000;
            using (HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                try
                {
                    if(httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var html = streamReader.ReadToEnd();
                            if (!string.IsNullOrEmpty(html))
                            {
                                html = html.Replace("for (;;);", "");
                                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                                dynamic dobj = jsonSerializer.Deserialize<dynamic>(html);
                                var domops = dobj["domops"];
                                if (domops != null)
                                {
                                    var _objhtmt = domops[0][3];
                                    if (_objhtmt != null)
                                    {
                                        var _html = _objhtmt["__html"];
                                        if (!string.IsNullOrEmpty(_html))
                                        {
                                            CrawlerDataFacebook(_html, true, ref pins, ref pageId);
                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }
                                }
                            }
                            streamReader.Close();
                            streamReader.Dispose();
                        }
                    }
                }
                catch(Exception ex)
                {
                    if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                        CrawlerNextPage(pageId, userId, cursor, referer, ref pins);
                }
            }
            // đệ quy craweler next page
            cursor = cursor + 8;
            CrawlerNextPage(pageId, userId, cursor, referer, ref pins);
        }

        public static void CrawlerAllFb(string url, ref CMS_CrawlerModels pins)
        {
            try
            {
                var user_Id = "";
                url = url + "ads/?country=1&ref=page_internal";
                if (!string.IsNullOrEmpty(Cookies))
                {
                    var start = Cookies.IndexOf("c_user=");
                    var end = Cookies.Length;
                    start = Cookies.IndexOf("=", start) + 1;
                    for (int i = start; i < Cookies.Length; i++)
                    {
                        char key = Cookies[i];
                        if (key == ';')
                        {
                            end = i;
                            break;
                        }
                    }
                    user_Id = Cookies.Substring(start, (end - start));
                }
                /* crawl first page */
                string _pageId = "";
                CrawlerFb(url, ref pins, ref _pageId);

                if(string.IsNullOrEmpty(_pageId))
                {
                    if (!string.IsNullOrEmpty(url))
                    {
                        var end = url.IndexOf("/ads/");
                        end = url.IndexOf("/", end);
                        var start = 0;
                        for (int i = end; i > 0; i--)
                        {
                            char key = url[i];
                            if (key == '-')
                            {
                                start = i;
                                break;
                            }
                        }
                        _pageId = url.Substring(start + 1, (end - start - 1));
                        if (!string.IsNullOrEmpty(_pageId) && _pageId.Length > 15)
                            _pageId = "";
                    }
                }

                /* check next page */
                if (!string.IsNullOrEmpty(_pageId) && !string.IsNullOrEmpty(user_Id))
                {
                    CrawlerNextPage(_pageId, user_Id, 8, url, ref pins);
                }
            }
            catch (Exception ex) { }

        }

        public static void CrawlerFBDetail(string Url, List<string> fb_id, ref PinsModels pin)
        {
            int _port = 0;
            string _proxy = CommonHelper.RamdomProxy(ref _port);
            Url = "https://www.facebook.com" + Url + "";
            Uri uri = new Uri(Url);
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Proxy = new WebProxy(_proxy, _port);
            httpWebRequest.Headers["Cookie"] = Cookies;
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0";
            httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            httpWebRequest.Timeout = 9000000;
            httpWebRequest.KeepAlive = true;
            using (HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                try
                {
                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var html = streamReader.ReadToEnd();
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(html);

                            /* FIND FB CREATED DATE (created_at) */
                            var tagA = doc.DocumentNode.Descendants("a").Where(n => n.GetAttributeValue("rel", "") == "theater").FirstOrDefault();
                            findCreateAt(fb_id, ref tagA, ref pin);

                            /* FIND FEEDBACK_TARGET */
                            var script = doc.DocumentNode.Descendants().Where(n => n.Name == "script").ToList();
                            var innerScript = script.Where(o => !string.IsNullOrEmpty(o.InnerText) && o.InnerText.Contains("require(\"TimeSlice\").guard(function() {require(\"ServerJSDefine\")")).Select(o => o.InnerText).FirstOrDefault();
                            findNode(innerScript, "feedbacktarget", 0, fb_id, ref pin);
                            streamReader.Close();
                            streamReader.Dispose();
                        }
                    }
                    else
                    {
                        CrawlerFBDetail(Url, fb_id, ref pin);
                    }
                }
                catch(Exception ex)
                {
                    if(httpResponse.StatusCode == HttpStatusCode.NotFound)
                        CrawlerFBDetail(Url, fb_id, ref pin);

                    LogHelper.WriteLogs("ErrorCrawlerFBDetail: ", JsonConvert.SerializeObject(ex));
                    NSLog.Logger.Error("CrawlerFB Detail", ex);
                }
                // Do your processings here....

            }
        }

        public static bool findCreateAt(List<string> listFbId, ref HtmlNode node, ref PinsModels pin)
        {
            var ret = false;
            try
            {
                var ajaxify = node.GetAttributeValue("ajaxify", "");
                var fbID = findFbId(ajaxify);
                if (listFbId.Contains(fbID)) /* check fb id */
                {
                    var abbr = node.Descendants("abbr").FirstOrDefault();
                    if (abbr != null)
                    {
                        /* pares datetime */
                        //DateTime created_at = Commons.MinDate;
                        //var timeTitle = abbr.GetAttributeValue("title", "");
                        //if (DateTime.TryParse(timeTitle, out created_at))
                        //{
                        //    pin.Created_At = created_at;
                        //    ret = true;
                        //}

                        var timeStamp = abbr.GetAttributeValue("data-utime", "");
                        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                        pin.Created_At = dtDateTime.AddSeconds(double.Parse(timeStamp)).ToLocalTime();
                    }
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

        public static bool findNode(string input, string key, int start, List<string> fb_id, ref PinsModels pin)
        {
            var jsonfeedbacktarget = findElement(input, "feedbacktarget", 0);
            if (string.IsNullOrEmpty(jsonfeedbacktarget))
                return false;
            try
            {
                var dobj = JsonConvert.DeserializeObject<JsonObject_v2>(jsonfeedbacktarget);
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
            catch (Exception ex) { }
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
    }

    public class JsonObject_v2
    {
        public int commentcount { get; set; }
        public int reactioncount { get; set; }
        public int sharecount { get; set; }
        public string entidentifier { get; set; }
    }
}

using System.Collections.Generic;
using System.IO;
using System;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace GroupPublicationsParser
{
    class Program
    {
        static public string IsoToUtf8(string s)
        {
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(s);
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            return iso.GetString(isoBytes);
        }

        static void Main(string[] args)
        {
            var publicationsList = new List<Publication>();
            var htmlDoc = new HtmlDocument();
            htmlDoc.Load("allPublicationsFacebookSourceCode_Stripped.html", Encoding.UTF8);

            File.Delete("allPublications.json");
            var publications = htmlDoc.DocumentNode.SelectNodes("//div[contains(@id,'mall_post_')]");
            foreach (var node in publications)
            {
                Publication currentPublication = new Publication();
                var timeDate = node.SelectNodes(".//abbr[contains(@class,'_5ptz')]")[0].Attributes["title"].Value;
                var content = node.SelectNodes(".//p");

                currentPublication.PublicationDate = timeDate;
                if(content != null)
                {
                    foreach (var p in content)
                    {
                        if (p.InnerText != "")
                        {
                            string pTxt = WebUtility.HtmlDecode(p.InnerText);

                            var restOfTxt = p.SelectSingleNode(".//span[contains(@class,'text_exposed_show')]");
                            if(restOfTxt != null)
                            {
                                pTxt += WebUtility.HtmlDecode(p.InnerText);
                                //Sometimes first paragraph end with ... or begin with ... Remove that!
                                int place = pTxt.LastIndexOf("...");
                                if (place != -1)
                                    pTxt = pTxt.Remove(place, "...".Length);
                                place = pTxt.IndexOf("...");
                                if (place != -1)
                                    pTxt = pTxt.Remove(place, "...".Length);
                            }

                            //For longer publications, they need to be dowloaded from facebook ...
                            //For now only get the link to the full publication
                            var publicationLink = p.SelectSingleNode("..//span[contains(@class,'text_exposed_link')]/a");
                            if (publicationLink != null)
                            {
                                string link = "http://facebook.com" + publicationLink.Attributes["href"].Value;
                                currentPublication.PublicationLink = link;
                            }

                            currentPublication.PublicationContent +=  pTxt + "\r\n";
                        }
                    }
                }

                var pictureContainer = node.SelectNodes(".//img[contains(@src,'https://scontent') and contains(@class,'scaledImageFitWidth img')]");
                if (pictureContainer != null)
                {
                    var pictureLink = WebUtility.HtmlDecode(pictureContainer[0].Attributes["src"].Value);
                    currentPublication.PictureLink = pictureLink;
                }

                publicationsList.Add(currentPublication);
            }

            var json = JsonConvert.SerializeObject(publicationsList);
            File.AppendAllText("allPublications.json", json);
        }
    }
}

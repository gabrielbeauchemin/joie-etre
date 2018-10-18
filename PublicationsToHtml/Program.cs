using System.IO;
using HtmlAgilityPack;
using System.Net;

namespace PublicationsToHtml
{
    class Program
    {
        static void Main(string[] args)
        {
            var doc = new HtmlDocument();

            // create html document
            var html = HtmlNode.CreateNode("<html><head><meta charset=\"UTF-8\"></head><body></body></html>");
            doc.DocumentNode.AppendChild(html);
            var body = doc.DocumentNode.SelectSingleNode("/html/body");

            bool isNewPublication = true;
            bool isAutor = false;
            foreach (var line in File.ReadLines("allPublications.txt"))
            {
                if(isNewPublication)
                {
                    isNewPublication = false;
                    isAutor = true;
                    var datePublication = HtmlNode.CreateNode("<p>" + line + "</p>");
                    body.AppendChild(datePublication);
                }
                else if(line == "")
                {
                    isNewPublication = true;
                    isAutor = false;
                    for(var _ = 0; _ < 5; ++_)
                    {
                        var newLine = HtmlNode.CreateNode("<Br>");
                        body.AppendChild(newLine);
                    }
                    
                }
                else if(isAutor)
                {
                    isNewPublication = false;
                    isAutor = false;
                    var autor = HtmlNode.CreateNode("<p>" + line + "</p>");
                    body.AppendChild(autor);
                }
                else if(line.Contains("https://scontent")) //images
                {
                    isNewPublication = false;
                    isAutor = false;
                    var content = HtmlNode.CreateNode("<img src=\"" + line + "\">");
                    body.AppendChild(content);
                }
                else //Content
                {
                    isNewPublication = false;
                    isAutor = false;
                    var content = HtmlNode.CreateNode("<p>" + WebUtility.HtmlEncode(line) + "</p>");
                    body.AppendChild(content);
                }
                
            }

            File.Delete("allPublications.html");
            File.WriteAllText("allPublications.html", html.OuterHtml);
        }
    }
}

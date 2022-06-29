using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WebsiteCrawler
{
    class Program
    {
        static string siteUrl = string.Empty;
        static int numberOfPages = 0;
        static List<string> scrappedSites = new List<string>();
        static List<string> storedAssets = new List<string>();

        // Change static text
        static void Main(string[] args)
        {
            DownloadWebsiteAsync().GetAwaiter().GetResult();

            Console.Read();
        }

        // Download website from url that is defined at config  file
        static async Task DownloadWebsiteAsync()
        {
            try
            {
                DateTime startTime = DateTime.Now;
                Console.WriteLine($"-----------------------\nStarted at {startTime}\n");

                siteUrl = ConfigurationManager.AppSettings["url"];

                if (string.IsNullOrWhiteSpace(siteUrl))
                {
                    Console.WriteLine("Url in the settings file is empty");
                    return;
                }

                CertificateResolver();

                await ScrapUrlAsync(siteUrl);

                DateTime endTime = DateTime.Now;
                Console.WriteLine($"-----------------------\nFinished at {endTime}\nNumber of pages downloaded: " +
                    $"{numberOfPages} page downloaded\nTotal time {endTime - startTime}");

                Console.Read();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FINISHED-----------------------{DateTime.Now}\nError has been occured with the" +
                    $" following details: {ex}");

                LogToFile(ex.Message, ex.StackTrace);
            }
        }


        // Checks and adds the certificate to the url if necessary
        static void CertificateResolver()
        {
            if (!siteUrl.StartsWith("http"))
            {
                siteUrl = $"https://{siteUrl}";
            }
        }

        // Scraps website with given url; stores assets, webpage and internal links
        static async Task ScrapUrlAsync(string url)
        {
            try
            {
                if (scrappedSites.Contains(url))
                {
                    return;
                }

                Console.WriteLine($"Downloading page: {url} \n");

                scrappedSites.Add(url);

                string websiteCodes = FetchWebsiteCodes(url);

                string downloadPath = CreateFolder(url);

                SaveFileToDisk(websiteCodes, downloadPath, url);
                
                await CrawlLinksAsync(websiteCodes, url);
                
                numberOfPages++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while fething the page: {url}.\n");

                LogToFile(ex.Message, ex.StackTrace);
            }
        }

        // Reads source code of the website and returns the content as string
        static string FetchWebsiteCodes(string url)
        {
            var request = GetWebRequest(url);

            using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }

        // Stores given content to the disk
        static void SaveFileToDisk(string content, string path, string url)
        {
            string[] splittedUrl = url.Split("/");
            
            string fileName = $"/{splittedUrl[splittedUrl.Length - 1]}";

            StringBuilder pathBuilder = new StringBuilder();
            pathBuilder.Append(path);
            pathBuilder.Append(fileName);
            pathBuilder.Append(".html");

            string htmlFile = pathBuilder.ToString();

            if (File.Exists(htmlFile) || string.Equals(fileName, "/"))
            {
                return;
            }

            using (StreamWriter streamWriter = File.CreateText(htmlFile))
            {
                streamWriter.WriteLine(content);
                
                Console.WriteLine($"Following page saved: {htmlFile} \n");
            }
        }

        // Finds all the links and assets of website
        static async Task CrawlLinksAsync(string websiteCodes, string url)
        {
            Console.WriteLine($"Crawling for the assets of following page: {url} \n");

            var srcRegex = new Regex("\\s+(?:[^>]*?\\s+)?src=(?:\"(?<src>.*?)\")");
           
            var srcMatches = srcRegex.Matches(websiteCodes);
            
            var linkList = srcMatches.OfType<Match>().Select(m => m.Groups["src"].Value).ToList();

            var hrefRegex = new Regex("\\s+(?:[^>]*?\\s+)?href=(?:\"(?<href>.*?)\")");
            
            var hrefMatches = hrefRegex.Matches(websiteCodes);
            
            var hrefList = hrefMatches.OfType<Match>().Select(m => m.Groups["href"].Value).ToList();

            linkList.AddRange(hrefList);

            List<Task> linkListTask = new List<Task>();

            foreach (var link in linkList)
            {
                var linkTask = Task.Run(() =>
                {
                    ParseLinkValue(link);
                });

                linkListTask.Add(linkTask);
            }

            Task.WaitAll(linkListTask.ToArray());
        }

        // Processes given link value, filters or stores locally
        static async Task ParseLinkValue(string value)
        {
            if (value.StartsWith("tel:+"))
            {
                return;
            }

            // to filter external links
            if (value.Contains("//"))
            {
                return;
            }

            if (value.Contains("javascript"))
            {
                return;
            }

            if (value.Contains("?"))
            {
                value = value.Split("?")[0];
            }

            value = value.Replace(" ", string.Empty);

            if (value.Contains("#"))
            {
                value = value.Split("#")[0];
            }

            // asset that needs to be stored
            if (value.Contains("."))
            {
                if (!storedAssets.Contains(value))
                {
                    StoreAssets(value);
                }

                storedAssets.Add(value);

                return;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!value.StartsWith("/"))
            {
                value = $"/{value}";
            }

            await ScrapUrlAsync($"{siteUrl}{value}");
        }

        // Stores internal assets to local disk
        static void StoreAssets(string value)
        {
            string assetFolderPath = CreateFolder(value);

            string[] pathArray = value.Split("/");

            string fileName = pathArray[pathArray.Length - 1];

            string file = $"{assetFolderPath}/{fileName}";

            if (File.Exists(file))
            {
                return;
            }

            var request = GetWebRequest($"{siteUrl}/{value}");

            using (var response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (FileStream fileStream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write))
            {
                byte[] buffer = new byte[102400];

                int count;

                while ((count = stream.Read(buffer, 0, 10400)) > 0)
                {
                    fileStream.Write(buffer, 0, count);
                }
            }
        }

        // Creates and returns a web request from given url in order to handle redirects
        static HttpWebRequest GetWebRequest(string url)
        {
            var uri = new Uri(url);
            
            var request = (HttpWebRequest)WebRequest.Create(uri);
            
            request.Method = "GET";
            
            request.AllowAutoRedirect = false;

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            string location;
            
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                location = response.GetResponseHeader("Location");
            }
            
            if (location != uri.OriginalString)
            {
                uri = new Uri(location);
            
                request = (HttpWebRequest)WebRequest.Create(uri);
            }

            return request;
        }

        // Used in order to create a local folder if does not exist
        static string CreateFolder(string url)
        {
            string extension = url.Replace(siteUrl, string.Empty);

            string domain = FetchDomainFromUrl(siteUrl);

            StringBuilder pathBuilder = new StringBuilder();
            pathBuilder.Append(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            pathBuilder.Append("/DownloadedWebsites/");
            pathBuilder.Append(domain);

            if (extension.StartsWith("/"))
            {
                extension = extension.Substring(1);
            }

            if (extension.EndsWith("/"))
            {
                extension = extension.Substring(0, extension.Length - 1);
            }

            int lastIndexOf = extension.LastIndexOf("/");
            
            if (lastIndexOf != -1)
            {
                string prefixPath = extension.Substring(0, lastIndexOf);
            
                pathBuilder.Append($"/{prefixPath}");
            }
            string path = pathBuilder.ToString();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        // Returns domain value of given url
        static string FetchDomainFromUrl(string url)
        {
            Uri uri = new Uri(url);

            string domain = uri.Host;

            domain = domain.Replace(".com", string.Empty).Replace("www.", string.Empty);

            return domain;
        }

        // Logs the errors that are encountered to the local file
        static void LogToFile(string exception, string stackTrace)
        {
            string domain = FetchDomainFromUrl(siteUrl);

            StringBuilder pathBuilder = new StringBuilder();
            pathBuilder.Append(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            pathBuilder.Append("/DownloadedWebsites/");
            pathBuilder.Append(domain);

            string time = DateTime.Now.ToString("dd-MM-yyyy");

            string fileName = $"/log/{time}.log";

            pathBuilder.Append(fileName);

            string logFile = pathBuilder.ToString();

            CreateFolder(fileName);

            if (!File.Exists(logFile))
            {
                File.Create(logFile);
            }

            Thread.Sleep(1000);

            using (StreamWriter streamWriter = File.AppendText(logFile))
            {
                streamWriter.WriteLine($"\nError at: {DateTime.Now} \nWith exception: {exception}\n" +
                    $"StackTrace: {stackTrace}\n");
            }
        }
    }
}

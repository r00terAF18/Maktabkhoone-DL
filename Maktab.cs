using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace maktab_dl;

public class Maktab
{
    private CookieContainer cc = new();
    private List<string> course_urls = new();
    private List<string> not_found_links = new();
    private HtmlWeb web = new();
    private HtmlDocument doc = new();

    public Dictionary<string, string> courls_vid_name = new();
    const string base_link = "https://maktabkhooneh.org";
    public string course_link;
    public int Lectures = 0;
    public string course_name;
    public string base_folder;


    public Maktab(string course_link)
    {
        this.course_link = course_link;

        doc = web.Load(course_link);

        CreateDirStructure();
    }

    // I swear one day I will recite every letter of this function
    private string ToUtf8(string text)
    {
        byte[] bytes = Encoding.Default.GetBytes(text);
        text = Encoding.UTF8.GetString(bytes);
        return text;
    }

    /// <summary>
    /// Checks if the required directory exists based on the course name
    /// if not, create it
    /// </summary>
    private void CreateDirStructure()
    {
        course_name = doc.DocumentNode.SelectSingleNode("//h1[@class=\"course-intro__title\"]")
            .InnerText;

        course_name = ToUtf8(course_name);

        if (!Directory.Exists(course_name))
        {
            Directory.CreateDirectory(course_name);
        }

        base_folder = Path.GetFullPath(course_name);
    }

    /// <summary>
    /// Properly reads and parses a pre-defined cookies.txt file
    /// adds the individual items to a CookieContainer
    /// It needs sessionid and csrftoken
    /// </summary>
    public void ReadCookies()
    {
        if (!File.Exists("cookies.txt"))
        {
            return;
        }

        foreach (string line in File.ReadLines("cookies.txt"))
        {
            if (line.Length > 0 && line[0] != '#')
            {
                ParseCookies(line);
            }
        }
    }

    /// <summary>
    /// Properly reads and parses a pre-defined cookies.txt file
    /// adds the individual items to a CookieContainer
    /// It needs sessionid and csrftoken
    /// </summary>
    /// <param name="cookies_path">A path to the cookies file</param>
    public void ReadCookies(string cookies_path)
    {
        if (!File.Exists(cookies_path))
        {
            return;
        }

        foreach (string line in File.ReadLines(cookies_path))
        {
            if (line.Length > 0 && line[0] != '#')
            {
                ParseCookies(line);
            }
        }
    }

    private void ParseCookies(string line)
    {
        List<string> temp = new();
        temp.AddRange(line.Split('\t'));

        if (temp.Contains("sessionid"))
        {
            cc.Add(new Cookie("sessionid", temp[temp.IndexOf("sessionid") + 1]) { Domain = "maktabkhooneh.org" });
        }

        if (temp.Contains("csrftoken"))
        {
            cc.Add(new Cookie("csrftoken", temp[temp.IndexOf("csrftoken") + 1]) { Domain = "maktabkhooneh.org" });
        }
    }

    public void GetLectureLinks()
    {
        foreach (var node in doc.DocumentNode.SelectNodes("//a[@class=\"chapter__unit\"]"))
        {
            course_urls.Add($"{base_link}{node.Attributes["href"].Value.Trim()}");
            if (course_urls.Count >= 3)
            {
                break;
            }
        }

        Lectures = course_urls.Count;
    }

    public void GetVideoLinks()
    {
        foreach (var link in course_urls)
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(link);
            web.CookieContainer = cc;
            var res = web.GetResponse();
            var resStream = res.GetResponseStream();
            StreamReader read = new StreamReader(resStream, Encoding.UTF8);
            File.WriteAllText("index.html", read.ReadToEnd());
            HtmlDocument doc = new();
            doc.Load("index.html");
            // Thread.Sleep(2500);
            string courseTitle = doc.DocumentNode.SelectSingleNode("//html/head/title").InnerText;
            string courseVidUrl;
            try
            {
                courseVidUrl = doc.DocumentNode.SelectSingleNode("//*[@class=\"button--round\"]").Attributes["href"]
                    .Value;
                courls_vid_name.Add(ToUtf8(courseTitle), courseVidUrl);
            }
            catch (Exception)
            {
                not_found_links.Add(link);
            }
        }
    }
}
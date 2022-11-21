using System.Net;
using ShellProgressBar;
using System.Text;
using Downloader;
using System.Net.Http;
using maktab_dl;

ProgressBar ConsoleProgress;
ProgressBarOptions ChildOption;
ProgressBarOptions ProcessBarOption;

ProcessBarOption = new ProgressBarOptions
{
    ForegroundColor = ConsoleColor.Green,
    ForegroundColorDone = ConsoleColor.DarkGreen,
    BackgroundColor = ConsoleColor.DarkGray,
    BackgroundCharacter = '\u2593',
    EnableTaskBarProgress = true,
    ProgressBarOnBottom = false,
    ProgressCharacter = '#'
};
ChildOption = new ProgressBarOptions
{
    ForegroundColor = ConsoleColor.Yellow,
    BackgroundColor = ConsoleColor.DarkGray,
    ProgressCharacter = '-',
    ProgressBarOnBottom = true
};

string ToUtf8(string text)
{
    byte[] bytes = Encoding.Default.GetBytes(text);
    text = Encoding.UTF8.GetString(bytes);
    return text;
}

var downloadOpt = new DownloadConfiguration()
{
    ChunkCount = 8, // file parts to download, default value is 1
    ParallelDownload = true // download parts of file as parallel or not. Default value is false
};

var downloader = new DownloadService(downloadOpt);

void OnDownloadStarted(object sender, DownloadStartedEventArgs e)
{
    ConsoleProgress = new ProgressBar(10000, $"Downloading {Path.GetFileName(e.FileName)}   ", ProcessBarOption);
    ConsoleProgress.Spawn(1000, "Downloading", ChildOption);
    Console.WriteLine($"Started downloading {e.FileName}");
}

static void OnDownloadFinished(object sender, DownloadDataCompletedEventArgs e)
{
    Console.WriteLine($"Finished downloading file {e.Result}");
}

downloader.DownloadStarted += OnDownloadStarted;
// downloader.DownloadFileCompleted += OnDownloadFinished;

Console.Write("Course Link: ");
string link = Console.ReadLine();

Maktab mk = new(link);
// Maktab mk = new(
//     "https://maktabkhooneh.org/course/%D8%B3%DB%8C%DA%AF%D9%86%D8%A7%D9%84-%D9%88-%D8%B3%D9%8A%D8%B3%D8%AA%D9%85-mk75/");
mk.ReadCookies();
mk.GetLectureLinks();
Console.WriteLine(mk.Lectures);
mk.GetVideoLinks();

int i = 1;
Console.WriteLine($"Base Folder is: {ToUtf8(mk.base_folder)}");
foreach (var item in mk.courls_vid_name)
{
    // DirectoryInfo path = new DirectoryInfo(mk.base_folder);
    Console.WriteLine($"{ToUtf8(mk.course_name + " " + i)} -> {item.Value}");
    // downloader.DownloadFileTaskAsync(item.Value, $"{mk.base_folder}/{item.Key}");
    // HttpClient client = new();
    // await client.GetStreamAsync(new Uri(item.Value));
    // await downloader.DownloadFileTaskAsync(item.Value, path);
    // i++;
}

Console.ReadLine();

// Console.WriteLine("But Lost This: ");
// foreach (var item in not_found_links)
// {
//     Console.WriteLine($"{item}");
// }
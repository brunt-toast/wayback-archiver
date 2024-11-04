using System.Text.RegularExpressions;

namespace WaybackArchiver;

internal class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Usage: wayback-archiver <file>");
            Environment.Exit(1);
        }

        if (!File.Exists(args[0]))
        {
            Console.Error.WriteLine($"{args[0]} is not a file");
            Environment.Exit(1);
        }

        var fileContent = File.ReadAllText(args[0]);

        var urlRegex = new Regex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
        var matches = urlRegex.Matches(fileContent);

        var client = new HttpClient();
        var longestUrlLength = matches.Select(x => x.ToString().Length).Max();

        for (int i = 0; i < matches.Count; i++)
        {
            var url = matches[i].ToString();
            string displayUrl = (url.Length < 65 ? url : string.Join("", url.Take(62)) + "...");
            Console.Write($"{(i + 1).ToString().PadLeft(matches.Count.ToString().Length, '0')} of {matches.Count}: {displayUrl.PadRight(Math.Min(70, longestUrlLength))} - ");

            HttpResponseMessage? res;
            try
            {
                res = client.GetAsync($"https://web.archive.org/save/{url}").Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while getting {url}:");
                Console.WriteLine(ex);
                continue;
            }
            if (res.IsSuccessStatusCode)
            {
                Console.WriteLine("OK");
            }
            else
            {
                Console.Error.WriteLine("Error");
            }
        }
    }
}

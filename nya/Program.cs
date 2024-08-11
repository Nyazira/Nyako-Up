using nya.ConfigModel;
using System.Net.Http.Headers;
using System.Text;
using Tomlet;
using Tomlet.Models;

namespace nya
{
    internal class Program
    {
        static string Bucket { get; set; } = string.Empty;
        static string Operator { get; set; } = string.Empty;
        static string Password { get; set; } = string.Empty;
        static string Path { get; set; } = string.Empty;
        static string Url { get; set; } = string.Empty;
        static string DestinationFolder { get; set; } = string.Empty;
        static readonly string apiDomain = "http://v0.api.upyun.com";

        static async Task Main(string[] args)
        {
            Init();
            if (args.Length > 0)
            {
                foreach (string arg in args)
                {
                    string localFilePath = arg;
                    string fileName = System.IO.Path.GetFileName(localFilePath);
                    string remoteFilePath = Path + fileName;
                    await UploadFileAsync(localFilePath, remoteFilePath);
                }
            }
            else
            {
                Console.WriteLine("Invalid arguments. Please provide a file name.");
            }
        }

        public static void Init()
        {
            try
            {
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string configFile = System.IO.Path.Combine(exeDirectory, "config.toml");
                TomlDocument document = TomlParser.ParseFile(configFile);
                Config upyun = TomletMain.To<Config>(document);
                Bucket = upyun.Bucket;
                Operator = upyun.Operator;
                Password = upyun.Password;
                Path = upyun.Path;
                Url = upyun.Url;
                DestinationFolder = upyun.DestinationFolder;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static async Task UploadFileAsync(string localFilePath, string remoteFilePath)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Operator}:{Password}")));
                using (FileStream fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
                {
                    var content = new StreamContent(fileStream);
                    var response = await client.PutAsync($"{apiDomain}/{Bucket}/{remoteFilePath}", content);
                    try
                    {
                        if (response.EnsureSuccessStatusCode().IsSuccessStatusCode)
                        {
                            Console.WriteLine($"{Url}/{remoteFilePath}");
                            if (DestinationFolder.Trim() != string.Empty)
                            {
                                if (!Directory.Exists(DestinationFolder))
                                {
                                    Directory.CreateDirectory(DestinationFolder);
                                }
                                string destinationFile = System.IO.Path.Combine(DestinationFolder, System.IO.Path.GetFileName(localFilePath));
                                try
                                {
                                    File.Copy(localFilePath, destinationFile, true);
                                }
                                catch (Exception ex)
                                {
                                    // 记录异常信息到nya.log文件
                                    string logFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nya.log");
                                    File.AppendAllText(logFilePath, $"{DateTime.Now}: {ex.Message}{Environment.NewLine}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(await response.Content.ReadAsStringAsync());
                        string logFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nya.log");
                        File.AppendAllText(logFilePath, $"{DateTime.Now}: {ex.Message}{Environment.NewLine}");
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;

namespace AdvertisingPolicyValidationTool
{
    internal class Program
    {
        private const string OutputFileName = "Output.csv";
        private const string LogFileName = "trace.log";

        private static ConcurrentQueue<DomainOutput> _domainOutputQueue = new ConcurrentQueue<DomainOutput>();
        private static bool _inProgress = true;
        private static CSVFileClient csvClient;

        private static readonly List<string> _keyWords = new List<string> {
                "jammers","hacking","drug","paraphernalia","marijuana","salvia","salvinorin", "CBD","Ivory","endangered","threatened","shark","hunting","fireworks","explosives",
                "hazardous","pyrotechnic","ponzi","violent","rape","torture","cannibalism","death","self-harm","violence","blood","guns",
                "fatality","cigarettes","cigars","tobacco","blunt","nicotine","smoking","bribes","usenet"
            };
        static async Task Main()
        {
            Console.WriteLine("Microsoft Advertising Policy Validation Tool");

            Logger.LogFilePath = LogFileName;
            Logger.LogLineBreak();

            var filePath = string.Empty;
            while (true)
            {
                Console.Write("\nEnter the input domain list csv file path: ");
                filePath = Console.ReadLine();

                if (File.Exists(filePath))
                {
                    break;
                }

                Console.WriteLine("Invalid file path");
            }

            var maxParallelism = Environment.ProcessorCount;
            Console.Write($"\nEnter number of parallel task for execution (default {maxParallelism} - Max Processor Count of the System): ");
            if (!int.TryParse(Console.ReadLine(), out maxParallelism))
            {
                maxParallelism = Environment.ProcessorCount;
            }

            // reading file
            Console.WriteLine("Reading file");

            csvClient = new CSVFileClient(filePath, OutputFileName);
            var inputDomains = csvClient.ReadFile();

            var logMessage = $"Domains found: {inputDomains.Count}";
            Console.WriteLine(logMessage);
            Logger.LogInfo(logMessage);

            logMessage = $"Disallowed Keywords: {_keyWords.Count}";
            Console.WriteLine($"{logMessage}\n");
            Logger.LogInfo(logMessage);


            // initiating output list
            _domainOutputQueue = new ConcurrentQueue<DomainOutput>();

            // task scheduling
            Console.WriteLine("Scheduling tasks");
            var numberOfTasks = inputDomains.Count < maxParallelism ? inputDomains.Count : maxParallelism;
            var domainsPerTask = inputDomains.Count / numberOfTasks;

            logMessage = $"Number of workers: {numberOfTasks}, Domains per worker: {domainsPerTask}";
            Console.WriteLine(logMessage);
            Logger.LogInfo(logMessage);

            var startTime = DateTime.Now;
            logMessage = $"Started: {startTime.ToShortDateString()} {startTime.ToLongTimeString()}";
            Console.WriteLine($"\n{logMessage}");
            Logger.LogInfo(logMessage);

            var taskList = new List<Task>();
            for (var i = 0; i < numberOfTasks; i++)
            {
                var itemsToSkip = i * domainsPerTask;
                // for last task take remaining works
                if (i == numberOfTasks - 1)
                {
                    domainsPerTask += (inputDomains.Count - (i + 1) * domainsPerTask);
                }

                taskList.Add(CheckDomain(i + 1, inputDomains.Skip(itemsToSkip).Take(domainsPerTask).ToList()));
            }

            // start the output task in background
            var outputTask = Task.Run(() => WriteToFile());

            // wait for the operation to complete
            await Task.WhenAll(taskList.ToArray());
            var endTime = DateTime.Now;
            var elapsed = endTime - startTime;

            // setting in progress to false to signal the write task to end
            _inProgress = false;

            // wait for the write task to write pending items
            outputTask.Wait();

            logMessage = $"Completed: {endTime.ToShortDateString()} {endTime.ToLongTimeString()}, Elapsed: {(int)elapsed.TotalHours % 60}h: {(int)elapsed.TotalMinutes % 60}m: {(int)elapsed.TotalSeconds % 60}s";
            Console.WriteLine($"\n{logMessage}");
            Logger.LogInfo(logMessage);

            // write to the file
            logMessage = $"Writing results to {Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)?.Replace("file:\\", "")}\\{OutputFileName}";
            Console.WriteLine($"\n{logMessage}");
            Logger.LogInfo(logMessage);

            // open the file
            System.Diagnostics.Process.Start(OutputFileName);

            Console.WriteLine("\nPress any key to close");
            Console.ReadLine();
        }

        private static async Task CheckDomain(int workerId, List<DomainInput> domains)
        {
            /// puppeteer headlessbroswer
            var browser = await new WebPageReader().CreateBrowser();

            foreach (var (domain, index) in domains.Select((value, i) => (value, i)))
            {
                // output object
                var domainOutput = domain.CloneOutput(workerId);

                var pageUrl = $"https://{domain.Name}";
                var logMessage = $"Worker: {workerId}, Processing Domain {index + 1}/{domains.Count}: {pageUrl}";
                Console.Write($"\r                                                                                      ");
                Console.Write($"\r{logMessage}");
                Logger.LogInfo(logMessage);

                var htmlString = string.Empty;

                try
                {
                    var page = await browser.NewPageAsync();
                    await page.GoToAsync(pageUrl);
                    // read html content
                    htmlString = (await page.EvaluateExpressionAsync("document.documentElement.innerHTML")).ToString();
                    await page.CloseAsync();
                }
                catch (Exception ex)
                {
                    domainOutput.Error = ex.Message;
                    _domainOutputQueue.Enqueue(domainOutput);
                    Logger.LogError(ex.Message);
                    continue;
                }

                var disallowedWords = new Dictionary<string, int>();

                foreach (var keyword in _keyWords)
                {
                    var matches = Regex.Matches(htmlString, keyword.Trim().ToLowerInvariant());
                    if (matches.Count > 1)
                    {
                        disallowedWords.Add(keyword, matches.Count);
                    }
                }

                // for csv
                domainOutput.IsDisalloweded = disallowedWords.Any();
                domainOutput.Words = disallowedWords.Any()
                 ? string.Join(" ", disallowedWords.Select(x => $"{x.Key}={x.Value}"))
                 : string.Empty;

                // add to the output list
                _domainOutputQueue.Enqueue(domainOutput);
            }

            await browser.CloseAsync();
        }

        public static void WriteToFile()
        {
            while (_inProgress || !_domainOutputQueue.IsEmpty)
            {
                if (_domainOutputQueue.TryDequeue(out var result))
                {
                    csvClient.UpdateOutput(result);
                }
            }

            csvClient.CloseOutput();
        }
    }
}

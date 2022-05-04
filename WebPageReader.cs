using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace AdvertisingPolicyValidationTool
{
    internal class WebPageReader
    {
        public async Task<Browser> CreateBrowser()
        {
            await new BrowserFetcher(Product.Chrome).DownloadAsync();
            var browser = await Puppeteer.LaunchAsync(
                    new LaunchOptions
                    {
                        Args =
                            new[]
                            {
                                "--disable-gpu",
                                "--incognito",
                                "--no-sandbox",
                                "--no-zygote",
                                "--single-process"
                            },
                        LogProcess = true
                    });

            return browser;
        }
    }
}

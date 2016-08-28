using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace TaskParser
{
    internal class Program
    {
        private static void Main()
        {
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            var requestUri = @"http://novoaltaysk.online/category/gorod/page/1/";

            var tasks = Enumerable.Range(0, 1)
                .Select(n => requestUri)
                .Select(async url =>
                {
                    using (var client = new HttpClient())
                        return await client.GetStringAsync(url).ConfigureAwait(false);
                })
                .SelectMany(content =>
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(content.Result);

                    return
                        doc.DocumentNode.SelectNodes("//a[@rel='bookmark']/@href")
                            .Select(node => node.Attributes["href"].Value)
                            .Distinct();
                })
                .Select(async url =>
                {
                    using (var client = new HttpClient())
                        return await client.GetStringAsync(url).ConfigureAwait(false);
                })
                .Select(async content =>
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(await content.ConfigureAwait(false));

                    return string.Join(Environment.NewLine,
                        doc.DocumentNode.SelectNodes("//div[@role='main']").Select(s => s.InnerText));
                })
                .ToArray();

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);


            Console.WriteLine("Done!");

            Debugger.Break();
        }
    }
}
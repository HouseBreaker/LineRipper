namespace LineRipper
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text.RegularExpressions;

	using HtmlAgilityPack;
	using System.Threading.Tasks;

	public static class LineRipperMain
	{
		public static void Main(string[] args)
		{
#if DEBUG
			args = new string[] { "https://store.line.me/stickershop/product/1602439/en" };
#endif
			string url;
			if (args.Length < 1)
			{
				Console.Write("Paste the link to the sticker pack on the LINE website: ");
				url = Console.ReadLine();
			}
			else
			{
				url = args[0];
			}

			//const string IdRegex = @"https:\/\/store\.line\.me\/stickershop\/product\/(?<id>\d+)\/en";
			//var id = Regex.Match(url, IdRegex).Groups["id"].Value;

			string siteData;
			using (var client = new WebClient())
			{
				siteData = client.DownloadString(url);
			}

			var doc = new HtmlDocument();
			doc.LoadHtml(siteData);

			var stickerPackName = GetStickerPackName(doc);

			Console.WriteLine("Name: " + stickerPackName);

			var outputPath = "stickers\\" + Utilities.RemoveInvalidPathChars(stickerPackName);
			Directory.CreateDirectory(outputPath);

			var stickerUrls = GetStickerUrls(doc);

			var counter = 0;


			Parallel.For(0, stickerUrls.Length, index =>
			{
				var stickerUrl = stickerUrls[index];
				using (var client = new WebClient())
				{
					var fileName = $@"{outputPath}\{index + 1}.png";
					client.DownloadFile(stickerUrl, fileName);
				}
				Console.Write("\r" + $"{++counter}/{stickerUrls.Length} downloaded.");
			});


			Console.WriteLine();

			Console.WriteLine("Done!");
		}

		private static string GetStickerPackName(HtmlDocument doc)
		{
			const string nameId = "mdCMN08Ttl";

			var headings = doc.DocumentNode
				.Descendants("h3")
				.ToArray();

			var heading = headings
				.First(a => a.Attributes.Contains("class") && a.Attributes["class"].Value == nameId);

			var name = heading.InnerText;

			return name;
		}

		private static string[] GetStickerUrls(HtmlDocument doc)
		{
			const string stickerClass = "mdCMN09Image";

			var stickerUrls = doc.DocumentNode.Descendants("span")
				.Where(a => a.Attributes.Contains("class") && a.Attributes["class"].Value == stickerClass)
				.Select(node => ParseUrl(node))
				.ToArray();

			return stickerUrls;
		}

		private static string ParseUrl(HtmlNode node)
		{
			string attributeValue = GetStyleAttributeValue(node);

			string url = ParseStyleString(attributeValue);

			return url;
		}

		private static string GetStyleAttributeValue(HtmlNode node)
		{
			var attributes = node.Attributes;

			var styleAttribute = attributes.FirstOrDefault(attr => attr.Name == "style");

			return styleAttribute.Value;
		}

		private static string ParseStyleString(string style)
		{
			var pattern = @"background-image:url\((?<link>.+?);.*\)";

			var match = Regex.Match(style, pattern);
			var link = match.Groups["link"].Value;

			return link;
		}
	}
}
namespace LineRipper
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text.RegularExpressions;

	using HtmlAgilityPack;

	public static class LineRipperMain
	{
		public static void Main(string[] args)
		{
			var stickerClass = "mdCMN09Image";

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

			const string IdRegex = @"https:\/\/store\.line\.me\/stickershop\/product\/(?<id>\d+)\/en";
			var id = Regex.Match(url, IdRegex).Groups["id"].Value;

			string siteData;
			using (var client = new WebClient())
			{
				siteData = client.DownloadString(url);
			}

			var doc = new HtmlDocument();
			doc.LoadHtml(siteData);

			const string nameId = "mdCMN08Ttl";

			var name =
				doc.DocumentNode
					.Descendants("h3").First(a => a.Attributes.Contains("class") && a.Attributes["class"].Value == nameId).InnerText;

			Console.WriteLine("Name: " + name);

			var outputPath = "stickers\\" + name;
			Directory.CreateDirectory(outputPath);

			var stickerIds =
				doc.DocumentNode.Descendants("span")
					.Where(a => a.Attributes.Contains("class") && a.Attributes["class"].Value == stickerClass)
					.Select(a => a.Attributes[2].Value)
					.ToArray();

			const string downloadUrl = "https://sdl-stickershop.line.naver.jp/products/0/0/1/{0}/android/stickers/{1}.png";

			for (var index = 0; index < stickerIds.Length; index++)
			{
				var stickerId = stickerIds[index];
				using (var client = new WebClient())
				{
					var stickerUrl = string.Format(downloadUrl, id, stickerId);
					client.DownloadFile(stickerUrl, outputPath + "\\" + stickerId + ".png");
				}

				Console.Write("\r" + $"{index+1}/{stickerIds.Length} downloaded.");
			}
			Console.WriteLine();

			Console.WriteLine("Done!");
		}
	}
}
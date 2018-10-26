namespace LineRipper
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text.RegularExpressions;

	using HtmlAgilityPack;
	using System.Threading.Tasks;
	using ImageMagick;

	public static class LineRipperMain
	{
		public static void Main(string[] args)
		{
			Console.Write("Paste the link to the sticker pack on the LINE website: ");
			var url = Console.ReadLine();

			Console.Write("Should the images be resized to Telegram sticker size (512px)? (y/n): ");
			var shouldResize = Console.ReadLine().Equals("y", StringComparison.InvariantCultureIgnoreCase);

			Console.WriteLine("------------------");
			Console.WriteLine("Downloading Stickers");
			string siteHtml;
			using (var client = new WebClient())
			{
				siteHtml = client.DownloadString(url);
			}

			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(siteHtml);

			var stickerPackName = GetStickerPackName(htmlDoc);

			Console.WriteLine($"Name: {stickerPackName}");
			var outputPath = CreateOutputDirectory(stickerPackName);

			var stickerUrls = GetStickerUrls(htmlDoc);

			var downloadedStickers = 0;

			// for (int index = 0; index < stickerUrls.Length; index++)
			Parallel.For(0, stickerUrls.Length, index =>
			{
				var stickerUrl = stickerUrls[index];
				using (var client = new WebClient())
				{
					var fileName = $@"{outputPath}\{index + 1}.png";

					var bytes = client.DownloadData(stickerUrl);

					if (shouldResize)
					{
						bytes = ResizeImage(bytes, 512);
					}

					File.WriteAllBytes(fileName, bytes);
				}

				Console.Write("\r" + $"{++downloadedStickers}/{stickerUrls.Length} downloaded.");
			});
			// }

			Console.WriteLine();
			Console.Write("Done! Press any key to exit. . .");
			Console.ReadKey();
		}

		private static byte[] ResizeImage(byte[] bytes, int greaterDimensionPx)
		{
			var image = new MagickImage(bytes);

			MagickGeometry geometry;
			if (image.Width >= image.Height)
			{
				geometry = new MagickGeometry
				{
					Width = greaterDimensionPx,
				};
			}
			else
			{
				geometry = new MagickGeometry
				{
					Height = 512,
				};
			}

			image.Resize(geometry);

			return image.ToByteArray();
		}

		private static string CreateOutputDirectory(string stickerPackName)
		{
			var outputPath = "stickers\\" + Utilities.RemoveInvalidPathChars(stickerPackName);
			Directory.CreateDirectory(outputPath);
			return outputPath;
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
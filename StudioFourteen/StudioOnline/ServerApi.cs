// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioOnline;

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public static class ServerApi
{
	public static string Url = "https://fourteen.studio/api/";

	private static readonly HttpClient Client;

	static ServerApi()
	{
		HttpClientHandler handler = new();
		handler.AutomaticDecompression = DecompressionMethods.All;

		Client = new(handler);
	}

	public static async Task<string> GetAsync(string uri)
	{
		using HttpResponseMessage response = await Client.GetAsync(Url + uri);

		return await response.Content.ReadAsStringAsync();
	}

	public static async Task<string> PostAsync(string uri, string data, string contentType)
	{
		using HttpContent content = new StringContent(data, Encoding.UTF8, contentType);

		HttpRequestMessage requestMessage = new HttpRequestMessage()
		{
			Content = content,
			Method = HttpMethod.Post,
			RequestUri = new Uri(Url + uri),
		};

		using HttpResponseMessage response = await Client.SendAsync(requestMessage);

		return await response.Content.ReadAsStringAsync();
	}
}

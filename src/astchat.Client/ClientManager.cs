using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace astchat.Client
{
	public class ClientManager
	{
		private Dictionary<string, WebSocket> socketDic = new Dictionary<string, WebSocket>();

		public WebSocket ConnectChannel(string channel)
		{
			if (channel == null) throw new ArgumentNullException(nameof(channel));
			if (channel.Trim() == string.Empty) channel = "lobby";

			channel = channel.ToLower();

			if (this.socketDic.ContainsKey(channel) && this.socketDic[channel] != null)
			{
				return this.socketDic[channel];
			}
			else
			{
				WebSocket ws = new WebSocket(string.Format("wss://chat.antnf.com/ws?channel={0}", channel));

				return ws;
			}
		}

		public void DisconnetChannel(string channel)
		{
			if (channel == null) throw new ArgumentNullException(nameof(channel));
			if (channel.Trim() == string.Empty) channel = "lobby";

			channel = channel.ToLower();

			if (this.socketDic.ContainsKey(channel))
			{
				WebSocket ws = this.socketDic[channel];
				if (ws != null)
				{
					ws.Close(CloseStatusCode.Normal);
				}

				this.socketDic.Remove(channel);
			}
		}

		public static void SendPureText(WebSocket ws, string message)
		{
			string json = JsonConvert.SerializeObject(new
			{
				message = message,
				time = FormatDateTime(DateTime.Now),
				user = string.Empty
			});

			ws.Send(json);
		}

		public static string ParsePureText(string json)
		{
			var jsonObj = new
			{
				message = default(string),
				time = default(long),
				user = default(string)
			};
			jsonObj = JsonConvert.DeserializeAnonymousType(json, jsonObj);
			return jsonObj.message;
		}

		public static void SendImage(WebSocket ws, string imageUrl)
		{
			SendPureText(ws, string.Format("<img class=\"ui msg rounded image\" src=\"{0}\">", imageUrl));
        }

		public static bool TryParseImage(string json, out string imageUrl)
		{
			string message = ParsePureText(json);
			System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(message, "<img class\\=\"ui msg rounded image\" src\\=\"(?<imageUrl>.*)\">");

			imageUrl = null;
			if (!m.Success) return false;

			imageUrl = m.Groups["imageUrl"].Value;
			
			return true;
        }

		public static long ParseTime(string json)
		{
			var jsonObj = new
			{
				message = default(string),
				time = default(long),
				user = default(string)
			};
			jsonObj = JsonConvert.DeserializeAnonymousType(json, jsonObj);
			return jsonObj.time;
		}
		
		public static long FormatDateTime(DateTime time)
		{
			TimeSpan span = time - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
			return (long)span.TotalMilliseconds; ;
		}

		public static DateTime FormatUTC(long time)
		{
			return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddMilliseconds(time);
		}
	}
}

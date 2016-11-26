using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Astchat.Client
{
	/// <summary>
	/// Astchat客户端管理。
	/// </summary>
	public class ClientManager : IDataJsonSerializable<DataPackage>
	{
		/// <summary>
		/// 所有已管理的<see cref="WebSocket"/>的字典。
		/// </summary>
		private Dictionary<string, WebSocket> socketDic = new Dictionary<string, WebSocket>();

		/// <summary>
		/// 添加一个连接指定频道的<see cref="WebSocket"/>。
		/// </summary>
		/// <param name="channel">
		/// <para>指定的频道。</para>
		/// <para>如果值为<see cref="string.Empty"/>，则默认频道为<c>"lobby"</c>。</para>
		/// </param>
		/// <returns>
		/// <para>连接参数<paramref name="channel"/>指定的频道的<see cref="WebSocket"/>。</para>
		/// <para>如果已经连接指定频道，则返回连接指定频道的<see cref="WebSocket"/>。</para>
		/// <para>如果未连接指定频道，则新建一个<see cref="WebSocket"/>并返回。</para>
		/// </returns>
		/// <remarks>
		/// 该方法不会自动启动<see cref="WebSocket"/>，需要显示调用<see cref="WebSocket.Connect"/>方法。
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// 参数<paramref name="channel"/>的值为<see langword="null"/>。
		/// </exception>
		public WebSocket AddChannel(string channel)
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

				if (this.socketDic.ContainsKey(channel))
					this.socketDic.Add(channel, ws);
				else
					this.socketDic[channel] = ws;

				return ws;
			}
		}

		/// <summary>
		/// 移除添加一个连接指定频道的<see cref="WebSocket"/>。
		/// </summary>
		/// <param name="channel">
		/// <para>指定的频道。</para>
		/// <para>如果值为<see cref="string.Empty"/>，则默认频道为<c>"lobby"</c>。</para>
		/// </param>
		/// <remarks>
		/// 如果连接参数<paramref name="channel"/>指定的频道的<see cref="WebSocket"/>处于连接状态，该方法将自动调用<see cref="WebSocket.Close()"/>方法关闭这个连接。强烈建议显示调用<see cref="WebSocket.Close()"/>以及其他参数重载的方法。
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// 参数<paramref name="channel"/>的值为<see langword="null"/>。
		/// </exception>
		public void RemoveChannel(string channel)
		{
			if (channel == null) throw new ArgumentNullException(nameof(channel));
			if (channel.Trim() == string.Empty) channel = "lobby";

			channel = channel.ToLower();

			if (this.socketDic.ContainsKey(channel))
			{
				WebSocket ws = this.socketDic[channel];
				if (ws != null)
				{
					if (ws.IsAlive) // 如果WebSocket处于连接状态，则关闭这个连接。
						ws.Close();
				}

				this.socketDic.Remove(channel);
			}
		}

		/// <summary>
		/// 向指定的频道发送数据包。
		/// </summary>
		/// <param name="channel">指定的频道。</param>
		/// <param name="package">发送的数据包。</param>
		public void SendDataPackage(string channel, DataPackage package)
		{
			string json = this.JsonSerializeDataPackage(package);
			WebSocket ws = this.AddChannel(channel);

			ws.Send(json);
		}

		/// <summary>
		/// 向指定的频道发送纯文本消息。
		/// </summary>
		/// <param name="channel">指定的频道。</param>
		/// <param name="message">纯文本消息。</param>
		public void SendPureText(string channel, string message)
		{
			this.SendDataPackage(channel, new DataPackage()
			{
				message = message,
				time = FormatDateTime(DateTime.Now),
				user = string.Empty
			});
		}

		/// <summary>
		/// 从消息中获取纯文本。
		/// </summary>
		/// <param name="json">JSON格式字符串。</param>
		/// <returns>纯文本消息。</returns>
		public string ParsePureText(string json)
		{
			DataPackage package = this.JsonDeserializeDataPackage(json);
			return package.message;
		}

		/// <summary>
		/// 向指定的频道发送图片消息。
		/// </summary>
		/// <param name="channel">指定的频道。</param>
		/// <param name="imageUrl">图片URL。</param>
		public void SendImage(string channel, string imageUrl)
		{
			const string SERVER_IMAGE_HTML = "<img class=\"ui msg rounded image\" src=\"{0}\">"; // Astchat服务器包装图片URL的HTML元素，优化解析。
			SendPureText(channel, string.Format(SERVER_IMAGE_HTML, imageUrl));
        }

		/// <summary>
		/// 尝试从消息中获取图片。
		/// </summary>
		/// <param name="json">JSON格式字符串。</param>
		/// <param name="imageUrl">图片URL。</param>
		/// <returns>图片是否获取成功。</returns>
		public bool TryParseImage(string json, out string imageUrl)
		{
			const string SERVER_IMAGE_HTML_REGEX = "<img class\\=\"ui msg rounded image\" src\\=\"(?<ImageUrl>.*)\">"; // 匹配Astchat服务器包装图片URL的HTML元素的正则表达式。
			const string SERVER_IMAGE_HTML_REGEX_GROUP_PATTERN = "ImageUrl"; // 匹配Astchat服务器包装图片URL的HTML元素的正则表达式组名称。
			string message = ParsePureText(json);
			Match m = Regex.Match(message, SERVER_IMAGE_HTML_REGEX);

			imageUrl = null;
			if (!m.Success) return false;

			imageUrl = m.Groups[SERVER_IMAGE_HTML_REGEX_GROUP_PATTERN].Value;
			
			return true;
        }

		/// <summary>
		/// 从消息中获取发送时间。
		/// </summary>
		/// <param name="json">JSON格式字符串。</param>
		/// <returns>消息发送的时间。</returns>
		public long ParseTime(string json)
		{
			DataPackage package = this.JsonDeserializeDataPackage(json);
			return package.time;
		}
		
		/// <summary>
		/// 将本地<see cref="DateTime"/>时间转换为UTC时间。
		/// </summary>
		/// <param name="time">本地<see cref="DateTime"/>时间。</param>
		/// <returns>UTC时间。</returns>
		public static long FormatDateTime(DateTime time)
		{
			TimeSpan span = time - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
			return (long)span.TotalMilliseconds;
		}

		/// <summary>
		/// 将UTC时间转换为本地<see cref="DateTime"/>时间。
		/// </summary>
		/// <param name="time">UTC时间。</param>
		/// <returns>本地<see cref="DateTime"/>时间。</returns>
		public static DateTime FormatUTC(long time)
		{
			return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddMilliseconds(time);
		}

		/// <summary>
		/// 将数据包序列化为JSON格式字符串。
		/// </summary>
		/// <param name="package">数据包。</param>
		/// <returns>JSON格式字符串。</returns>
		public string JsonSerializeDataPackage(DataPackage package)
		{
			return JsonConvert.SerializeObject(package);
		}

		/// <summary>
		/// 将JSON格式化字符串反序列化为数据包。
		/// </summary>
		/// <param name="json">JSON格式化字符串。</param>
		/// <returns>数据包。</returns>
		public DataPackage JsonDeserializeDataPackage(string json)
		{
			return JsonConvert.DeserializeObject<DataPackage>(json);
		}

		string IDataJsonSerializable<DataPackage>.JsonSerialize(DataPackage data)
		{
			return this.JsonSerializeDataPackage(data);
		}

		DataPackage IDataJsonSerializable<DataPackage>.JsonDeserialize(string json)
		{
			return this.JsonDeserializeDataPackage(json);
		}
	}
}

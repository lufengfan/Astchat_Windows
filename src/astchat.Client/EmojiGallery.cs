using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Astchat.Client
{
	/// <summary>
	/// Emoji表情库。
	/// </summary>
	public static class EmojiGallery
	{
		/// <summary>
		/// 所有支持的Emoji表情信息的字典。
		/// </summary>
		public static readonly Dictionary<string, EmojiInfo> EmojiDic;

		static EmojiGallery()
		{
			EmojiDic = new Dictionary<string, EmojiInfo>();

			string file = @"emoji.json";
			string emoji_json;
			if (File.Exists(file))
				emoji_json = File.ReadAllText(file); // 从文件中读取记录Emoji表情格式的JSON文件内容。
			else
				emoji_json = Encoding.UTF8.GetString(Properties.Resources.emoji_json); // 从程序集资源中读取记录Emoji表情格式的JSON文件内容。

			JObject items = JsonConvert.DeserializeObject(emoji_json) as JObject;
			foreach (JProperty emoji in items.Properties())
			{
				EmojiInfo ei = new EmojiInfo();
				foreach (JProperty emoji_info in (emoji.Value as JObject).Properties())
				{
					JToken value = emoji_info.Value;
					object ei_value = null;
					if (value.Type == JTokenType.Array)
					{
						ei_value = value.Select((token) => token.ToString()).ToArray();
					}
					else
					{
						ei_value = value.ToString();
					}

					// 反射技术填充数据。
					typeof(EmojiInfo).GetProperty(emoji_info.Name).SetValue(ei, ei_value, null);
				}

				EmojiDic.Add(emoji.Name, ei);
			}
		}

		/// <summary>
		/// 获取Emoji图片的URI。
		/// </summary>
		/// <param name="unicode">Emoji的unicode。</param>
		/// <returns>
		/// <para>Emoji图片的URI。</para>
		/// <para>如果本地存在已加载的文件，则返回文件路径。</para>
		/// <para>如果本地不存在已加载的文件，则返回代理服务器上的文件URL。</para>
		/// </returns>
		public static string GetEmojiUri(string unicode)
		{
			const string emoji_directory = @"emojis";
			const string emoji_host = @"https://cdn.jsdelivr.net/emojione/assets/png/";

			string uri = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, emoji_directory), unicode + ".png");
			if (File.Exists(uri))
				return uri;
			else
				return emoji_host + unicode + ".png";
		}
	}
}

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
	public static class EmojiGallery
	{
		public static readonly Dictionary<string, EmojiInfo> EmojiDic;

		static EmojiGallery()
		{
			EmojiDic = new Dictionary<string, EmojiInfo>();

			string file = @"emoji.json";
			string emoji_json;
			if (File.Exists(file))
				emoji_json = File.ReadAllText(file);
			else
				emoji_json = Encoding.UTF8.GetString(Properties.Resources.emoji_json);

			JObject items = JsonConvert.DeserializeObject(emoji_json) as JObject;
			foreach (JProperty emoji in items.Properties())
			{
				EmojiInfo ei = new EmojiInfo();
				foreach (JProperty emoji_info in (emoji.Value as JObject).Properties())
				{
					//JProperty _prop = o as JProperty;
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

					typeof(EmojiInfo).GetProperty(emoji_info.Name).SetValue(ei, ei_value, null);
				}

				EmojiDic.Add(emoji.Name, ei);
			}



			//Type t = obj.GetType();
			//foreach (PropertyInfo pi in t.GetProperties())
			//{
			//	object pv = pi.GetValue(obj, null);
			//	Type _t = pv.GetType();
			//
			//	EmojiInfo ei = new EmojiInfo();
			//	foreach (PropertyInfo _pi in _t.GetProperties())
			//	{
			//		typeof(EmojiInfo).GetProperty(_pi.Name).SetValue(ei, _pi.GetValue(pv, null).ToString(), null);
			//	}
			//
			//	EmojiDic.Add(pi.Name, ei);
			//}
		}

	}
}

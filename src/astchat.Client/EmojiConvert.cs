using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace astchat.Client
{
	public static class EmojiConvert
	{
		public static readonly Dictionary<string, EmojiInfo> EmojiDic;

		static EmojiConvert()
		{
			EmojiDic = new Dictionary<string, EmojiInfo>();

			string file = null;
#warning 在正式发布版明确emoji.json文件的位置
#if DEBUG
			file = @"..\..\emoji.json";
#else
#error 未明确emoji.json文件的位置
#endif

			object obj = JsonConvert.DeserializeObject(File.ReadAllText(file));
			Type t = obj.GetType();
			foreach (PropertyInfo pi in t.GetProperties())
			{
				object pv = pi.GetValue(obj, null);
				Type _t = pv.GetType();

				EmojiInfo ei = new EmojiInfo();
				foreach (PropertyInfo _pi in _t.GetProperties())
				{
					typeof(EmojiInfo).GetProperty(_pi.Name).SetValue(ei, _pi.GetValue(pv, null).ToString(), null);
				}

				EmojiDic.Add(pi.Name, ei);
			}
		}

	}
}

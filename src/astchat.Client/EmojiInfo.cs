using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astchat.Client
{
	public class EmojiInfo
	{
		public string unicode { get; set; }
		public string unicode_alternates { get; set; }
		public string name { get; set; }
		public string shortname { get; set; }
		public string category { get; set; }
		public string emoji_order { get; set; }
		public string[] aliases { get; set; }
		public string[] aliases_ascii { get; set; }
		public string[] keywords { get; set; }
	}
}

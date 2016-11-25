using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astchat.Client
{
	/// <summary>
	/// Astchat客户端一次发送的数据包。
	/// </summary>
	public class DataPackage
	{
		/// <summary>
		/// 消息的内容。
		/// </summary>
		public string message { get; set; }
		/// <summary>
		/// 消息发出的时间。
		/// </summary>
		public long time { get; set; }
		/// <summary>
		/// 发出消息的用户。
		/// </summary>
		public string user { get; set; }
	}
}

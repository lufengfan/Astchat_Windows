using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astchat.Client
{
	/// <summary>
	/// 支持数据序列化为JSON格式和从JSON格式反序列化的接口。
	/// </summary>
	/// <typeparam name="T">序列化和反序列化的数据类型。</typeparam>
	public interface IDataJsonSerializable<T>
	{
		/// <summary>
		/// 将数据序列化为JSON格式字符串。
		/// </summary>
		/// <param name="data">数据。</param>
		/// <returns>JSON格式字符串。</returns>
		string JsonSerialize(T data);
		/// <summary>
		/// 将JSON格式字符串反序列化为数据。
		/// </summary>
		/// <param name="json">JSON格式字符串。</param>
		/// <returns>数据。</returns>
		T JsonDeserialize(string json);
	}
}

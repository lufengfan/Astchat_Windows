using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BugReport
{
	static class Program
	{
		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length < 3) return;
			
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm(args[0], args[1], args.Skip(2).ToArray()));
		}
	}
}

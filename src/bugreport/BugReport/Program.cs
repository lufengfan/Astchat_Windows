using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BugReport
{
	static class Program
	{
		internal static string ProductName { get; set; }
		internal static string ComName { get; set; }
		internal static string ExecutiveName { get; set; }
		internal static string RestartCmdLine { get; set; }
		internal static string[] ReportFiles { get; private set; }

		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length < 4) return;

			try
			{
				Program.ProductName = args[0];
				Program.ComName = args[1];
				Program.ExecutiveName = args[2];
				Program.RestartCmdLine = args[3];
				Program.ReportFiles = args.Skip(4).ToArray();
				
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new MainForm());
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}
	}
}

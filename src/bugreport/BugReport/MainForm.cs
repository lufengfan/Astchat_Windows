using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BugReport
{
	public partial class MainForm : Form
	{
		private Dictionary<file, string> reports;
		
		public MainForm(string productName, string comName, params string[] reportFiles)
		{
			InitializeComponent();

			this.Text = string.Format(this.Text, productName);
			this.cbRestart.Text = string.Format(this.cbRestart.Text, productName);
			this.lblInfo.Text = string.Format(this.lblInfo.Text, comName);

			this.reports = new Dictionary<file, string>();
			foreach (string reportFile in reportFiles)
			{
				file file = new file()
				{
					FullName = Path.GetFullPath(reportFile),
					Name = Path.GetFileName(reportFile)
				};
				this.lbReports.Items.Add(file);
				this.reports.Add(file, File.ReadAllText(file.FullName));
				File.Delete(file.FullName);
			}
		}

		struct file : IEquatable<file>
		{
			public string FullName { get; set; }
			public string Name { get; set; }

			public override bool Equals(object obj)
			{
				return (obj is file && this.Equals((file)obj));
			}

			public bool Equals(file other)
			{
				return (this.FullName == other.FullName && this.Name == other.Name);
			}

			public override int GetHashCode()
			{
				return this.FullName.GetHashCode() ^ this.Name.GetHashCode();
			}

			public override string ToString()
			{
				return this.Name;
			}
		}

		private void lbReports_DoubleClick(object sender, EventArgs e)
		{
			file file = (file)this.lbReports.SelectedItem;
			this.txtReportContent.Text = this.reports[file];
		}
	}
}

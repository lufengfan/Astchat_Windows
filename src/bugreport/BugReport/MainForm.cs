using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Windows.Forms;

namespace BugReport
{
	public partial class MainForm : Form
	{
		private Dictionary<file, FileStream> reports;

		public MainForm()
		{
			InitializeComponent();

			this.Text = string.Format(this.Text, Program.ProductName);
			this.cbRestart.Text = string.Format(this.cbRestart.Text, Program.ProductName);
			this.lblInfo.Text = string.Format(this.lblInfo.Text, Program.ComName);

			this.reports = new Dictionary<file, FileStream>();
			foreach (string reportFile in Program.ReportFiles)
			{
				file file = new file()
				{
					FullName = Path.GetFullPath(reportFile),
					Name = Path.GetFileName(reportFile)
				};
				this.lbReports.Items.Add(file);
				this.reports.Add(file, File.OpenRead(file.FullName));
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
			if (this.lbReports.SelectedItems.Count == 0) return;

			file file = (file)this.lbReports.SelectedItem;
			this.txtReportContent.Text = File.ReadAllText(file.FullName);
		}

		private void btnSend_Click(object sender, EventArgs e)
		{
			MailMessage msg = new MailMessage();
			msg.To.Add("549429518@qq.com");
			msg.From = new MailAddress("549429518@qq.com", "Astchat Bug Report", Encoding.UTF8);
			msg.Subject = "Astchat错误报告"; // 邮件标题  
			msg.SubjectEncoding = Encoding.UTF8; // 邮件标题编码  
			msg.Body = string.Format(
				"<html><body><center><h1>Astchat错误报告</h1></center><h2>错误是如何发生的？</h2><p>{0}</p><h2>其他信息：</h2><p>{1}</p></body></html>",
				HttpUtility.HtmlEncode(this.txt1.Text),
				HttpUtility.HtmlEncode(this.txt2.Text)
			); // 邮件内容  
			msg.BodyEncoding = Encoding.UTF8; // 邮件内容编码  
			msg.IsBodyHtml = true; // 是否是HTML邮件  
			msg.Priority = MailPriority.High; // 邮件优先级 
			foreach (var pair in this.reports)
			{
				msg.Attachments.Add(new Attachment(pair.Value, MediaTypeNames.Text.Plain) { Name = pair.Key.Name + ".xml" });
			}
			SmtpClient client = new SmtpClient("smtp.qq.com");
			client.EnableSsl = true;
			client.UseDefaultCredentials = false;
			client.Credentials = new NetworkCredential("549429518@qq.com", "jbiinuegdlcgbcdi");
			
			try
			{
				client.Send(msg);
				//MessageBox.Show("发送成功");
			}
			catch (SmtpException ex)
			{
				//MessageBox.Show(ex.Message, "发送邮件出错");
			}

			this.cleanReports();

			#region 重新启动
			if (this.cbRestart.Checked)
			{
				DialogResult result = DialogResult.Retry;
				while (result == DialogResult.Retry)
				{
					result = DialogResult.Abort;
					try
					{
						Process p = new Process();
						p.StartInfo.FileName = Program.ExecutiveName;
						p.StartInfo.Arguments = Program.RestartCmdLine;
						p.StartInfo.UseShellExecute = false;
						p.StartInfo.CreateNoWindow = true;
						p.Start();
					}
					catch (Win32Exception ex)
					{
						result = MessageBox.Show(string.Format("重新启动 {0} 失败。 \"{1}\" {2}。", Program.ProductName, Program.ExecutiveName, ex.Message), this.Text, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
					}
				}
			}
			#endregion

			this.Close();
		}

		private void cleanReports()
		{
			foreach (var pair in this.reports)
			{
				if (!File.Exists(pair.Key.FullName)) continue;
				pair.Value.Close();
				File.SetAttributes(pair.Key.FullName, File.GetAttributes(pair.Key.FullName) & ~FileAttributes.ReadOnly);
				File.Delete(pair.Key.FullName);
			}

			this.reports.Clear();
		}

		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			this.cleanReports();
		}
	}
}

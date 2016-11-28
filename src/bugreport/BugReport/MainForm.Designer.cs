namespace BugReport
{
	partial class MainForm
	{
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows 窗体设计器生成的代码

		/// <summary>
		/// 设计器支持所需的方法 - 不要修改
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.lblInfo = new System.Windows.Forms.Label();
			this.lbl1 = new System.Windows.Forms.Label();
			this.txt1 = new System.Windows.Forms.TextBox();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tpInfo = new System.Windows.Forms.TabPage();
			this.txt2 = new System.Windows.Forms.TextBox();
			this.lbl2 = new System.Windows.Forms.Label();
			this.tpList = new System.Windows.Forms.TabPage();
			this.txtReportContent = new System.Windows.Forms.TextBox();
			this.lbReports = new System.Windows.Forms.ListBox();
			this.btnSend = new System.Windows.Forms.Button();
			this.cbRestart = new System.Windows.Forms.CheckBox();
			this.tabControl.SuspendLayout();
			this.tpInfo.SuspendLayout();
			this.tpList.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblInfo
			// 
			this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblInfo.Location = new System.Drawing.Point(6, 3);
			this.lblInfo.Name = "lblInfo";
			this.lblInfo.Size = new System.Drawing.Size(340, 78);
			this.lblInfo.TabIndex = 0;
			this.lblInfo.Text = "　　在产品组件 {0} 中发生未知错误导致程序崩溃。我们迫切希望得到错误信息以及时修复。\r\n　　对给您带来的不便表示真诚的歉意。";
			// 
			// lbl1
			// 
			this.lbl1.AutoSize = true;
			this.lbl1.Location = new System.Drawing.Point(6, 81);
			this.lbl1.Name = "lbl1";
			this.lbl1.Size = new System.Drawing.Size(116, 17);
			this.lbl1.TabIndex = 1;
			this.lbl1.Text = "错误是如何发生的？";
			// 
			// txt1
			// 
			this.txt1.AcceptsReturn = true;
			this.txt1.AcceptsTab = true;
			this.txt1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txt1.Location = new System.Drawing.Point(9, 101);
			this.txt1.Multiline = true;
			this.txt1.Name = "txt1";
			this.txt1.Size = new System.Drawing.Size(337, 120);
			this.txt1.TabIndex = 2;
			// 
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl.Controls.Add(this.tpInfo);
			this.tabControl.Controls.Add(this.tpList);
			this.tabControl.Location = new System.Drawing.Point(12, 12);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(360, 408);
			this.tabControl.TabIndex = 3;
			// 
			// tpInfo
			// 
			this.tpInfo.Controls.Add(this.txt2);
			this.tpInfo.Controls.Add(this.lbl2);
			this.tpInfo.Controls.Add(this.lblInfo);
			this.tpInfo.Controls.Add(this.txt1);
			this.tpInfo.Controls.Add(this.lbl1);
			this.tpInfo.Location = new System.Drawing.Point(4, 26);
			this.tpInfo.Name = "tpInfo";
			this.tpInfo.Padding = new System.Windows.Forms.Padding(3);
			this.tpInfo.Size = new System.Drawing.Size(352, 378);
			this.tpInfo.TabIndex = 0;
			this.tpInfo.Text = "报告信息";
			this.tpInfo.UseVisualStyleBackColor = true;
			// 
			// txt2
			// 
			this.txt2.AcceptsReturn = true;
			this.txt2.AcceptsTab = true;
			this.txt2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txt2.Location = new System.Drawing.Point(9, 252);
			this.txt2.Multiline = true;
			this.txt2.Name = "txt2";
			this.txt2.Size = new System.Drawing.Size(337, 120);
			this.txt2.TabIndex = 4;
			// 
			// lbl2
			// 
			this.lbl2.AutoSize = true;
			this.lbl2.Location = new System.Drawing.Point(6, 232);
			this.lbl2.Name = "lbl2";
			this.lbl2.Size = new System.Drawing.Size(68, 17);
			this.lbl2.TabIndex = 3;
			this.lbl2.Text = "其他信息：";
			// 
			// tpList
			// 
			this.tpList.Controls.Add(this.txtReportContent);
			this.tpList.Controls.Add(this.lbReports);
			this.tpList.Location = new System.Drawing.Point(4, 26);
			this.tpList.Name = "tpList";
			this.tpList.Padding = new System.Windows.Forms.Padding(3);
			this.tpList.Size = new System.Drawing.Size(352, 378);
			this.tpList.TabIndex = 1;
			this.tpList.Text = "报告列表";
			this.tpList.UseVisualStyleBackColor = true;
			// 
			// txtReportContent
			// 
			this.txtReportContent.Location = new System.Drawing.Point(6, 101);
			this.txtReportContent.Multiline = true;
			this.txtReportContent.Name = "txtReportContent";
			this.txtReportContent.ReadOnly = true;
			this.txtReportContent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtReportContent.Size = new System.Drawing.Size(340, 271);
			this.txtReportContent.TabIndex = 1;
			this.txtReportContent.Text = "双击列表项浏览报告内容。";
			this.txtReportContent.WordWrap = false;
			// 
			// lbReports
			// 
			this.lbReports.FormattingEnabled = true;
			this.lbReports.ItemHeight = 17;
			this.lbReports.Location = new System.Drawing.Point(6, 6);
			this.lbReports.Name = "lbReports";
			this.lbReports.Size = new System.Drawing.Size(340, 89);
			this.lbReports.TabIndex = 0;
			this.lbReports.DoubleClick += new System.EventHandler(this.lbReports_DoubleClick);
			// 
			// btnSend
			// 
			this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSend.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnSend.Location = new System.Drawing.Point(297, 426);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(75, 23);
			this.btnSend.TabIndex = 4;
			this.btnSend.Text = "发送(&S)";
			this.btnSend.UseVisualStyleBackColor = true;
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// cbRestart
			// 
			this.cbRestart.AutoSize = true;
			this.cbRestart.Checked = true;
			this.cbRestart.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbRestart.Location = new System.Drawing.Point(12, 428);
			this.cbRestart.Name = "cbRestart";
			this.cbRestart.Size = new System.Drawing.Size(110, 21);
			this.cbRestart.TabIndex = 5;
			this.cbRestart.Text = "重新启动 {0} 。";
			this.cbRestart.UseVisualStyleBackColor = true;
			// 
			// MainForm
			// 
			this.AcceptButton = this.btnSend;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(384, 461);
			this.Controls.Add(this.cbRestart);
			this.Controls.Add(this.btnSend);
			this.Controls.Add(this.tabControl);
			this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "{0} - 错误报告";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
			this.tabControl.ResumeLayout(false);
			this.tpInfo.ResumeLayout(false);
			this.tpInfo.PerformLayout();
			this.tpList.ResumeLayout(false);
			this.tpList.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblInfo;
		private System.Windows.Forms.Label lbl1;
		private System.Windows.Forms.TextBox txt1;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tpInfo;
		private System.Windows.Forms.TextBox txt2;
		private System.Windows.Forms.Label lbl2;
		private System.Windows.Forms.TabPage tpList;
		private System.Windows.Forms.TextBox txtReportContent;
		private System.Windows.Forms.ListBox lbReports;
		private System.Windows.Forms.Button btnSend;
		private System.Windows.Forms.CheckBox cbRestart;
	}
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WebSocketSharp;

namespace Astchat.Client.Launcher.WPF
{
	partial class MainWindow
	{

		private string currentChannel;
		/// <summary>
		/// 当前频道改变的事件参数。
		/// </summary>
		public class CurrentChannelChangedEventArgs : EventArgs
		{
			/// <summary>
			/// 频道名称。
			/// </summary>
			public string Channel { get; set; }
			public CurrentChannelChangedEventArgs() : base() { }
			public CurrentChannelChangedEventArgs(string channel) : this()
			{
				this.Channel = channel;
			}
		}
		/// <summary>
		/// 当前频道改变后发生。
		/// </summary>
		public event EventHandler<CurrentChannelChangedEventArgs> CurrentChannelChanged = (sender, e) => { };

		private static readonly ClientManager manager = new ClientManager();
		private void connect(string channel)
		{
			channel = channel.Trim().ToLower();
			if (string.IsNullOrEmpty(channel) || this.controlSetDic.ContainsKey(channel)) return;
			
			this.addChannelControls(channel);
			this.svChannelList.ScrollToEnd(); // 添加新频道自动滚动到底部。
			
			WebSocket ws = manager.AddChannel(channel);
			ws.OnOpen += (sender, e) =>
			{
				this.Dispatcher.Invoke(new Action(() => this.controlSetDic[channel].tbMessageRecord.Inlines.Add(new Run(channel + " connected.\r\n\r\n") { Foreground = new SolidColorBrush(Colors.Gray) })));
			};
			ws.OnClose += (sender, e) =>
			{
				this.Dispatcher.Invoke(new Action(() => this.controlSetDic[channel].tbMessageRecord.Inlines.Add(new Run(channel + " disconnected. \r\n\r\n") { Foreground = new SolidColorBrush(Colors.Gray) })));
			};
			ws.OnError += (sender, e) =>
			{
				this.Dispatcher.Invoke(new Action(() =>
					this.controlSetDic[channel].tbMessageRecord.Inlines.Add(new Run(
					string.Format("{0} connect error.\r\n    {1} -> {2}\r\n{3}\r\n\r\n",
					  channel, e.Message, e.Exception.Message, e.Exception.StackTrace)
					)
					{ Foreground = new SolidColorBrush(Colors.Gray) })));
			};
			ws.OnMessage += (sender, e) =>
			{
				string time_short, time_long, message;

				DateTime dt = ClientManager.FormatUTC(manager.ParseTime(e.Data));
				if ((DateTime.Now - dt).TotalDays < 7)
				{
					switch (DateTime.Now.Day - dt.Day)
					{
						case 0:
							time_short = dt.ToString("HH:mm");
							time_long = dt.ToString("HH:mm:ss");
							break;
						case 1:
							time_short = "昨天";
							time_long = time_short + dt.ToString(" HH:mm:ss");
							break;
						default:
							time_short = dt.DayOfWeek.ToString("D");
							time_long = time_short + dt.ToString(" HH:mm:ss");
							break;
					}
				}
				else
				{
					time_short = dt.ToString("yyyy-MM-dd");
					time_long = time_short + dt.ToString(" HH:mm:ss");
				}


				#region 处理message
				this.Dispatcher.BeginInvoke(new Action(() =>
				{
					this.controlSetDic[channel].tbMessageRecord.Inlines.AddRange(new Inline[]
					{
						new Run(time_long) { Foreground = new SolidColorBrush(Colors.Gray), FontStyle = FontStyles.Italic },
						new Run(Environment.NewLine)
					}
					);
				}));


				message = manager.ParsePureText(e.Data);

				// 把新建控件操作放在this.Dispatcher.(Begin)Invoke方法里，可解决多线程的对象访问问题。
				this.Dispatcher.Invoke(new Action(() =>
				{
					string imageUrl; Image image;
					if (manager.TryParseImage(e.Data, out imageUrl))
					{
						//message = string.Format("目前版本不支持图片浏览，请复制以下链接至浏览器地址栏：{1}{0}", imageUrl, Environment.NewLine);

						image = new Image();
						try
						{
							image.Source = new BitmapImage(new Uri(imageUrl));
						}
						catch (UriFormatException) { } // 如果Uri格式不正确就不加载图片源
						this.controlSetDic[channel].tbMessageRecord.Inlines.Add(new InlineUIContainer(image));
					}
					else
					{
						MatchCollection matches = Regex.Matches(message, "\\:(?<EmojiShortName>\\w*)\\:");

						if (matches.Count != 0)
						{
							string emoji_directory = @"https://cdn.jsdelivr.net/emojione/assets/png/";

							int index = 0;
							foreach (Match match in matches)
							{
								if (index != match.Index)
									this.controlSetDic[channel].tbMessageRecord.Inlines.Add(new Run(message.Substring(index, match.Index - index)));

								#region 加载emoji
								var lbl = new Label() { Width = 25, Height = 25 };
								string uri = emoji_directory + EmojiGallery.EmojiDic[match.Groups["EmojiShortName"].Value].unicode + ".png";
								if (true)
									lbl.Background = new ImageBrush(new BitmapImage(new Uri(uri)));
								this.controlSetDic[channel].tbMessageRecord.Inlines.Add(new InlineUIContainer(lbl));
								#endregion

								index = match.Index + match.Length;
							}

							if (index != message.Length)
								this.controlSetDic[channel].tbMessageRecord.Inlines.Add(new Run(message.Substring(index)));
						}
						else
							this.controlSetDic[channel].tbMessageRecord.Inlines.Add(new Run(message));
					}

				}));



				//record = string.Format("{0}{2}{1}{2}{2}", time_long, message, Environment.NewLine);



				this.Dispatcher.BeginInvoke(new Action(() =>
				{
					this.controlSetDic[channel].tbMessageRecord.Inlines.AddRange(new Inline[]
					{
						new Run(Environment.NewLine),
						new Run(Environment.NewLine)
					}
					);
				}));
				#endregion

				this.Dispatcher.BeginInvoke(new Action(() =>
					this.controlSetDic[channel].svMessageRecord.ScrollToEnd() // 显示出新信息自动滚动到底部。
				));

				this.Dispatcher.BeginInvoke(new Action(() =>
				{
					this.controlSetDic[channel].tbLatestMessageContent.Text = message;
					this.controlSetDic[channel].lblLatestMessageTime.Content = time_short;
					
					lock (this.currentChannel)
					{
						if (channel != this.currentChannel)
						{
							lock (this.controlSetDic[channel].brdrNewMessageBubble)
							{
								if (this.controlSetDic[channel].brdrNewMessageBubble.Visibility != Visibility.Visible)
									this.controlSetDic[channel].brdrNewMessageBubble.Visibility = Visibility.Visible; 
							}

							lock (this.controlSetDic[channel].tbNewMessageCount)
							{
								int count = int.Parse(this.controlSetDic[channel].tbNewMessageCount.Text);
								this.controlSetDic[channel].tbNewMessageCount.Text = (count + 1).ToString(); 
							}
						} 
					}
				}));
			};
			ws.Connect();

			RoutedEventHandler send = (sender, e) =>
			{
				manager.SendPureText(channel, this.controlSetDic[channel].txtMessage.Text);
				this.Dispatcher.Invoke(new Action(() => this.controlSetDic[channel].txtMessage.Clear()));
			};

			this.btnSend.Click += send;
			this.controlSetDic[channel].txtMessage.KeyDown += (sender, e) =>
			{
				if (e.Key == Key.Enter)
				{
					if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
						this.Dispatcher.Invoke(new Action(() =>
						{
							TextBox txtMessage = (TextBox)sender;
							string newline = Environment.NewLine;
							txtMessage.SelectedText = newline;
							txtMessage.SelectionLength = 0;
							txtMessage.SelectionStart += newline.Length;
						}));
					else
						send(sender, null);
				}
			};

			this.Closing += (sender, e) =>
			{
				manager.RemoveChannel(channel);
			};

			this.setCurrentChannel(channel);
		}

		private void disconnect(string channel)
		{
			channel = channel.Trim().ToLower();

			if (!this.controlSetDic.ContainsKey(channel)) return;

			manager.RemoveChannel(channel);
			this.removeChannelControls(channel);
		}

		private void setCurrentChannel(string channel)
		{
			this.currentChannel = channel;
			this.CurrentChannelChanged.Invoke(null, new CurrentChannelChangedEventArgs(channel));
		}
	}
}

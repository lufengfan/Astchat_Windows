using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebSocketSharp;

namespace astchat.Client.Launcher.WPF
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			this.InitializeClientControls();

			this.connect();
		}

		/// <summary>
		/// 初始化自定义窗口的虚拟客户栏、边界以及其他与Win32交互的界面响应设置
		/// </summary>
		private void InitializeClientControls()
		{
			this.SourceInitialized += (sender, e) =>
			{
				// 获得供与Win32互操作所需的源
				this._HwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
			};

			#region 重定向虚拟客户栏上的三个按钮的处理程序
			this.Window_Title_Button_Minimize.AddHandler(Button.MouseLeftButtonUpEvent, new RoutedEventHandler(this.Window_Minimize_Confirm), true);
			this.Window_Title_Button_Maximize.AddHandler(Button.MouseLeftButtonUpEvent, new RoutedEventHandler(this.Window_Maximize_Confirm), true);
			this.Window_Title_Button_Close.AddHandler(Button.MouseLeftButtonUpEvent, new RoutedEventHandler(this.Window_Close_Confirm), true);
			#endregion
		}



		private void Window_Minimize_Confirm(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			// 如果鼠标完成左键单击时是处于获得焦点的状态且指针位置处在按钮内部，则最大化或正常化窗口
			Button button = (Button)sender;
			if (button.IsFocused && button.IsMouseOver)
			{
				this.WindowState = WindowState.Minimized;
			}
		}

		private void Window_Maximize_Confirm(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			// 如果鼠标完成左键单击时是处于获得焦点的状态且指针位置处在按钮内部，则最大化或还原窗口，并更换图标
			Button button = (Button)sender;
			if (button.IsFocused && button.IsMouseOver)
				this.WindowState = (this.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
		}

		private void Window_Close_Confirm(object sender, RoutedEventArgs e)
		{
			e.Handled = true; // 表示路由事件已被处理，不再发往下一级

			// 如果鼠标完成左键单击时是处于获得焦点的状态且指针位置处在按钮内部，则关闭窗口
			Button button = (Button)sender;
			if (button.IsFocused && button.IsMouseOver)
				this.Close();
		}



		#region 移动窗口
		/// <summary>
		/// 用户将鼠标放在窗体虚拟标题栏，按下鼠标左键试图拖动整个窗口时发生。由事件 <see cref="UIElement.MouseLeftButtonDown"/> 调用。
		/// </summary>
		/// <param name="sender">事件的发起者</param>
		/// <param name="e">事件的参数</param>
		private void Window_Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (this.WindowState != WindowState.Normal) return;

			this.DragMove();
		}
		#endregion


		#region 改变窗口的尺寸
		private HwndSource _HwndSource;
		private const int WM_SYSCOMMAND = 0x0112;
		private const int HWND_BROADCAST = 0xFFFF;

		/// <summary>
		/// 该函数将指定的消息发送到一个或多个窗口。此函数为指定的窗口调用窗口程序，直到窗口程序处理完消息再返回。
		/// </summary>
		/// <param name="hWnd">其窗口程序将接收消息的窗口的句柄。如果此参数为 <see cref="HWND_BROADCAST"/> ，则消息将被发送到系统中所有顶层窗口，包括无效或不可见的非自身拥有的窗口、被覆盖的窗口和弹出式窗口，但消息不被发送到子窗口。</param>
		/// <param name="Msg">指定被发送的消息</param>
		/// <param name="wParam">指定附加的消息特定信息。</param>
		/// <param name="lParam">指定附加的消息特定信息。</param>
		/// <returns>返回值指定消息处理的结果，依赖于所发送的消息。</returns>
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		/// <summary>
		/// 定义了一系列窗口边界改变的方向。
		/// </summary>
		private enum Window_ResizeBorder_Directions
		{
			/// <summary>左</summary>
			West = 1,
			/// <summary>右</summary>
			East = 2,
			/// <summary>上</summary>
			North = 3,
			/// <summary>下</summary>
			South = 6,
			/// <summary>左上</summary>
			NorthWest = Window_ResizeBorder_Directions.North + Window_ResizeBorder_Directions.West,
			/// <summary>右上</summary>
			NorthEast = Window_ResizeBorder_Directions.North + Window_ResizeBorder_Directions.East,
			/// <summary>左下</summary>
			SouthWest = Window_ResizeBorder_Directions.South + Window_ResizeBorder_Directions.West,
			/// <summary>右下</summary>
			SouthEast = Window_ResizeBorder_Directions.South + Window_ResizeBorder_Directions.East
		}

		private void Window_ResizeBorder_MouseLeftButtonDown(object sender, MouseEventArgs e)
		{
			FrameworkElement element = sender as FrameworkElement;
			Window_ResizeBorder_Directions direction = (Window_ResizeBorder_Directions)Enum.Parse(typeof(Window_ResizeBorder_Directions), element.Name.Replace("Window_ResizeBorderRectangle_", ""));

			if (e.LeftButton == MouseButtonState.Pressed) // 如果鼠标左键为按下状态，则不停向窗体的源发送改变窗体大小消息
				SendMessage(_HwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(0xF000 + direction), IntPtr.Zero);
		}

		private void SetWindowBorderRectanglesVisibility(Visibility visibility)
		{
			if ((int)visibility == -1)
				visibility = this.WindowState == WindowState.Normal ? Visibility.Visible : Visibility.Collapsed;

			this.Window_ResizeBorderRectangle_East.Visibility =
				this.Window_ResizeBorderRectangle_West.Visibility =
				this.Window_ResizeBorderRectangle_North.Visibility =
				this.Window_ResizeBorderRectangle_South.Visibility =
				this.Window_ResizeBorderRectangle_NorthEast.Visibility =
				this.Window_ResizeBorderRectangle_NorthWest.Visibility =
				this.Window_ResizeBorderRectangle_SouthEast.Visibility =
				this.Window_ResizeBorderRectangle_SouthWest.Visibility = visibility;
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (this.WindowState != WindowState.Minimized)
			{
				// 根据当前窗口状态更换按钮显示图标
				this.Window_Title_Button_Maximize.Content = this.Window_Title.Resources[(this.WindowState == WindowState.Normal) ? "Window_Title_Button_Maximize_MaximizeContent" : "Window_Title_Button_Maximize_NormalizeContent"];

				this.SetWindowBorderRectanglesVisibility((this.WindowState == WindowState.Maximized) ? Visibility.Collapsed : Visibility.Visible);
			}
		}
		#endregion


		private static readonly ClientManager manager = new ClientManager();
		private void connect()
		{
			WebSocket ws = manager.ConnectChannel("lobby");
			ws.OnOpen += (sender, e) =>
			 {
				 this.Dispatcher.Invoke(new Action(() => this.tbRecord.Inlines.Add(new Run("lobby connected.\r\n\r\n") { Foreground = new SolidColorBrush(Colors.Gray) })));
			 };
			ws.OnClose += (sender, e) =>
			{
				this.Dispatcher.Invoke(new Action(() => this.tbRecord.Inlines.Add(new Run("lobby disconnected. \r\n\r\n") { Foreground = new SolidColorBrush(Colors.Gray) })));
			};
			ws.OnError += (sender, e) =>
			  {
				  this.Dispatcher.Invoke(new Action(() =>
					  this.tbRecord.Inlines.Add(new Run(
					  string.Format("lobby connect error.\r\n    {0} -> {1}\r\n{2}\r\n\r\n", e.Message, e.Exception.Message, e.Exception.StackTrace)
					  ) { Foreground = new SolidColorBrush(Colors.Gray) })));
			  };
			ws.OnMessage += (sender, e) =>
			{
				string record, time, message;

				DateTime dt = ClientManager.FormatUTC(ClientManager.ParseTime(e.Data));
				if ((DateTime.Now - dt).TotalDays <= 7)
				{
					if (dt.DayOfWeek == DateTime.Now.DayOfWeek)
						time = dt.ToString("hh:mm:ss");
					else
						time = dt.DayOfWeek.ToString("D");
				}
				else
					time = dt.ToString("yyyy-MM-dd hh:mm");



				#region 处理message
				this.Dispatcher.BeginInvoke(new Action(() =>
				{
					this.tbRecord.Inlines.AddRange(new Inline[]
					{
						new Run(time) { Foreground = new SolidColorBrush(Colors.Gray), FontStyle = FontStyles.Italic },
						new Run(Environment.NewLine)
					}
					);
				}));


				message = ClientManager.ParsePureText(e.Data);

				// 把新建控件操作放在this.Dispatcher.(Begin)Invoke方法里，可解决多线程的对象访问问题。
				this.Dispatcher.Invoke(new Action(() =>
				{
					string imageUrl; Image image;
					if (ClientManager.TryParseImage(e.Data, out imageUrl))
					{
						//message = string.Format("目前版本不支持图片浏览，请复制以下链接至浏览器地址栏：{1}{0}", imageUrl, Environment.NewLine);

						image = new Image();
						try
						{
							image.Source = new BitmapImage(new Uri(imageUrl));
						}
						catch (UriFormatException) { } // 如果Uri格式不正确就不加载图片源
						this.tbRecord.Inlines.Add(new InlineUIContainer(image));
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
									this.tbRecord.Inlines.Add(new Run(message.Substring(index, match.Index - index - 1)));

								#region 加载emoji
								var lbl = new Label() { Width = 25, Height = 25 };
								string uri = emoji_directory + EmojiGallery.EmojiDic[match.Groups["EmojiShortName"].Value].unicode + ".png";
								if (true)
									lbl.Background = new ImageBrush(new BitmapImage(new Uri(uri)));
								this.tbRecord.Inlines.Add(new InlineUIContainer(lbl));
								#endregion

								index = match.Index + match.Length;
							}

							if (index != message.Length)
								this.tbRecord.Inlines.Add(new Run(message.Substring(index)));
						}
						else
							this.tbRecord.Inlines.Add(new Run(message));
					}

				}));



				record = string.Format("{0}{2}{1}{2}{2}", time, message, Environment.NewLine);



				this.Dispatcher.BeginInvoke(new Action(() =>
				{
					this.tbRecord.Inlines.AddRange(new Inline[]
					{
						new Run(Environment.NewLine),
						new Run(Environment.NewLine)
					}
					);
				}));
				#endregion
			};
			ws.Connect();

			RoutedEventHandler send = (sender, e) =>
			{
				ClientManager.SendPureText(ws, this.txtMessage.Text);
				this.Dispatcher.Invoke(new Action(() => this.txtMessage.Clear()));
			};

			this.btnSend.Click += send;
			this.txtMessage.KeyDown += (sender, e) =>
			  {
				  if (e.Key == Key.Enter)
				  {
					  if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
						  this.Dispatcher.Invoke(new Action(() =>
						  {
							  string newline = Environment.NewLine;
							  this.txtMessage.SelectedText = newline;
							  this.txtMessage.SelectionLength = 0;
							  this.txtMessage.SelectionStart += newline.Length;
							  //int selectionStart = this.txtMessage.SelectionStart;
							  //int messageLength = this.txtMessage.Text.Length;
							  //this.txtMessage.Text += Environment.NewLine;
							  //
							  //if (selectionStart == messageLength)
							  //	selectionStart += 2;
							  //
							  //this.txtMessage.SelectionStart = selectionStart;
						  }));
					  else
						  send(sender, null);
				  }
			  };

			this.Closing += (sender, e) =>
			  {
				  manager.DisconnetChannel("lobby");
			  };
		}

		private void gridEmojiGallery_Loaded(object sender, RoutedEventArgs e)
		{
			string emoji_directory = @"https://cdn.jsdelivr.net/emojione/assets/png/";

			var gs = from ei in EmojiGallery.EmojiDic.Values
					 group ei by ei.category
			into g
					 select new
					 {
						 Category = g.Key,
						 Emojis = g
					 };

			Grid gridEmojiCategory = new Grid();

			List<ScrollViewer> sv_list = new List<ScrollViewer>();
			int index = 0;
			foreach (var g in gs)
			{
				string category = g.Category;
				gridEmojiCategory.ColumnDefinitions.Add(new ColumnDefinition());

				Label lblCategory = new Label();
				lblCategory.SetValue(Grid.ColumnProperty, index);
				lblCategory.Height = lblCategory.Width = 25;
				string uri = emoji_directory + g.Emojis.First().unicode + ".png";
				if (true)
					lblCategory.Background = new ImageBrush(new BitmapImage(new Uri(uri)));
				lblCategory.ToolTip = new ToolTip() { Content = g.Category };
				lblCategory.MouseEnter += (_sender, _e) =>
				 {
					 foreach (var item in sv_list)
					 {
						 if (item.Name == "svEmoji_" + g.Category)
						 {
							 item.Visibility = Visibility.Visible;
						 }
						 else
						 {
							 item.Visibility = Visibility.Collapsed;
						 }
					 }
				 };

				Grid gridEmoji = new Grid();

				int row, column;
				column = (int)this.popupEmojiGallery.Width / 25;
				row = g.Emojis.Count() / column + 1;
				for (int i = 0; i < row; i++) gridEmoji.RowDefinitions.Add(new RowDefinition());
				for (int i = 0; i < column; i++) gridEmoji.ColumnDefinitions.Add(new ColumnDefinition());

				int _index = 0;
				foreach (var ei in g.Emojis)
				{
					Label lblEmoji = new Label();
					lblEmoji.SetValue(Grid.RowProperty, (_index - _index % column) / column);
					lblEmoji.SetValue(Grid.ColumnProperty, _index % column);
					lblEmoji.Height = lblEmoji.Width = 25;
					string _uri = emoji_directory + ei.unicode + ".png";
					if (true)
						lblEmoji.Background = new ImageBrush(new BitmapImage(new Uri(_uri)));
					lblEmoji.ToolTip = new ToolTip() { Content = ei.name };
					lblEmoji.MouseLeftButtonUp += (_sender, _e) =>
					{
						string insertStr = ei.shortname;
						this.txtMessage.SelectedText = insertStr;
						this.txtMessage.SelectionLength = 0;
						this.txtMessage.SelectionStart += insertStr.Length;
					};

					gridEmoji.Children.Add(lblEmoji);
					_index++;
				}

				ScrollViewer _sv = new ScrollViewer();
				_sv.SetValue(Grid.RowProperty, 0);
				_sv.Name = "svEmoji_" + g.Category;
				_sv.Content = gridEmoji;
				_sv.Visibility = Visibility.Collapsed;
				gridEmoji.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
				sv_list.Add(_sv);

				gridEmojiCategory.Children.Add(lblCategory);
				this.gridEmojiGallery.Children.Add(_sv);
				index++;
			}

			ScrollViewer sv = new ScrollViewer();
			sv.SetValue(Grid.RowProperty, 1);
			sv.Content = gridEmojiCategory;
			gridEmojiCategory.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Visible);
			gridEmojiCategory.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
			this.gridEmojiGallery.Children.Add(sv);
		}

		private void lblInsertEmoji_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			this.popupEmojiGallery.IsOpen = true;
		}

		private void gridImageUrl_Loaded(object sender, RoutedEventArgs e)
		{
			this.txtImageUrl.KeyUp += (_sender, _e) =>
			  {
				  if (_e.Key == Key.Enter)
				  {
					  if (this.txtImageUrl.Text != string.Empty)
						  ClientManager.SendImage(manager.ConnectChannel("lobby"), this.txtImageUrl.Text);
					  this.txtImageUrl.Clear();
					  this.popupSendImage.IsOpen = false;
				  }
			  };
		}

		private void lblSendImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			this.popupSendImage.IsOpen = true;
		}
	}
}

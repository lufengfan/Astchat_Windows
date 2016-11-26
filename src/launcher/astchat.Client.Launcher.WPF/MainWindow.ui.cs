using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Astchat.Client.Launcher.WPF
{
	partial class MainWindow
	{

		/// <summary>
		/// 初始化自定义窗口的虚拟客户栏、边界以及其他与Win32交互的界面响应设置
		/// </summary>
		private void InitializeClientControls()
		{
			this.CurrentChannelChanged += (sender, e) =>
			{
				foreach (var pair in this.controlSetDic)
				{
					var set = pair.Value;
					if (pair.Key == this.currentChannel)
					{
						this.lblWinTitle.Content = string.Format("{0} - Astchat", pair.Key);
						set.gChannel.Background = new SolidColorBrush(Color.FromArgb(48, 0, 0, 0));
						set.tbLatestMessageContent.Foreground = new SolidColorBrush(Colors.Black);
						set.lblLatestMessageTime.Foreground = new SolidColorBrush(Colors.Black);
						set.brdrNewMessageBubble.Visibility = Visibility.Hidden;
						set.tbNewMessageCount.Text = "0";
						set.svMessageRecord.Visibility = Visibility.Visible;
						set.svMessage.Visibility = Visibility.Visible;
					}
					else
					{
						set.gChannel.Background = new SolidColorBrush(Color.FromArgb(16, 0, 0, 0));
						set.tbLatestMessageContent.Foreground = new SolidColorBrush(Colors.Gray);
						set.lblLatestMessageTime.Foreground = new SolidColorBrush(Colors.Gray);
						set.svMessageRecord.Visibility = Visibility.Hidden;
						set.svMessage.Visibility = Visibility.Hidden;
					}
				}
			};
			
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
			if (this.WindowState == WindowState.Minimized) return;

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
		/// <param name="hWnd">其窗口程序将接收消息的窗口的句柄。如果此参数为<see cref="HWND_BROADCAST"/>，则消息将被发送到系统中所有顶层窗口，包括无效或不可见的非自身拥有的窗口、被覆盖的窗口和弹出式窗口，但消息不被发送到子窗口。</param>
		/// <param name="message">指定被发送的消息</param>
		/// <param name="wParam">指定附加的消息特定信息。</param>
		/// <param name="lParam">指定附加的消息特定信息。</param>
		/// <returns>返回值指定消息处理的结果，依赖于所发送的消息。</returns>
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam);

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

				// this.SetWindowBorderRectanglesVisibility((this.WindowState == WindowState.Maximized) ? Visibility.Collapsed : Visibility.Visible);
			}
		}
		#endregion













		private Dictionary<string, BindingControlSet> controlSetDic = new Dictionary<string, BindingControlSet>();
		private void addChannelControls(string channel)
		{
			BindingControlSet set = new BindingControlSet();
			this.addChannelItem(channel, ref set);
			this.addMessageProcesser(channel, ref set);

			this.controlSetDic.Add(channel, set);
		}

		private void addChannelItem(string channel, ref BindingControlSet set)
		{
			#region gChannel
			set.gChannel = new Grid();
			this.spChannelList.Children.Add(set.gChannel);
			set.gChannel.Background = new SolidColorBrush(Color.FromArgb(16, 0, 0, 0));

			#region grid1
			Grid grid1 = new Grid();
			set.gChannel.Children.Add(grid1);
			grid1.Margin = new Thickness(15);
			grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
			grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(10) });
			grid1.ColumnDefinitions.Add(new ColumnDefinition());
			grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });

			#region imgChannelIcon
			set.imgChannelIcon = new Image();
			grid1.Children.Add(set.imgChannelIcon);
			set.imgChannelIcon.Height = 35;
			set.imgChannelIcon.Width = 35;
			set.imgChannelIcon.Source = new BitmapImage(new Uri(@"/images/channel_default_icon.png", UriKind.Relative));
			set.imgChannelIcon.Stretch = Stretch.Fill;
			#endregion

			#region grid2
			Grid grid2 = new Grid();
			grid1.Children.Add(grid2);
			grid2.SetValue(Grid.ColumnProperty, 2);
			grid2.VerticalAlignment = VerticalAlignment.Center;
			grid2.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
			grid2.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(2) });
			grid2.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });

			#region tbChannelName
			set.tbChannelName = new TextBlock();
			grid2.Children.Add(set.tbChannelName);
			set.tbChannelName.SetValue(Grid.RowProperty, 0);
			set.tbChannelName.Text = channel;
			#endregion

			#region tbLatestMessageContent
			set.tbLatestMessageContent = new TextBlock();
			grid2.Children.Add(set.tbLatestMessageContent);
			set.tbLatestMessageContent.SetValue(Grid.RowProperty, 2);
			set.tbLatestMessageContent.Foreground = new SolidColorBrush(Colors.Gray);
			#endregion
			#endregion

			#region grid3
			Grid grid3 = new Grid();
			grid1.Children.Add(grid3);
			grid3.SetValue(Grid.ColumnProperty, 3);
			grid3.RowDefinitions.Add(new RowDefinition());
			grid3.RowDefinitions.Add(new RowDefinition());

			#region lblLatestMessageTime
			set.lblLatestMessageTime = new Label();
			grid3.Children.Add(set.lblLatestMessageTime);
			set.lblLatestMessageTime.SetValue(Grid.RowProperty, 0);
			set.lblLatestMessageTime.Foreground = new SolidColorBrush(Colors.Gray);
			set.lblLatestMessageTime.HorizontalAlignment = HorizontalAlignment.Center;
			set.lblLatestMessageTime.VerticalAlignment = VerticalAlignment.Center;
			set.lblLatestMessageTime.HorizontalContentAlignment = HorizontalAlignment.Center;
			set.lblLatestMessageTime.VerticalAlignment = VerticalAlignment.Center;
			//set.lblLatestMessageTime.Content = "20:45";
			#endregion

			#region brdrNewMessageBubble
			set.brdrNewMessageBubble = new Border();
			grid3.Children.Add(set.brdrNewMessageBubble);
			set.brdrNewMessageBubble.SetValue(Grid.RowProperty, 1);
			set.brdrNewMessageBubble.Background = new SolidColorBrush(Colors.Red);
			set.brdrNewMessageBubble.Margin = new Thickness(11, 0, 11, 0);
			set.brdrNewMessageBubble.Padding = new Thickness(1);
			set.brdrNewMessageBubble.BorderBrush = new SolidColorBrush(Colors.Red);
			set.brdrNewMessageBubble.BorderThickness = new Thickness(1);
			set.brdrNewMessageBubble.CornerRadius = new CornerRadius(10);
			set.brdrNewMessageBubble.Visibility = Visibility.Collapsed;

			#region tbNewMessageCount
			set.tbNewMessageCount = new TextBlock();
			set.brdrNewMessageBubble.Child = set.tbNewMessageCount;
			set.tbNewMessageCount.Foreground = new SolidColorBrush(Colors.White);
			set.tbNewMessageCount.HorizontalAlignment = HorizontalAlignment.Center;
			set.tbNewMessageCount.VerticalAlignment = VerticalAlignment.Center;
			set.tbNewMessageCount.Text = "0";
			#endregion
			#endregion
			#endregion
			#endregion


			Label lblHotArea = new Label();
			set.gChannel.Children.Add(lblHotArea);
			lblHotArea.MouseLeftButtonUp += (sender, e) =>
			{
				// 引发CurrentChannelChange事件。
				this.setCurrentChannel(channel);
			};

			#endregion
		}

		private void addMessageProcesser(string channel, ref BindingControlSet set)
		{
			#region svMessageRecord
			set.svMessageRecord = new ScrollViewer();
			this.gMessageRecord.Children.Add(set.svMessageRecord);
			set.svMessageRecord.Visibility = Visibility.Hidden;

			#region tbRecord
			set.tbMessageRecord = new TextBlock();
			set.svMessageRecord.Content = set.tbMessageRecord;
			set.tbMessageRecord.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
			set.tbMessageRecord.Margin = new Thickness(15, 0, 15, 0);
			set.tbMessageRecord.TextWrapping = TextWrapping.Wrap;
			#endregion
			#endregion

			#region svMessage
			set.svMessage = new ScrollViewer();
			this.gMessage.Children.Add(set.svMessage);
			set.svMessage.SetValue(Grid.RowProperty, 1);
			set.svMessage.Visibility = Visibility.Hidden;

			#region tbMessage
			set.txtMessage = new TextBox();
			set.svMessage.Content = set.txtMessage;
			set.txtMessage.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
			set.txtMessage.AcceptsTab = true;
			set.txtMessage.Padding = new Thickness(15, 0, 15, 0);
			set.tbMessageRecord.TextWrapping = TextWrapping.Wrap;
			#endregion
			#endregion
		}

		class BindingControlSet
		{
			public Grid gChannel { get; set; }
			public Image imgChannelIcon { get; set; }
			public TextBlock tbChannelName { get; set; }
			public TextBlock tbLatestMessageContent { get; set; }
			public Label lblLatestMessageTime { get; set; }
			public Border brdrNewMessageBubble { get; set; }
			public TextBlock tbNewMessageCount { get; set; }
			public ScrollViewer svMessageRecord { get; set; }
			public TextBlock tbMessageRecord { get; set; }
			public ScrollViewer svMessage { get; set; }
			public TextBox txtMessage { get; set; }
		}
	}
}

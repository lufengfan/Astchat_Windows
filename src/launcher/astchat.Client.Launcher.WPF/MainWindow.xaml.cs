using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Astchat.Client.Launcher.WPF
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

			this.connect("lobby");

			#region 附属启动接口
			string sse_dir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SatelliteSelfExecutive");
			if (Directory.Exists(sse_dir))
			{
				foreach (string executive in Directory.GetFiles(sse_dir))
				{
					if (System.IO.Path.GetExtension(executive) == ".exe")
					{
						Process p = new Process();
						p.StartInfo.FileName = executive;
						p.StartInfo.RedirectStandardOutput = true;
						p.StartInfo.UseShellExecute = false;
						p.StartInfo.CreateNoWindow = true;

						p.Start();
					}
				}
			}
			#endregion
		}

		private bool isGridEmojiGalleryLoaded = false;
		private void gridEmojiGallery_Loaded(object sender, RoutedEventArgs e)
		{
			if (isGridEmojiGalleryLoaded) return;
			
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
				gridEmojiCategory.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);

				Image imgCategory = new Image();
				imgCategory.SetValue(Grid.ColumnProperty, index);
				imgCategory.Height = imgCategory.Width = 25;
				if (true)
					imgCategory.Source = new BitmapImage(new Uri(EmojiGallery.GetEmojiUri(g.Emojis.First().unicode)));
				imgCategory.ToolTip = new ToolTip() { Content = g.Category };
				imgCategory.MouseEnter += (_sender, _e) =>
				 {
					 if (!sv_list.Any(_sv => _sv.Name == "svEmoji_" + g.Category))
					 {

						 Grid gridEmoji = new Grid();

						 int row, column;
						 column = (int)this.popupEmojiGallery.Width / 25;
						 row = g.Emojis.Count() / column + 1;
						 for (int i = 0; i < row; i++) gridEmoji.RowDefinitions.Add(new RowDefinition());
						 for (int i = 0; i < column; i++) gridEmoji.ColumnDefinitions.Add(new ColumnDefinition());

						 int _index = 0;
						 foreach (var ei in g.Emojis)
						 {
							 Image imgEmoji = new Image();
							 imgEmoji.SetValue(Grid.RowProperty, (_index - _index % column) / column);
							 imgEmoji.SetValue(Grid.ColumnProperty, _index % column);
							 imgEmoji.Stretch = Stretch.Fill;
							 imgEmoji.Height = imgEmoji.Width = 25;
							 if (true)
								 imgEmoji.Source = new BitmapImage(new Uri(EmojiGallery.GetEmojiUri(ei.unicode)));
							 imgEmoji.ToolTip = new ToolTip() { Content = ei.name };
							 imgEmoji.MouseLeftButtonUp += (__sender, __e) =>
							 {
								 string insertStr = ei.shortname;
								 this.controlSetDic[currentChannel].txtMessage.SelectedText = insertStr;
								 this.controlSetDic[currentChannel].txtMessage.SelectionLength = 0;
								 this.controlSetDic[currentChannel].txtMessage.SelectionStart += insertStr.Length;
							 };

							 gridEmoji.Children.Add(imgEmoji);
							 _index++;
						 }

						 ScrollViewer _sv = new ScrollViewer();
						 _sv.SetValue(Grid.RowProperty, 0);
						 _sv.Name = "svEmoji_" + g.Category;
						 _sv.Content = gridEmoji;
						 _sv.Visibility = Visibility.Collapsed;
						 gridEmoji.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
						 sv_list.Add(_sv);

						 this.gridEmojiGallery.Children.Add(_sv);
					 }

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
				
				gridEmojiCategory.Children.Add(imgCategory);
				index++;

				GC.Collect();
			}

			ScrollViewer sv = new ScrollViewer();
			sv.SetValue(Grid.RowProperty, 1);
			sv.Content = gridEmojiCategory;
			this.gridEmojiGallery.Children.Add(sv);

			this.isGridEmojiGalleryLoaded = true;
		}

		private void lblInsertEmoji_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (this.popupEmojiGallery.IsOpen == true) return;

			this.popupEmojiGallery.IsOpen = true;
			//try {
				this.gridEmojiGallery_Loaded(this.gridEmojiGallery, new RoutedEventArgs());
			//}
			//catch (OutOfMemoryException _e)
			//{
			//	MessageBox.Show(_e.Message);
			//}
		}

		private void gridImageUrl_Loaded(object sender, RoutedEventArgs e)
		{
			this.txtImageUrl.KeyUp += (_sender, _e) =>
			  {
				  if (_e.Key == Key.Enter)
				  {
					  if (this.txtImageUrl.Text != string.Empty)
						  manager.SendImage(currentChannel, this.txtImageUrl.Text);
					  this.txtImageUrl.Clear();
					  this.popupSendImage.IsOpen = false;
				  }
			  };
		}

		private void lblSendImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			this.popupSendImage.IsOpen = true;
		}

		private void btnAddChannel_Click(object sender, RoutedEventArgs e)
		{
			this.connect(this.txtChannel.Text);
			this.txtChannel.Clear();
		}

		private void txtChannel_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				btnAddChannel_Click(sender, new RoutedEventArgs());
		}

		private void txtChannel_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(this.txtChannel.Text))
			{
				this.btnAddChannel.Content = this.btnAddChannel.Resources["lblAddChannel_Unabled"];
			}
			else
			{
				this.btnAddChannel.Content = this.btnAddChannel.Resources["lblAddChannel_Gray"];
			}
		}

		private void btnAddChannel_Loaded(object sender, RoutedEventArgs e)
		{
			Button btn = (Button)sender;
			btn.Content = btn.Resources["lblAddChannel_Unabled"];
		}
	}
}

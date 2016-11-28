using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Xml;

namespace Astchat.Client.Launcher.WPF
{
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App : Application
	{
		private bool handled = false;
		private object syncObj = new object();

		public App()
		{
			this.DispatcherUnhandledException += this.App_DispatcherUnhandledException;
		}

		private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			lock (this.syncObj)
			{
				if (this.handled)
				{
					e.Handled = true;
					return;
				}
				
				XmlDocument doc = new XmlDocument();
				doc.AppendChild(doc.CreateDocumentFragment());
				XmlElement bugreport = doc.CreateElement("BugReport");
				doc.AppendChild(bugreport);

				#region environment
				XmlElement environment = doc.CreateElement("Environment");
				bugreport.AppendChild(environment);

				XmlElement osversion = doc.CreateElement("OSVersion");
				environment.AppendChild(osversion);
				this.SerializeObject(doc, osversion, Environment.OSVersion);

				#region currentdirectory
				XmlElement currentdirectory = doc.CreateElement("CurrentDirectory");
				environment.AppendChild(currentdirectory);
				currentdirectory.AppendChild(doc.CreateTextNode(Environment.CurrentDirectory));
				#endregion

				#region basedirectory
				XmlElement basedirectory = doc.CreateElement("BaseDirectory");
				environment.AppendChild(basedirectory);
				basedirectory.AppendChild(doc.CreateTextNode(AppDomain.CurrentDomain.BaseDirectory));
				#endregion

				#region version
				XmlElement version = doc.CreateElement("Version");
				environment.AppendChild(version);
				version.AppendChild(doc.CreateTextNode(Environment.Version.ToString()));
				#endregion

				#region machinename
				XmlElement machinename = doc.CreateElement("MachineVersion");
				environment.AppendChild(machinename);
				machinename.AppendChild(doc.CreateTextNode(Environment.MachineName));
				#endregion

				#region processorcount
				XmlElement processorcount = doc.CreateElement("ProcessorCount");
				environment.AppendChild(processorcount);
				processorcount.AppendChild(doc.CreateTextNode(Environment.ProcessorCount.ToString()));
				#endregion

				#region commandline
				XmlElement commandline = doc.CreateElement("CommandLine");
				environment.AppendChild(commandline);
				commandline.AppendChild(doc.CreateTextNode(Environment.CommandLine));
				#endregion

				#region workingset
				XmlElement workingset = doc.CreateElement("WorkingSet");
				environment.AppendChild(workingset);
				workingset.AppendChild(doc.CreateTextNode(Environment.WorkingSet.ToString()));
				#endregion
				#endregion

				XmlElement exception = doc.CreateElement("Exception");
				bugreport.AppendChild(exception);
				exception.SetAttribute("type", e.Exception.GetType().FullName);





				this.SerializeObject(doc, exception, e.Exception);
				
				string bugreportfile = "__bugreport_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
				doc.Save(bugreportfile);
				File.SetAttributes(bugreportfile, File.GetAttributes(bugreportfile) | (FileAttributes.Hidden | FileAttributes.ReadOnly));
				//MessageBox.Show(sb.ToString());

#if DEBUG
				Clipboard.SetText(File.ReadAllText(bugreportfile));
#endif

				Process p = new Process();
				p.StartInfo.FileName = "BugReport.exe";
				p.StartInfo.Arguments = string.Format("Astchat Astchat.Client.Launcher.WPF \"{0}\" \"\" \"{1}\"", Process.GetCurrentProcess().MainModule.FileName, bugreportfile);
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.CreateNoWindow = true;
				p.Start();

				this.handled = e.Handled = true;
				Application.Current.Shutdown(-1); 
			}
		}

		private void SerializeObject(XmlDocument doc, XmlElement element, object o, int stack = 0)
		{
			if (stack == 8) return;

			try
			{
				if (o == null)
				{
					element.AppendChild(doc.CreateElement("null"));
				}
				else if (
					o is sbyte ||
					o is byte ||
					o is int ||
					o is uint ||
					o is long ||
					o is ulong ||
					o is float ||
					o is double ||
					o is decimal ||
					o is string
				)
				{
					element.AppendChild(doc.CreateTextNode(o.ToString()));
				}
				else if (o is Enum)
				{
					XmlElement enum_element = doc.CreateElement(o.GetType().FullName);
					element.AppendChild(enum_element);
					enum_element.AppendChild(doc.CreateTextNode(o.ToString()));
				}
				else if (o is IDictionary)
				{
					foreach (DictionaryEntry item in (IDictionary)o)
					{
						XmlElement keyvaluepair_element = doc.CreateElement("DictionaryEntry");
						element.AppendChild(keyvaluepair_element);

						XmlElement key_element = doc.CreateElement("Key");
						keyvaluepair_element.AppendChild(key_element);
						this.SerializeObject(doc, key_element, item.Key, stack + 1);

						XmlElement value_element = doc.CreateElement("Value");
						keyvaluepair_element.AppendChild(value_element);
						this.SerializeObject(doc, value_element, item.Value, stack + 1);
					}
				}
				else if (o is IEnumerable)
				{
					foreach (object item in (IEnumerable)o)
					{
						XmlElement item_element = doc.CreateElement("Item");
						element.AppendChild(item_element);
						this.SerializeObject(doc, item_element, o, stack + 1);
					}
				}
				else
				{
					try
					{
						Type t = o.GetType();
						PropertyInfo[] properties = t.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
						foreach (PropertyInfo property in properties)
						{
							if (o is Exception && property.Name == "TargetSite") continue;
							
							XmlElement property_element = doc.CreateElement(property.Name);
							element.AppendChild(property_element);
							this.SerializeObject(doc, property_element, property.GetValue(o, null), stack + 1);
						}
					}
					catch (Exception)
					{
					}
				}
			}
			catch (StackOverflowException)
			{
			}
		}

		private void SerializeException(XmlDocument doc, Exception e)
		{

		}
	}
}

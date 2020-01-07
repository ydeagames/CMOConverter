using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.Graphics.Api;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;

namespace CMOConverter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            await new MsBuilder()
            {
                Logger = new TextBoxLogger()
                {
                    TextBox = logText
                }
            }.Execute();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            //var task = new MeshContentTask();
            //task.Source = new TaskIt "E:\\softdata\\git\\CMOConverter\\MakeCMO\\star.FBX";
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            const string projectFileName = "../../../MakeCMO/MakeCMO.vcxproj";
            var parameter = new BuildParameters
            {
                Loggers = new List<ILogger>
                {
                    new TextBoxLogger()
                    {
                        TextBox = logText
                    }
                }
            };
            var proj = new ProjectInstance(projectFileName);
            var item = proj.AddItem("MeshContentTask", Path.GetFullPath("CoinOld.FBX"));
            item.SetMetadata("ContentOutput", Path.GetFullPath("CoinOld.cmo"));
            proj.SetProperty("Configuration", "Release");
            proj.SetProperty("Platform", "Win32");
            BuildManager.DefaultBuildManager.Build(parameter, new BuildRequestData(proj, new string[] { "_MeshContentTask" }));
        }
    }

    public class TextBoxLogger : ConsoleLogger
    {
        public TextBox TextBox;

        public TextBoxLogger() : base(LoggerVerbosity.Normal)
        {
            this.WriteHandler = new WriteHandler(Write);
        }

        public void Write(string text)
        {
            TextBox.Invoke((MethodInvoker)delegate () { TextBox.Text += text; });
        }

        public void WriteLine(string text)
        {
            Write(text + Environment.NewLine);
        }
    }
}

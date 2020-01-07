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
            openFileDialog1.ShowDialog(this);
            var files = openFileDialog1.FileNames;
            await new MsBuilder()
            {
                Logger = new TextBoxLogger()
                {
                    TextBox = logText
                },
                Inputs = files
            }.Execute();

        }

        private async void TabPage1_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            await new MsBuilder()
            {
                Logger = new TextBoxLogger()
                {
                    TextBox = logText
                },
                Inputs = files
            }.Execute();
        }

        private void TabPage1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
            tabPage1.BorderStyle = BorderStyle.FixedSingle;
            tabPage1.BackColor = Color.FromArgb(230, 247, 247);
        }

        private void TabPage1_DragLeave(object sender, EventArgs e)
        {
            tabPage1.BorderStyle = BorderStyle.None;
            tabPage1.BackColor = Color.White;
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

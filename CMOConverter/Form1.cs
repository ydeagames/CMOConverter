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

namespace CMOConverter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            new MsBuilder() { Logger = new TextboxWriter() { TextBox = logText } }.Execute();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            //var task = new MeshContentTask();
            //task.Source = new TaskIt "E:\\softdata\\git\\CMOConverter\\MakeCMO\\star.FBX";
        }
    }

    class TextboxWriter : TextWriter
    {
        public TextBox TextBox;

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            TextBox.Invoke((MethodInvoker) delegate () { TextBox.Text += value; });
        }
    }
}

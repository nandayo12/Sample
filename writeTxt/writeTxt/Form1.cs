using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace writeTxt
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = "\\test.txt";//file Loc: *start->file explorer->text.txt*
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine("Hello");
                    sw.WriteLine("And");
                    sw.WriteLine("Welcome");
                }
            }
            using (StreamReader sr = File.OpenText(path))
            {
                string s = "";
                label1.Text = "";
                while ((s = sr.ReadLine()) != null)
                {
                    label1.Text += s;
                }
            }
        }
    }
}
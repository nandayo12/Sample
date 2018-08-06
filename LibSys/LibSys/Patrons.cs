using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.WindowsCE.Forms;
using System.IO;

namespace LibSys
{
    public partial class Patrons : Form
    {
        #region DLL
        [DllImport("scanapiax.dll", EntryPoint = "API_Register")]
        public static extern bool API_Register(IntPtr hwnd);
        [DllImport("scanapiax.dll", EntryPoint = "API_Unregister")]
        public static extern void API_Unregister();
        [DllImport("scanapiax.dll", EntryPoint = "API_GetBarDataLength")]
        public static extern uint API_GetBarDataLength();
        [DllImport("scanapiax.dll", EntryPoint = "API_GetBarData")]
        public static extern uint API_GetBarData(byte[] buffer, ref uint uiLength, ref uint uiBarType);
        [DllImport("scanapiax.dll", EntryPoint = "API_GetBarType")]
        public static extern uint API_GetBarType();
        [DllImport("scanapiax.dll", EntryPoint = "API_ResetBarData")]
        public static extern void API_ResetBarData();
        [DllImport("scanapiax.dll", EntryPoint = "API_GoodRead")]
        public static extern void API_GoodRead();
        [DllImport("scanapiax.dll", EntryPoint = "API_GetError")]
        public static extern uint API_GetError();
        [DllImport("scanapiax.dll", EntryPoint = "API_GetSysError")]
        public static extern uint API_GetSysError();
        [DllImport("scanapiax.dll", EntryPoint = "API_Reset")]
        public static extern bool API_Reset();
        [DllImport("scanapiax.dll", EntryPoint = "GetLibraryVersion")]
        public static extern int GetLibraryVersion();
        [DllImport("scanapiax.dll", EntryPoint = "S2K_IsLoad")]
        public static extern bool S2K_IsLoad();
        [DllImport("scanapiax.dll", EntryPoint = "S2K_Load")]
        public static extern bool S2K_Load(bool bLoad, int dwTimeOut);
        [DllImport("scanapiax.dll", EntryPoint = "SCAN_SendCommand")]
        public static extern bool SCAN_SendCommand(int nCommand1, int nCommand2, byte[] pValue);
        [DllImport("scanapiax.dll", EntryPoint = "SCAN_QueryStatus")]
        public static extern bool SCAN_QueryStatus(int nCommand1, int nCommand2, byte[] pReturn);

        [DllImport("scanapiax.dll", EntryPoint = "GetTriggerKeyStatus")]
        public static extern int GetTriggerKeyStatus();
        [DllImport("scanapiax.dll", EntryPoint = "EnableTriggerKey")]
        public static extern int EnableTriggerKey(bool bEnable);
        [DllImport("scanapiax.dll", EntryPoint = "TriggerStatus")]
        public static extern int TriggerStatus();
        [DllImport("scanapiax.dll", EntryPoint = "PressTriggerKey")]
        public static extern int PressTriggerKey(bool bPress);

        [DllImport("scanapiax.dll", EntryPoint = "API_SaveSettingsToScanner")]
        public static extern bool API_SaveSettingsToScanner();
        [DllImport("scanapiax.dll", EntryPoint = "API_SaveSettingsToFile")]
        public static extern bool API_SaveSettingsToFile(string filename);
        [DllImport("scanapiax.dll", EntryPoint = "API_LoadSettingsFromFile")]
        public static extern bool API_LoadSettingsFromFile(string filename);
        #endregion
        public Patrons()
        {
            InitializeComponent();
            this.MsgWin = new MsgWindow(this);
    
        }
        MsgWindow MsgWin;
        public const int SM_DATA_READY = 0x8000 + 1;
        public class MsgWindow : MessageWindow
        {
            // Assign integers to messages.
            // Note that custom Window messages start at WM_APP = 0x8000.
            public const int SM_DATA_READY = 0x8000 + 1;
            public const int SM_ERROR_SYSTEM = 0x8000 + 2;
            public const int SM_ERROR_API = 0x8000 + 3;

            // Create an instance of the form.
            private Patrons msgform;

            // Save a reference to the form so it can
            // be notified when messages are received.
            public MsgWindow(Patrons msgform)
            {
                this.msgform = msgform;
            }

            // Override the default WndProc behavior to examine messages.
            protected override void WndProc(ref Message msg)
            {
                string strTemp;
                switch (msg.Msg)
                {
                    // If message is of interest, invoke the method on the form that
                    // functions as a callback to perform actions in response to the message.
                    case SM_DATA_READY:
                        uint uiLength;
                        uint uiSize;
                        uint uiType = 0;

                        uiLength = API_GetBarDataLength();
                        if (uiLength != 0)
                        {
                            uiSize = uiLength + 1;
                            byte[] pBuffer = new byte[uiSize];
                            API_GetBarData(pBuffer, ref uiSize, ref uiType);

                            strTemp = System.Text.Encoding.Default.GetString(pBuffer, 0, (int)uiSize);

                            string regex = "(\\[.*\\])|(\".*\")|('.*')|(\\(.*\\))|\\\r}.*?\\\0";
                            string output = Regex.Replace(strTemp, regex, "");

                            msgform.textBox1.Text = output.Replace("\r\0", "");

                            API_GoodRead();
                        }
                        else
                        {
                            //msgform.txtData.Text = "No Data";
                        }
                        break;
                    case SM_ERROR_SYSTEM:
                        strTemp = String.Format("System Error Code: {0:d}", API_GetSysError());
                        MessageBox.Show(strTemp);
                        break;
                    case SM_ERROR_API:
                        strTemp = String.Format("API Error Code: {0:d}", API_GetError());
                        MessageBox.Show(strTemp);
                        break;
                }
                // Call the base WndProc method
                // to process any messages not handled.
                base.WndProc(ref msg);
            }
        }

        private void Patrons_Load(object sender, EventArgs e)
        {
            Directory.CreateDirectory(Path.Combine(@"", @"LibSys\Patrons"));
            
            statusBar1.Text = "Items scanned : 0";
            if (!API_Register(MsgWin.Hwnd))
            {
                MessageBox.Show("Scanner is unaccessible");
                Application.Exit();
            }
            txtFile.Text = "Patrons-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Year.ToString() + ".txt";     
        
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            listBox1.Items.Add(this.textBox1.Text);
            statusBar1.Text =
                "Items Scanned " + listBox1.Items.Count.ToString();
        
        }

        private void Patrons_Closing(object sender, CancelEventArgs e)
        {
            API_Unregister();
            Application.Exit();
        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
            API_Unregister();
            asset f = new asset();
            f.Show();
            this.Hide();
        }

        private void menuItem3_Click(object sender, EventArgs e)
        {
            API_Unregister();
            Form1 f = new Form1();
            f.Show();
            this.Hide();
        }

        private void menuItem4_Click(object sender, EventArgs e)
        {
            API_Unregister();
            Textbook f = new Textbook();
            f.Show();
            this.Hide();
        }

        private void menuItem5_Click(object sender, EventArgs e)
        {
            API_Unregister();
            Media f = new Media();
            f.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string date = "Patrons-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Year.ToString() + ".txt";
            try
            {
                if (!File.Exists(@"LibSys\Patrons\" + date)) // may have to specify path here!
                {
                    // may have to specify path here!
                    File.Create(@"LibSys\Patrons\" + date).Close();
                    // may have to specify path here!
                    StreamWriter swFile =
                                 new StreamWriter(
                                    new FileStream(@"LibSys\Patrons\" + date,
                                                   FileMode.Truncate),
                                    Encoding.ASCII);
                    //string text = "";
                    foreach (string li in listBox1.Items)
                    {
                        swFile.WriteLine(li);
                    }
                    MessageBox.Show("Successfull in entering data");
                    swFile.Close();
                }
                else
                {
                    StreamWriter swFile =
                                 new StreamWriter(@"LibSys\Patrons\" + date,
                                    true,
                                    Encoding.ASCII);
                    //string text = "";
                    foreach (string li in listBox1.Items)
                    {
                        swFile.WriteLine(li);
                    }
                    MessageBox.Show("Successfull in adding data");
                    swFile.Close();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error printing data" + ex.ToString());
            }
        }
    }
}
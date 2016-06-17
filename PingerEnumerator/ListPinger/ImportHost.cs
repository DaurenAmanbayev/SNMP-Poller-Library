using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text.RegularExpressions;

namespace ListPinger
{
    public partial class ImportHost : Form
    {
        List<string> Lines = new List<string>();
        public List<string> network = new List<string>();
        public List<string> prevNetwork=new List<string>();
        
        int countAdded = 0;
        public ImportHost(List<string> prev)
        {
            prevNetwork = prev;
            InitializeComponent();
        }
        #region METHODS
        //checking that address not duplicate in our list
        private void ImportNotDuplicate(string address)
        {            
            if (!prevNetwork.Contains(address))
            {
                network.Add(address);
                countAdded++;
            }
        }        
        //simple checking ip address from list
        private void ParseIPAddress(string line)
        {
            IPAddress address;
            if (IPAddress.TryParse(line, out address))
            {
                ImportNotDuplicate(address.ToString());
            }          
        }
        //extract lines from textbox
        private void ExtractLine(string line)
        {
            string pattern = "\r\n";
            int prev = 0, next = 0;
            int last = line.LastIndexOf(pattern) + 2;//step of pattern
            int size = line.Length;
            bool stop = true;
            while (stop)
            {
                next = line.IndexOf(pattern, prev);
                if (next > prev)
                {
                    Lines.Add(line.Substring(prev, next - prev));
                    prev = next + 2;
                }
                else
                {
                    if (last < size)
                    {
                        Lines.Add(line.Substring(last, size - last));
                    }
                    break;
                }
            }
        }

        public string[] ExtractLines(string input)
        {
            return Regex.Split(input, "\r\n");
        }
        //notification template
        private void Notification(string line)
        {
            MessageBox.Show(line, "Notification", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }
        #endregion

        #region BUTTONS
        //simple import address list from textbox
        private void buttonImport_Click(object sender, EventArgs e)
        {
            string text = textBoxImport.Text;
            if (!string.IsNullOrWhiteSpace(text))//if text is not null or whitespace
            {
                try
                {
                    Lines=ExtractLines(text).ToList();//cut lines from text
                    foreach (string line in Lines) 
                    {
                        ParseIPAddress(line);//parse lines
                    }
                    //and notify our user about count of imported addresses
                    Notification(string.Format("{0} ip addresses was imported from list!", countAdded));
                }
                catch (Exception ex)
                {
                    Notification(ex.Message);
                }
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                Notification("Import list is empty!");
            }
        }     
       
        
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        //parse and import from textbox
        private void buttonParse_Click(object sender, EventArgs e)
        {
            string text = textBoxImport.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                try
                {
                    List<IPAddress> list=RegexExtract.Singletone.ExtractIpAddress(text);//parse and extract address list from text
                    foreach (IPAddress address in list)
                    {
                        ImportNotDuplicate(address.ToString());//checking that address not duplicate
                    }                    
                    Notification(string.Format("{0} ip addresses was parsed and imported from list!", countAdded));
                }
                catch (Exception ex)
                {
                    Notification(ex.Message);
                }
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                Notification("Import list is empty!");
            }
        }
        //checking with parse methods
        private void buttonCheck_Click(object sender, EventArgs e)
        {
            string text = textBoxImport.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                try
                {
                    int count=RegexExtract.Singletone.ExtractIpAddress(text).Count;                    
                    Notification(string.Format("{0} ip addresses was parsed from list!", count));
                }
                catch (Exception ex)
                {
                    Notification(ex.Message);
                }               
            }
            else
            {
                Notification("Import list is empty!");
            }

        }
        //add one single host
        private void buttonAddHost_Click(object sender, EventArgs e)
        {
            string text = textBoxImport.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                ParseIPAddress(text);               
                Notification(string.Format("{0} ip addresses was imported from list!", countAdded));
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                Notification("Import list is empty!");
            }
        }
        #endregion
    }

   
}

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

namespace SnmpLibrary
{
    public partial class SnmpLibrary : Form
    {
        List<string> Lines = new List<string>();
        Hashtable network = new Hashtable();
        string library = "library.conf";
        public SnmpLibrary()
        {
            InitializeComponent();
            StartConfiguration();
        }

        private void StartConfiguration()
        {
            buttonCreate.Enabled = false;
        }
        /*FOR CREATING LIBRARY PLEASE INSERT INFORMATION ABOUT DEVICES ADDRESSES AND COMMUNITIES AS FOLLOWING FORMAT
         * 192.168.1.1 public
         * 192.168.1.2 guest
         */
        //извлечь строки из текста
        //private void ExtractLine(string line)
        //{
        //    string pattern = "\r\n";
        //    int prev = 0, next = 0;
        //    int last = line.LastIndexOf(pattern) + 2;//step of pattern
        //    int size = line.Length;
        //    bool stop = true;
        //    while (stop)
        //    {
        //        next = line.IndexOf(pattern, prev);//find required index
        //        if (next > prev) 
        //        {
        //            Lines.Add(line.Substring(prev, next - prev));//cut line between indexes
        //            prev = next + 2;
        //        }
        //        else
        //        {
        //            if (last < size)//checking out of range
        //            {
        //                Lines.Add(line.Substring(last, size - last));//last line adding
        //            }
        //            break;
        //        }
        //    }
        //}

        public string[] ExtractLines(string input)
        {
            return Regex.Split(input, "\r\n");
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            string text = richTextBoxLibraryContent.Text;
            IPAddress address;
            //********************************
            Lines=ExtractLines(text).ToList();
            foreach (string line in Lines)
            {
                string host = RegexExtract.Singletone.ExtractIP(line);
                string community = RegexExtract.Singletone.ParseCommunity(line.Substring(host.Length));             
                if(IPAddress.TryParse(host, out address) & community!="")
                {
                    if (network.ContainsKey(address))
                    {
                        network.Remove(address);
                        network.Add(address, community);
                    }
                    else
                    {
                        network.Add(address, community);
                    }
                }
            }
            try
            {
                WriteLibrary();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        #region FILEIO
        private void WriteLibrary()
        {
            File.WriteAllBytes(library, network.Serialize());
        }

        private void ReadLibrary()
        {
            try
            {
                byte[] data = File.ReadAllBytes(library);
                                
                Hashtable network = data.Deserialize();
                string line = "";
                string endline = Environment.NewLine;
                foreach (IPAddress key in network.Keys)
                {
                    line += key.ToString() + "\t" + network[key].ToString() + endline;
                }
                richTextBoxLibraryContent.Text = line;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }

        private void buttonRead_Click(object sender, EventArgs e)
        {
            ReadLibrary();
        }

        #endregion

        private void richTextBoxLibraryContent_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(richTextBoxLibraryContent.Text))
            {
                buttonCreate.Enabled = true;
            }
            else
            {
                buttonCreate.Enabled = false;
            }
        }
        //поиск и подсветка паттерна
        private void buttonFind_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                HighlightText(richTextBoxLibraryContent, textBoxSearch.Text, Color.Crimson);
            }        
        }
        //highlighting
        private void HighlightText(RichTextBox myRtb, string word, Color color)
        {
          //  UndoAction(myRtb);  
            
            int s_start = myRtb.SelectionStart, startIndex = 0, index;

            while ((index = myRtb.Text.IndexOf(word, startIndex)) != -1)
            {
                myRtb.Select(index, word.Length);
                myRtb.SelectionColor = color;
                startIndex = index + word.Length;
            }

            myRtb.SelectionStart = s_start;
            myRtb.SelectionLength = 0;
            myRtb.SelectionColor = Color.Black;
        }
        private void UndoAction(RichTextBox myRtb)
        {
            while (myRtb.CanUndo)
            {
                myRtb.Undo();
                // Clear the undo buffer to prevent last action from being
                myRtb.ClearUndo();
            }
        }
        private void buttonReplace_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxSearch.Text) && !string.IsNullOrWhiteSpace(textBoxReplace.Text))
            {
                ReplaceAll(richTextBoxLibraryContent, textBoxSearch.Text, textBoxReplace.Text);
            }
        }
        private void ReplaceAll(RichTextBox myRtb, string word, string replacement)
        {
            int i = 0;
            int n = 0;
            int a = replacement.Length - word.Length;
            foreach (Match m in Regex.Matches(myRtb.Text, word))
            {
                myRtb.Select(m.Index + i, word.Length);
                i += a;
                myRtb.SelectedText = replacement;
                n++;
            }
            MessageBox.Show("Replaced " + n + " matches!");
        }
    }
    public static class SerializeExtention
    {
        public static byte[] Serialize(this Hashtable data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, data);
            return stream.ToArray();
        }
        public static Hashtable Deserialize(this byte[] data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(data);
            return (Hashtable)formatter.Deserialize(stream);
        }
    }
}

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
        List<string> lines = new List<string>();
        SortedList network = new SortedList();
        string filter = "SNMP Library Files(*.sconf)|*.sconf|All Files(*.*)|*.*||";
        public SnmpLibrary()
        {
            InitializeComponent();          
        }
        
        /*
         * FOR CREATING LIBRARY PLEASE INSERT INFORMATION ABOUT DEVICES ADDRESSES AND COMMUNITIES AS FOLLOWING FORMAT
         * 192.168.1.1 public
         * 192.168.1.2 guest
         */      

        public string[] ExtractLines(string input)
        {
            return Regex.Split(input, "\n");//import problem with lines \r\n or \n
        }
        private void ImportFromTextBox()
        {
            string text = richTextBoxLibraryContent.Text;
            IPAddress address;        
            lines=ExtractLines(text).ToList();
            foreach (string line in lines)
            {
                string host = RegexExtract.Singletone.ExtractIP(line);
                string community = RegexExtract.Singletone.ParseCommunity(line.Substring(host.Length));             
                if(IPAddress.TryParse(host, out address) & !string.IsNullOrWhiteSpace(community))
                {
                    if (network.ContainsKey(host))
                    {
                        network.Remove(host);
                        network.Add(host, community);
                    }
                    else
                    {
                        network.Add(host, community);
                    }
                }
            }            
        }                         
        private void ClearWorkspace()
        {
            DialogResult result = MessageBox.Show("Are you sure to clear workspace?", "Clear Workspace", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                richTextBoxLibraryContent.Text = "";
                lines.Clear();
                network.Clear();
            }
        }
        private void OpenLibrary()
        {
            OpenFileDialog open = new OpenFileDialog(); //создали экземпляр
            //установим фильтр для файлов
            open.Filter=filter;
            open.FilterIndex = 1;//по умолчанию фильтруются текстовые файлы
            if (open.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    byte[] data = File.ReadAllBytes(open.FileName);
                   // ImportFromTextBox();
                    SortedList network = data.Deserialize();
                    StringBuilder builder = new StringBuilder();
                    foreach (var key in network.Keys)
                    {
                        builder.AppendLine(key+ "\t" + network[key].ToString());
                    }
                    richTextBoxLibraryContent.Text = builder.ToString();
                }
                catch (Exception)
                {
                    MessageBox.Show("Wrong format!", "SNMP Library", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }

            }
        }
        private void SaveLibrary()
        {
            SaveFileDialog save = new SaveFileDialog();//создали экземпляр
            save.Filter = filter;
            save.FilterIndex = 1;//по умолчанию фильтруются текстовые файлы
            if (save.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ImportFromTextBox();
                    File.WriteAllBytes(save.FileName, network.Serialize());
                }
                catch (Exception e)
                {
                    MessageBox.Show("Can't Access! "+e.Message, "SNMP Library", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
        }
        private void ImportFromExcel()
        {
            ImportTool frm = new ImportTool();
            DialogResult result = frm.ShowDialog();
            {
                var imports = frm.GetImports;
                StringBuilder builder = new StringBuilder();
                string prevContent = richTextBoxLibraryContent.Text;
                builder.AppendLine(prevContent);
                foreach (var key in imports.Keys)
                {
                    builder.AppendLine(key + "\t" + imports[key].ToString());
                }               
                richTextBoxLibraryContent.Text = builder.ToString();
            }
        }
        private void GetInfo()
        {
            MessageBox.Show("SNMP Library http://daurenamanbayev.github.io/ 2016", "Info");
        }
        
        #region FIND AND REPLACE
        private void findAndReplace()
        {
            findToolStripMenuItem.Enabled = false;
            toolStripButtonFindTool.Enabled = false;
            FindTool creator = new FindTool();
            creator.Owner = this;
            creator.KeyChange += TextOnChangeFind;
            creator.Show();
            creator.Closed += FindOnClosed;
        }
        void TextOnChangeFind(object sender, EventArgs args)
        {
            FindTool creator = (FindTool)sender;
            string pattern = creator.GetPattern;
            if (creator.isReplaceMethod)
            {
                ReplaceAll(richTextBoxLibraryContent, pattern, creator.GetReplaceWord);
            }
            else
            {
                FindPattern(pattern);
            }
        }
        void FindOnClosed(object sender, EventArgs args)
        {
            findToolStripMenuItem.Enabled = true;
            toolStripButtonFindTool.Enabled = true;
        }

        //поиск и подсветка паттерна
        private void FindPattern(string pattern)
        {
            if (!string.IsNullOrWhiteSpace(pattern))
            {
                HighlightText(richTextBoxLibraryContent, pattern, Color.Crimson);
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
        private void ReplaceAll(RichTextBox myRtb, string pattern, string replaceWord)
        {
            int i = 0;
            int n = 0;
            int a = replaceWord.Length - pattern.Length;
            foreach (Match m in Regex.Matches(myRtb.Text, pattern))
            {
                myRtb.Select(m.Index + i, pattern.Length);
                i += a;
                myRtb.SelectedText = replaceWord;
                n++;
            }
            MessageBox.Show("Replaced " + n + " matches!");
        }
        #endregion

        #region MENU
        private void newLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearWorkspace();
        }
                
        private void openLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenLibrary();
        }

        private void saveLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveLibrary();
        }

        private void importFromExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportFromExcel();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            findAndReplace();
        }

        private void clearWorkspaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearWorkspace();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetInfo();
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GetInfo();
        }

        #endregion       

        #region TOOLSTRIP       
        private void toolStripButtonNewLibrary_Click(object sender, EventArgs e)
        {
            ClearWorkspace();
        }

        private void toolStripButtonOpenLibrary_Click(object sender, EventArgs e)
        {
            OpenLibrary();
        }

        private void toolStripButtonSaveLibrary_Click(object sender, EventArgs e)
        {
            SaveLibrary();
        }

        private void toolStripButtonImportFromExcel_Click(object sender, EventArgs e)
        {
            ImportFromExcel();
        }

        private void toolStripButtonFindTool_Click(object sender, EventArgs e)
        {
            findAndReplace();
        }
        #endregion

    }
    public static class SerializeExtention
    {
        public static byte[] Serialize(this SortedList data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, data);
            return stream.ToArray();
        }
        public static SortedList Deserialize(this byte[] data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(data);
            return (SortedList)formatter.Deserialize(stream);
        }
    }
}

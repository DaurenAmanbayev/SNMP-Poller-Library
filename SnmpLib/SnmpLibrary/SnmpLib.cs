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
        SortedList network = new SortedList();
        string library = "library.conf";
        public SnmpLibrary()
        {
            InitializeComponent();
            //StartConfiguration();
        }
        
        /*
         * FOR CREATING LIBRARY PLEASE INSERT INFORMATION ABOUT DEVICES ADDRESSES AND COMMUNITIES AS FOLLOWING FORMAT
         * 192.168.1.1 public
         * 192.168.1.2 guest
         */      

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
                if(IPAddress.TryParse(host, out address) & !string.IsNullOrWhiteSpace(community))
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
        
        
        #region MENU

        //File explorer
        private void LoadFile(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog(); //создали экземпляр
            //установим фильтр для файлов
            open.Filter = "RTF Files(*.rtf)|*.rtf|Text Files(*.txt)|*.txt|All Files(*.*)|*.*||";
            open.FilterIndex = 1;//по умолчанию фильтруются текстовые файлы
            if (open.ShowDialog() == DialogResult.OK)
            {
                StreamReader reader = File.OpenText(open.FileName);
                if (open.FileName.Contains(".txt"))
                {
                    try
                    {
                        richTextBoxMain.Text = reader.ReadToEnd(); //считываем файл до конца
                        reader.Close(); //закрываем reader
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Wrong format!", "Editor - Open", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
                if (open.FileName.Contains(".rtf"))
                {
                    try
                    {
                        richTextBoxMain.Rtf = reader.ReadToEnd(); //считываем файл до конца
                        reader.Close(); //закрываем reader
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Wrong format!", "Editor - Open", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
            }
        }
        private void SaveFile(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();//создали экземпляр
            save.Filter = "RTF Files(*.rtf)|*.rtf|Text Files(*.txt)|*.txt|All Files(*.*)|*.*||";
            save.FilterIndex = 1;//по умолчанию фильтруются текстовые файлы
            if (save.ShowDialog() == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(save.FileName);
                if (save.FileName.Contains(".txt"))
                {
                    try
                    {
                        writer.Write(richTextBoxMain.Text); //записываем в файл содержимое поля
                        writer.Close();//закрываем writer
                        toSave = false;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Not Access!", "Editor - Save", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
                if (save.FileName.Contains(".rtf"))
                {
                    try
                    {
                        writer.Write(richTextBoxMain.Rtf); //записываем в файл содержимое поля
                        writer.Close();//закрываем writer
                        toSave = false;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Not Access!", "Editor - Save", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
            }
        }

        private void newLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void saveLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void importFromExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void clearWorkspaceToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #region TOOLSTRIP
        private void toolStripButtonImport_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButtonOpenLibrary_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButtonSaveLibrary_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButtonFindTool_Click(object sender, EventArgs e)
        {

        }
        #endregion

        #region FIND AND REPLACE
        private void findAndReplaceToolStripMenuItem_Click(object sender, EventArgs e)
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
            string libraryContent = richTextBoxMain.Text;
            if (Rabina(pattern, libraryContent))
            {
                if (creator.isReplaceMethod)
                {
                    string changeText = creator.GetReplaceWord;
                    libraryContent = libraryContent.Replace(pattern, changeText);
                    richTextBoxMain.Text = libraryContent;
                    MessageBox.Show("Find and replaces " + countSuffix, "Editor - Find And Replace Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    countSuffix = 0;
                    suffixIndex.Clear();
                    suffixIndex.TrimToSize();
                }
                //use all selected item and other and other
                else
                {
                    foreach (int targetIndex in suffixIndex)
                    {
                        richTextBoxMain.Select(targetIndex, pattern.Length);
                        richTextBoxMain.SelectionColor = Color.Yellow;
                    }
                    MessageBox.Show("Find " + countSuffix, "Editor - Find Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    countSuffix = 0;
                    suffixIndex.Clear();
                    suffixIndex.TrimToSize();
                }
            }
            else MessageBox.Show("Target text '" + pattern + "' not found!", "Editor - Not Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //richTextBox1.Find(creator.GetData);
        }
        void FindOnClosed(object sender, EventArgs args)
        {
            findToolStripMenuItem.Enabled = true;
            toolStripButtonFindTool.Enabled = true;
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
        //private void buttonReplace_Click(object sender, EventArgs e)
        //{
        //    if (!string.IsNullOrWhiteSpace(textBoxSearch.Text) && !string.IsNullOrWhiteSpace(textBoxReplace.Text))
        //    {
        //        ReplaceAll(richTextBoxLibraryContent, textBoxSearch.Text, textBoxReplace.Text);
        //    }
        //}
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
        public static Hashtable Deserialize(this byte[] data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(data);
            return (Hashtable)formatter.Deserialize(stream);
        }
    }
}

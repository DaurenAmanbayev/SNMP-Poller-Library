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

namespace ListPinger
{
    public partial class LibraryManager : Form
    {
        bool _workplaceIsClear = true;
        string _directoryPath=string.Empty;
       // List<string> _checkedLiraries = new List<string>();
        private string _checkedLibrary = string.Empty;

        public LibraryManager()
        {
            InitializeComponent();
        }
        public LibraryManager(string defaultFolder)
        {
            InitializeComponent();
            _directoryPath = defaultFolder;
            DiscoveryFolder(_directoryPath);
        }

        public string GetLibrary {
            get { return _checkedLibrary; }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            _checkedLibrary = checkedListBoxUsageLibs.SelectedItem as String;
            
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult =DialogResult.Cancel;
        }

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            DialogResult result = fbd.ShowDialog();

            if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                if (!_workplaceIsClear)
                {
                    checkedListBoxUsageLibs.Items.Clear();
                }
                DiscoveryFolder(fbd.SelectedPath);
                _directoryPath = fbd.SelectedPath;
            }
        }
        //обработка выбранной директории, и агрегации таблицы
        //можно расширить функционал для рекурсивного поиска вложенных файлов...
        private void DiscoveryFolder(string directoryPath)
        {
            _workplaceIsClear = false;
            string[] files = Directory.GetFiles(directoryPath);
            foreach (var file in files)
            {
                FileInfo info = new FileInfo(file);
                if (info.Extension.Equals(".sconf"))
                {
                    checkedListBoxUsageLibs.Items.Add(info.FullName, false);
                }
            }
            // toolStripComboBoxExtensions.ComboBox.DataSource = extensionsList;          

        }       
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
        
    }
}

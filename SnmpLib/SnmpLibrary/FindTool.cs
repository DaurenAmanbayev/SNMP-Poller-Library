using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SnmpLibrary
{
    public partial class FindTool : Form
    {
        string pattern =string.Empty;
        string replaceWord = string.Empty;
        bool caseSense = false;
        bool useAll = false;
        bool replace = false;
        public event EventHandler KeyChange;
        //-----------------------------------------
        int TogMove, MValX, MValY;
        public FindTool()
        {
            InitializeComponent();
        }

        #region FINDER DATA
        public string GetPattern
        {
            get { return pattern; }
        }
        public string GetReplaceWord
        {
            get { return replaceWord; }
        }
        //------------------------------
        public bool isCaseSense
        {
            get { return caseSense; }
        }
        public bool isUsedForAll
        {
            get { return replace; }
        }
        public bool isReplaceMethod
        {
            get { return replace; }
        }
        #endregion

        //find
        private void buttonFind_Click(object sender, EventArgs e)
        {
            pattern = textBoxPattern.Text;
            this.DialogResult = DialogResult.OK;//????
            if (!string.IsNullOrWhiteSpace(pattern))
            {
                this.DialogResult = DialogResult.OK;
                replace = false;               
                if (KeyChange != null)
                    KeyChange(this, new EventArgs());
            }
            else MessageBox.Show("Empty Value", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }        
        //cancel
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //replace
        private void buttonReplace_Click(object sender, EventArgs e)
        {
            pattern = textBoxPattern.Text;
            replaceWord = textBoxReplaceWord.Text;
            if (!string.IsNullOrWhiteSpace(pattern) && !string.IsNullOrWhiteSpace(replaceWord))
            {
                this.DialogResult = DialogResult.OK;
                replace = true;               
                if (KeyChange != null)
                    KeyChange(this, new EventArgs());
            }
            else MessageBox.Show("Empty Value", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        #region MOVE WINDOW ON DESKTOP
        //Move Window on Desktop
        private void Submenu_MouseDown(object sender, MouseEventArgs e)
        {
            TogMove = 1; MValX = e.X; MValY = e.Y;
        }      
        private void Submenu_MouseMove(object sender, MouseEventArgs e) 
        { 
            if (TogMove == 1) 
            { 
                this.SetDesktopLocation(MousePosition.X - MValX, MousePosition.Y - MValY); 
            } 
        }
        private void Submenu_MouseUp(object sender, MouseEventArgs e)
        {
            TogMove = 0;
        }
        //-------------------------------------------------
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (!useAll) useAll = true;
            else useAll = false;
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!caseSense) caseSense = true;
            else caseSense = false;
        }
        #endregion
    }
}

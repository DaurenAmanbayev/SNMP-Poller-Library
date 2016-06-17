using LinqToExcel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SnmpLibrary
{
    public partial class ImportTool : Form
    {
        SortedList network = new SortedList();
        List<string> uniqList = new List<string>();
        public ImportTool()
        {
            InitializeComponent();
        }
        //импорт данных из таблицы Excel
        private void ImportFromExcel(string filename)
        {
            try
            {
                var excel = new ExcelQueryFactory();
                excel.FileName = filename;
                //указываем, что первый столбец называется IP Address
                excel.AddMapping<Node>(x => x.Address, "IP Address");
                //на листе Network
                var data = from c in excel.Worksheet<Node>("Network")
                           select c;
                //указываем, что начало обработки данных в указанном диапазоне
                var network = from c in excel.WorksheetRange<Node>("A1", "B1")
                              select c;
                dataGridViewImport.DataSource = data.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void openExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Excel Files(*.xls)|*.xlsx";
            open.FilterIndex = 1;
            if (open.ShowDialog() == DialogResult.OK)
            {
                //проверить производительность!!!!
                UseWaitCursor = true;
                ImportFromExcel(open.FileName);
                UseWaitCursor = false;

            }
        }
        //агрегация данных в таблице
        private void DataAggregate()
        {
            foreach (DataGridViewRow row in dataGridViewImport.Rows)
            {
                DataGridViewCell cellAddress = row.Cells["Address"];
                IPAddress address;
                if (cellAddress.Value != null)
                {

                    //если значение адреса не пустое, и ip address валидный
                    if (!string.IsNullOrWhiteSpace(cellAddress.Value.ToString()) && IPAddress.TryParse(cellAddress.Value.ToString(), out address))
                    {
                        DataGridViewCell cellHostname = row.Cells["Hostname"];
                        //если значение имени устройства не пустое
                        if (!string.IsNullOrWhiteSpace(cellHostname.Value.ToString()))
                        {
                            //если в таблице уже существует идентичный адрес, не добавлять
                            if (!network.ContainsKey(cellAddress.Value.ToString()))
                            {
                                //если в таблице существует идентичный устройство, не добавлять
                                if (!network.ContainsValue(cellHostname.Value.ToString()))
                                {
                                    network.Add(cellAddress.Value.ToString(), cellHostname.Value.ToString());
                                }
                                else
                                {
                                    //указываем, что значение было не уникальным
                                    uniqList.Add(cellHostname.Value.ToString());
                                }
                            }
                            else
                            {
                                //указываем, что значение было не уникальным
                                uniqList.Add(cellAddress.Value.ToString());
                            }
                        }
                    }
                }
            }
            //clear datagridview
            dataGridViewImport.DataSource = null;
        }
        
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataAggregate();
            this.DialogResult = DialogResult.OK;
        }
    }
}

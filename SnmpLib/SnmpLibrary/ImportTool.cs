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
        public SortedList GetImports
        {
            get { return network; }
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
               // MessageBox.Show(data.ToList().Count.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void openExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Excel Files(*.xlsx)|*.xlsx";
            open.FilterIndex = 1;
            if (open.ShowDialog() == DialogResult.OK)
            {
                //проверить производительность!!!!
                this.UseWaitCursor = true;
                ImportFromExcel(open.FileName);               
                this.UseWaitCursor = false;

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
                        DataGridViewCell cellCommunity = row.Cells["Community"];
                        //если значение имени устройства не пустое
                        if (!string.IsNullOrWhiteSpace(cellCommunity.Value.ToString()))
                        {
                            //если в таблице уже существует идентичный адрес, удаляем предыдущее значение, добавляем новое
                            if (network.ContainsKey(address.ToString()))
                            {
                                network.Remove(address.ToString());
                                network.Add(address.ToString(), cellCommunity.Value.ToString());

                                //указываем, что значение было не уникальным
                                uniqList.Add(address.ToString());
                            }
                            //добавляем адрес, если ранее не встречался
                            else
                            {
                                network.Add(address.ToString(), cellCommunity.Value.ToString());
                            }
                        }
                    }
                }
            }
            //clear datagridview
            dataGridViewImport.DataSource = null;
            dataGridViewImport.Rows.Clear();
        }        
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataAggregate();
            MessageBox.Show(string.Format("Обнаружено {0} конфликтов в данных", uniqList.Count), "Конфликты");
            this.DialogResult = DialogResult.OK;
        }
    }
}

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

namespace ListPinger
{
    public partial class ImportHost : Form
    {
        List<string> Lines = new List<string>();
        public List<IPAddress> network = new List<IPAddress>();
        public List<IPAddress> prevNetwork=new List<IPAddress>();
        
        int countAdded = 0;        
        public ImportHost(List<IPAddress> prevNetwork)
        {
            InitializeComponent();
            this.prevNetwork.AddRange(prevNetwork);
        }
        

        #region METHODS
        //проверка на наличие в списке
        private void ImportNotDuplicate(IPAddress address)
        {            
            if (!prevNetwork.Contains(address))
            {
                network.Add(address);
                countAdded++;
            }
        }        
        //простая проверка из списка строк
        private void ParseIPAddress(string line)
        {
            IPAddress address;
            if (IPAddress.TryParse(line, out address))
            {
                ImportNotDuplicate(address);
            }          
        }
        //извлечь строки из текста
        private void ExtractLine(string line)
        {
            string pattern = "\r\n";
            int prev = 0, next = 0;
            int last = line.LastIndexOf(pattern) + 2;//step of pattern
            int size = line.Length;
            bool stop = true;
            while (stop)
            {
                next = line.IndexOf(pattern, prev);//находим требуемый индекс
                if (next > prev)//если следующий индекс больше, чем предыдущий 
                {
                    Lines.Add(line.Substring(prev, next - prev));//добавить отрезок строки между индексами
                    prev = next + 2;
                }
                else//если следующий индекс меньше, чем предыдущий 
                {
                    if (last < size)//проверяем, что индекс последнего совпадения меньше чем размер текста
                    {
                        Lines.Add(line.Substring(last, size - last));//добавляем конец строки в наш перечень
                    }
                    break;
                }
            }
        }
        //уведомления
        private void Notification(string line)
        {
            MessageBox.Show(line, "Notification", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }
        #endregion

        #region BUTTONS
        //простой импорт из перечня строк
        private void buttonImport_Click(object sender, EventArgs e)
        {
            string text = textBoxImport.Text;
            if (text.Length != 0)//если размер текста не равно нулю
            {
                try
                {
                    ExtractLine(text);//извлекаем строки нашего текста
                    foreach (string line in Lines)//для каждой строки 
                    {
                        ParseIPAddress(line);//проводим парсинг
                    }
                    //уведомляем пользователя о количестве импортированных адресов
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
       
        //отмена операции
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        //парсинг и импорт адресов из текста
        private void buttonParse_Click(object sender, EventArgs e)
        {
            string text = textBoxImport.Text;
            if (text.Length != 0)//если размер текста не равно нулю
            {
                try
                {
                    List<IPAddress> list=RegexExtract.Singletone.ExtractIpAddress(text);
                    foreach (IPAddress address in list)
                    {
                        ImportNotDuplicate(address);
                    }
                    //уведомляем пользователя о количестве импортированных адресов
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
        //проверка парсингом
        private void buttonCheck_Click(object sender, EventArgs e)
        {
            string text = textBoxImport.Text;
            if (text.Length != 0)//если размер текста не равно нулю
            {
                try
                {
                    int count=RegexExtract.Singletone.ExtractIpAddress(text).Count;
                    //уведомляем пользователя о количестве импортированных адресов
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
        //добавить адрес
        private void buttonAddHost_Click(object sender, EventArgs e)
        {
            string text = textBoxImport.Text;
            if (text.Length != 0)//если размер текста не равно нулю
            {
                ParseIPAddress(text);
                //уведомляем пользователя о количестве импортированных адресов
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

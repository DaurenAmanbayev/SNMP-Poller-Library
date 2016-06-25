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
using System.Net.NetworkInformation;
using System.Collections;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


namespace ListPinger
{
    struct AgentInfo
    {
        public IPAddress host;
        public string key;
        public string community;
    }
    /*
    добавить папку с библиотеками, сделать функцию поиска 
    или же в отдельном окне сделать менеджер библиотек, затем обращаться к файлу при выборе с проведения опроса устройств
    */

    public partial class ListPinger : Form
    {
        #region VARIABLES

        List<string> network = new List<string>();
        List<Task> manager = new List<Task>();
        List<string> prevSnmpData = new List<string>(); //snmp oid or key unique

        SortedList SnmpLibrary = new SortedList();
        bool isLibrary = false;
        bool isLock = false;
        int successCount = 0;
        int failedCount = 0;
        string lineEnd = Environment.NewLine;
        string lineDivider = new string('-', 80);
        readonly string logjournal = "logging.dat";
        string community = "public";
        readonly string folder = "lib";
        string library = "library";
        string key = "1.3.6.1.2.1.1.3.0";

        StringBuilder logBuilder = new StringBuilder();

        #endregion

        public ListPinger()
        {
            InitializeComponent();
            lineDivider += lineEnd;

            toolStripComboBoxCommunity.Items.Add(community);
            toolStripComboBoxCommunity.Items.Add(library);
            toolStripComboBoxOID.Items.Add(key);

            prevSnmpData.Add(community);
            prevSnmpData.Add(library);
            prevSnmpData.Add(key);
            ReadLibrary();
        }

        private void ReadLibrary()
        {
            try
            {
                byte[] data = File.ReadAllBytes("library.sconf");
                SnmpLibrary = data.Deserialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region PINGER
        private static object lockObject=new object();
        //автомат доступности устройства
        private void Pinger(object host)
        {
            Ping pingSender = new Ping();
            IPAddress address = (IPAddress)host;
            PingReply reply = pingSender.Send(address);
            StringBuilder report = new StringBuilder();  
            //если проверка успешна     
            if (reply.Status == IPStatus.Success)
            {
                //создать отчет на основе данных
                report.AppendLine(String.Format("Address: {0}", reply.Address.ToString()));
                report.AppendLine(String.Format("RoundTrip time: {0}", reply.RoundtripTime));
                report.AppendLine(String.Format("Time to live: {0}", reply.Options.Ttl));
                report.AppendLine(String.Format("Don't fragment: {0}", reply.Options.DontFragment));
                report.AppendLine(String.Format("Buffer size: {0}", reply.Buffer.Length));
                successCount++;//увеличиваем количество успешных проверок                   
            }
            //если проверка не успешна
            else
            {
                report.AppendLine(address.ToString());
                report.AppendLine(reply.Status.ToString());
                failedCount++;//увеличиваем количество неуспешных проверок                                         
            }
            report.AppendLine(lineDivider);
            //блокируем наш объект
            lock(lockObject){
                logBuilder= report;
                Logging(report.ToString());//=>log
            }
        }       
        //блокировка кнопок
        private void Lock()
        {
            //если блокированы кнопки
            if (isLock)
            {
                toolStripButtonImport.Enabled = true;                
                toolStripButtonStart.Enabled = true;
                toolStripButtonClearLog.Enabled = true;
                toolStripButtonSnmpGet.Enabled = true;
                this.Cursor = Cursors.Default;
            }
            //если не заблокированы
            else
            {
                toolStripButtonImport.Enabled = false;
                toolStripButtonStart.Enabled = false;
                toolStripButtonClearLog.Enabled = false;
                toolStripButtonSnmpGet.Enabled = false;
                this.Cursor = Cursors.WaitCursor;
            }
            isLock = !isLock;
        }
        //IMPORT DATA
        private void toolStripButtonImport_Click(object sender, EventArgs e)
        {
            ImportHost frm = new ImportHost(network);
            DialogResult result = frm.ShowDialog();
            if (result == DialogResult.OK)
            {
                network.AddRange(frm.network);
                network.Sort();
                frm.Close();
                ListUpdate();
            }
        }
        //UPDATE
        private void ListUpdate()
        {
            listBoxAddress.Items.Clear();//очищаем список 
            listBoxAddress.Items.AddRange(network.ToArray());//добавляем в список
        }
        //START PINGER
        private void toolStripButtonStartPing_Click(object sender, EventArgs e)
        {
            try
            {
                Lock();
                ProgressInit();//init current progress bar
                foreach (var host in network)
                {
                    richTextBoxLog.Text += string.Format("Pinger started for {0} ip address" + Environment.NewLine, host);
                    IPAddress address = IPAddress.Parse(host);
                    Task task = Task.Factory.StartNew(Pinger, address);
                    //задачи не имеют доступа к richtextbox          
                    manager.Add(task);
                }
                //ожидаем выполнение каждой операции
                foreach (Task task in manager)
                {                    
                    task.Wait();
                    ProgressStep();//perform progress bar steps
                    richTextBoxLog.Text += logBuilder;
                }
                manager.Clear();
                //reporting            
                StringBuilder report = new StringBuilder();
                report.AppendLine(string.Format("Pinger report => success {0}, failed {1}, all {2}", successCount,failedCount, network.Count));
                report.AppendLine(lineDivider);
                richTextBoxLog.Text += report;
                Logging(report.ToString());//=>log
                ProgressClear();//clear progress
                successCount = 0;
                failedCount = 0;
                Lock();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message+ ex.StackTrace);
            }
        }      
        //CLEAR LOG
        private void toolStripButtonClearLog_Click(object sender, EventArgs e)
        {
            richTextBoxLog.Text = "";
        }
        #endregion

        #region ENUMERATOR
        //добавить другие методы для опроса устройства
        //snmpwalk, snmpbulkwalk, snmpbulkget
        //запрос snmpget 
        private void SnmpGet(object agentInfo)
        {
            try
            {
                AgentInfo agent = (AgentInfo)agentInfo;
                var result = Messenger.Get(VersionCode.V2,
                    new IPEndPoint(agent.host, 161),
                    new OctetString(agent.community),
                    new List<Variable> { new Variable(new ObjectIdentifier(agent.key)) },
                     500);
                StringBuilder report = new StringBuilder();
                report.AppendFormat("SNMP access checking for {0} by {1} key with {2} community\r\n", agent.host, agent.key,agent.community);
                //доработать ...
                if (result.Count > 0)
                {
                    successCount++;
                }
                else
                {
                    failedCount++;
                }

                foreach (var variable in result)
                {
                    report.AppendLine(variable.ToString());
                }
                lock (lockObject)
                {
                    logBuilder.AppendLine(report.ToString()+lineDivider);
                    Logging(report.ToString());//=>log
                }
            }
            catch (Exception)
            {
                failedCount++;
                lock (lockObject)
                {
                    logBuilder.AppendLine(String.Format("Time out for {0} was exceeded...", ((AgentInfo) agentInfo).host) + lineEnd + lineDivider);
                    Logging(String.Format("Time out for {0} was exceeded...", ((AgentInfo) agentInfo).host) + lineEnd + lineDivider);
                }
            }

        }
        //добавить другие методы, а также функционал для возможности выбора метода

        private void toolStripButtonSnmpGet_Click(object sender, EventArgs e)
        {
            if (CheckSnmpData())
            {
                Enum();
            }
            else
            {
                MessageBox.Show("Oid or community is wrong", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void Enum()
        {
            if (isLibrary)
            {
                Lock();
                ProgressInit();//init current progress bar              
                foreach (var host in network)
                {
                    richTextBoxLog.Text += string.Format("Enumerator started for {0} ip address" + Environment.NewLine, host);
                    AgentInfo agent = new AgentInfo();
                    agent.host = IPAddress.Parse(host);
                    //agent.community = "Zapad_RO";
                    //agent.key = "1.3.6.1.2.1.1.3.0";
                    if (SnmpLibrary.ContainsKey(host))
                    {
                        agent.community = (string)SnmpLibrary[host];
                        agent.key = key;
                        Task task = Task.Factory.StartNew(SnmpGet, agent);
                        //задачи не имеют доступа к richtextbox          
                        manager.Add(task);
                    }
                    
                }
                richTextBoxLog.Text += lineDivider;
                //ожидаем выполнение каждой операции
                foreach (Task task in manager)
                {
                    ProgressStep();//perform progress bar steps
                    task.Wait();
                }
                richTextBoxLog.Text += logBuilder;
                manager.Clear();
                //reporting            
                StringBuilder report = new StringBuilder();
                report.AppendFormat("Enumerator report => success {0}, failed {1}, all {2}", successCount, failedCount, network.Count);
                report.AppendLine();
                report.AppendLine(lineDivider);
                richTextBoxLog.Text += report;
                Logging(report.ToString());//=>log
                ProgressClear();//clear progress
                logBuilder.Clear();
                successCount = 0;
                failedCount = 0;
                Lock();
                isLibrary = false;
            }
            else
            {
                Lock();
                ProgressInit();//init current progress bar              
                foreach (var host in network)
                {
                    richTextBoxLog.Text += string.Format("Enumerator started for {0} ip address" + Environment.NewLine, host);
                    AgentInfo agent = new AgentInfo();
                    agent.host = IPAddress.Parse(host);
                    //agent.community = "Zapad_RO";
                    //agent.key = "1.3.6.1.2.1.1.3.0";
                    agent.community = community;
                    agent.key = key;
                    Task task = Task.Factory.StartNew(SnmpGet, agent);
                    //задачи не имеют доступа к richtextbox          
                    manager.Add(task);
                }
                richTextBoxLog.Text += lineDivider;
                //ожидаем выполнение каждой операции
                foreach (Task task in manager)
                {
                    ProgressStep();//perform progress bar steps
                    task.Wait();    
                }
                richTextBoxLog.Text += logBuilder;
                manager.Clear();
                //reporting            
                StringBuilder report = new StringBuilder();
                report.AppendFormat(
                    String.Format("Enumerator report => success {0}, failed {1}, all {2}", successCount, failedCount,
                        network.Count));
                report.AppendLine();
                report.AppendLine(lineDivider);
                richTextBoxLog.Text += report;
                Logging(report.ToString());//=>log
                ProgressClear();//clear progress
                logBuilder.Clear();
                successCount = 0;
                failedCount = 0;
                Lock();
            }
        }
      
        //проверка данных для snmp опроса
        private bool CheckSnmpData()
        {
            bool success = true;
            string community = RegexExtract.Singletone.ParseCommunity(toolStripComboBoxCommunity.Text);
            string key = RegexExtract.Singletone.ParseOidKey(toolStripComboBoxOID.Text);
            //-------------------------------------------
            if(community==library)
                isLibrary=true;

            if (community.Length != 0 && key.Length != 0)
            {
                this.community = community;
                this.key = key;
                //запомнить oid и community использовавшиеся ранее, если не встречались ранее
                if (!prevSnmpData.Contains(community))
                {
                    toolStripComboBoxCommunity.Items.Add(community);
                    prevSnmpData.Add(community);
                }
                if (!prevSnmpData.Contains(key))
                {
                    toolStripComboBoxOID.Items.Add(key);
                    prevSnmpData.Add(key);
                }
            }
            else success = false;
            return success;
        }
        #endregion

        #region PROGRESS BAR
        //PROGRESS INIT
        private void ProgressInit()
        {
            toolStripProgressBarOperation.Maximum = network.Count;
            toolStripProgressBarOperation.Minimum = 0;
            toolStripProgressBarOperation.Step = 1;
        }
        //PROGRESS CHANGES
        private void ProgressStep()
        {
            toolStripProgressBarOperation.PerformStep();            
        }
        //PROGRESS CLEAR
        private void ProgressClear()
        {
            toolStripProgressBarOperation.Value = 0;
        }
        #endregion

        #region CONTEXT MENU
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listBoxAddress.Items.Count; i++)
            {
               listBoxAddress.SetSelected(i, true);
            }
        }

        private void unselectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBoxAddress.ClearSelected();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListBox.SelectedObjectCollection selectedItems = new ListBox.SelectedObjectCollection(listBoxAddress);
            selectedItems = listBoxAddress.SelectedItems;

            if (listBoxAddress.SelectedIndex != -1)
            {
                //IPAddress address = IPAddress.Parse(selectedItems[0].ToString());
                network.Remove(selectedItems[0].ToString());
                listBoxAddress.Items.Remove(selectedItems[0]);

            }
            else
                MessageBox.Show("Not selected item's", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void deleteSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListBox.SelectedObjectCollection selectedItems = new ListBox.SelectedObjectCollection(listBoxAddress);
            selectedItems = listBoxAddress.SelectedItems;

            if (listBoxAddress.SelectedIndex != -1)
            {
                for (int i = selectedItems.Count - 1; i >= 0; i--)
                {
                    //IPAddress address = IPAddress.Parse(selectedItems[i].ToString());
                    network.Remove(selectedItems[i].ToString());
                    listBoxAddress.Items.Remove(selectedItems[i]);
                }
            }
            else
                MessageBox.Show("Not selected item's", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result=MessageBox.Show("Are you sure to clear all addresses?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.OK)
            {
                listBoxAddress.Items.Clear();
                network.Clear();
            }
        }
        #endregion

        #region WRITE
        void Logging(string log)
        {
            string[] content = new string[1] { "===== Logging data ====" };
            FileInfo fileInf = new FileInfo(logjournal);
            if (fileInf.Exists && fileInf.Length < 4000000)//если размер не превышает 4 Мб, прочитать и дополнить данные лога
            {
                FileRead(logjournal, ref content);
            }
            string buffer = string.Join("\n", content);               
            string space = " => ";
            string date = DateTime.Now.ToString();
            StringBuilder logs = new StringBuilder();
            logs.AppendLine(buffer);
            logs.AppendFormat(date + space + log);
            WriteCharacters(logs.ToString(), logjournal);
        }
        public void FileRead(string targetPath, ref string[] content)
        {
            try
            {
                content = File.ReadAllLines(targetPath);
            }
            catch (Exception)
            {

            }
        }
        public async void WriteCharacters(string targetText, string targetPath)
        {
            try
            {
                using (StreamWriter writer = File.CreateText(targetPath))
                {
                    await writer.WriteLineAsync(targetText);
                }
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region HELP
        private void toolStripButtonHelp_Click(object sender, EventArgs e)
        {
            string info = "Use utils for ping and snmp access checking to remote devices.";
            info += "\r\nFor creating snmp address and community library use other utils SnmpLibrary.";
            info+="\r\nCopy created file library.conf to local folder. For using library in snmp checking select library in Community ComboBox.";
            MessageBox.Show(info, "Help", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

        }
        #endregion

        private void toolStripButtonSettings_Click(object sender, EventArgs e)
        {
            LibraryManager frm=new LibraryManager();
            DialogResult result = frm.ShowDialog();

            if (result == DialogResult.OK)
            {

            }
        }

       

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

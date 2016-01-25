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
    public partial class ListPinger : Form
    {
        #region VARIABLES
        List<IPAddress> network = new List<IPAddress>();
        List<Task> manager = new List<Task>();
        List<string> prevSnmpData = new List<string>();//snmp oid or key unique

        Hashtable SnmpLibrary = new Hashtable();
        bool isLibrary = false;
        bool isLock = false;
        int successCount = 0;
        int failedCount = 0;
        string lineEnd = Environment.NewLine;
        string lineDivider = new string('-', 80);
        string logjournal = "logging.dat";
        string community = "public";
        string library = "library";
        string key = "1.3.6.1.2.1.1.3.0";
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
            ReadConfiguration();
        }

        private void ReadConfiguration()
        {
            try
            {
                byte[] data = File.ReadAllBytes("library.conf");
                SnmpLibrary = data.Deserialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region Pinger
        private static object lockObject=new object();
        //автомат доступности устройства
        private void Pinger(object host)
        {
            Ping pingSender = new Ping();
            IPAddress address = (IPAddress)host;
            PingReply reply = pingSender.Send(address);                    
            string report = "";     
            //если проверка успешна     
            if (reply.Status == IPStatus.Success)
            {
                //создать отчет на основе данных
                report += string.Format("Address: {0}", reply.Address.ToString()+lineEnd);
                report += string.Format("RoundTrip time: {0}", reply.RoundtripTime) + lineEnd;
                report += string.Format("Time to live: {0}", reply.Options.Ttl + lineEnd);
                report += string.Format("Don't fragment: {0}", reply.Options.DontFragment + lineEnd);
                report += string.Format("Buffer size: {0}", reply.Buffer.Length + lineEnd);
                successCount++;//увеличиваем количество успешных проверок                   
            }
            //если проверка не успешна
            else
            {
                report += address.ToString() + lineEnd;
                report += reply.Status+lineEnd;
                failedCount++;//увеличиваем количество неуспешных проверок                                         
            }
            report += lineDivider;
            //блокируем наш объект
            lock(lockObject){
                richTextBoxLog.Text+= report;
                Logging(report);//=>log
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
            Lock();
            ProgressInit();//init current progress bar
            foreach (IPAddress host in network)
            {
                richTextBoxLog.Text += string.Format("Pinger started for {0} ip address" + Environment.NewLine, host);
                Task task = Task.Factory.StartNew(Pinger, host);
                //задачи не имеют доступа к richtextbox          
                manager.Add(task);
            }
            //ожидаем выполнение каждой операции
            foreach (Task task in manager)
            {
                ProgressStep();//perform progress bar steps
                task.Wait();               
            }
            manager.Clear();
            //reporting            
            string report = "";       
            report += string.Format("Pinger report => success {0}, failed {1}, all {2}", successCount, failedCount, network.Count)+lineEnd;          
            report += lineDivider;
            richTextBoxLog.Text +=report;
            Logging(report);//=>log
            ProgressClear();//clear progress
            successCount = 0;
            failedCount = 0;
            Lock();            
        }      
        //CLEAR LOG
        private void toolStripButtonClearLog_Click(object sender, EventArgs e)
        {
            richTextBoxLog.Text = "";
        }
        #endregion

        #region Enumerator
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
                string report = string.Format("SNMP access checking for {0} by {1} key with {2} community", agent.host, agent.key, agent.community) + lineEnd;
                //доработать ...
                if (result.Count > 0)
                {
                    successCount++;
                }
                else
                {
                    failedCount++;
                }

                foreach (Variable v in result)
                {
                    report += v + lineEnd;
                }
                lock (lockObject)
                {
                    richTextBoxLog.Text += report + lineDivider;
                    Logging(report);//=>log
                }
            }
            catch (Exception)
            {
                failedCount++;
                lock (lockObject)
                {
                    richTextBoxLog.Text += string.Format("Time out for {0} was exceeded...", ((AgentInfo)agentInfo).host) + lineEnd + lineDivider;
                    Logging(string.Format("Time out for {0} was exceeded...", ((AgentInfo)agentInfo).host) + lineEnd + lineDivider);
                }
            }

        }

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
                foreach (IPAddress host in network)
                {
                    richTextBoxLog.Text += string.Format("Enumerator started for {0} ip address" + Environment.NewLine, host);
                    AgentInfo agent = new AgentInfo();
                    agent.host = host;
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
                //ожидаем выполнение каждой операции
                foreach (Task task in manager)
                {
                    ProgressStep();//perform progress bar steps
                    task.Wait();
                }
                manager.Clear();
                //reporting            
                string report = "";
                report += string.Format("Enumerator report => success {0}, failed {1}, all {2}", successCount, failedCount, network.Count) + lineEnd;
                report += lineDivider;
                richTextBoxLog.Text += report;
                Logging(report);//=>log
                ProgressClear();//clear progress
                successCount = 0;
                failedCount = 0;
                Lock();
                isLibrary = false;
            }
            else
            {
                Lock();
                ProgressInit();//init current progress bar              
                foreach (IPAddress host in network)
                {
                    richTextBoxLog.Text += string.Format("Enumerator started for {0} ip address" + Environment.NewLine, host);
                    AgentInfo agent = new AgentInfo();
                    agent.host = host;
                    //agent.community = "Zapad_RO";
                    //agent.key = "1.3.6.1.2.1.1.3.0";
                    agent.community = community;
                    agent.key = key;
                    Task task = Task.Factory.StartNew(SnmpGet, agent);
                    //задачи не имеют доступа к richtextbox          
                    manager.Add(task);
                }
                //ожидаем выполнение каждой операции
                foreach (Task task in manager)
                {
                    ProgressStep();//perform progress bar steps
                    task.Wait();
                }
                manager.Clear();
                //reporting            
                string report = "";
                report += string.Format("Enumerator report => success {0}, failed {1}, all {2}", successCount, failedCount, network.Count) + lineEnd;
                report += lineDivider;
                richTextBoxLog.Text += report;
                Logging(report);//=>log
                ProgressClear();//clear progress
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

        #region ProgressBar
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
                IPAddress address = IPAddress.Parse(selectedItems[0].ToString());
                network.Remove(address);
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
                    IPAddress address = IPAddress.Parse(selectedItems[i].ToString());
                    network.Remove(address);
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
            string[] content = new string[1] { "*** Logging data ***" };
            FileInfo fileInf = new FileInfo(logjournal);
            if (fileInf.Exists && fileInf.Length < 4000000)//если размер не превышает 4 Мб, прочитать и дополнить данные лога
            {
                FileRead(logjournal, ref content);
            }
            string buffer = string.Join("\n", content);
            string line = "\n";
            string space = " => ";
            string date = DateTime.Now.ToString();
            WriteCharacters(buffer + line + date + space + log, logjournal);
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

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
        }
        /*FOR CREATING LIBRARY PLEASE INSERT INFORMATION ABOUT DEVICES ADDRESSES AND COMMUNITIES AS FOLLOWING FORMAT
         * 192.168.1.1 public
         * 192.168.1.2 guest
         */
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
                next = line.IndexOf(pattern, prev);//find required index
                if (next > prev) 
                {
                    Lines.Add(line.Substring(prev, next - prev));//cut line between indexes
                    prev = next + 2;
                }
                else
                {
                    if (last < size)//checking out of range
                    {
                        Lines.Add(line.Substring(last, size - last));//last line adding
                    }
                    break;
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            string text = textBoxLibraryContent.Text;
            IPAddress address;
            ExtractLine(text);
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
                textBoxLibraryContent.Text = line;
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

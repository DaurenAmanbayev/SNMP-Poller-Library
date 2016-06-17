using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Collections;
using System.Threading.Tasks;

namespace ListPinger
{
    class RegexExtract
    {
        private RegexExtract() { }
        //конструкция шаблона Singletone
        private static RegexExtract _singletone;
        public static RegexExtract Singletone
        {
            get {
                if (_singletone == null)
                    _singletone = new RegexExtract();
                return _singletone;
            }
        }

        //snmp oid key checking
        public string ParseOidKey(string input)
        {
            string pattern = @"(\d+\.){1,}\d";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(input.Trim());
            return match.ToString();
        }
        //snmp community checking
        public string ParseCommunity(string input)
        {
            string pattern = @"(\S){1,}";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(input.Trim());
            return match.ToString();
        }        
        //заменить паттерны
        public string ReplacePattern(string input, string pattern, string replacement)
        {
            return Regex.Replace(input, pattern, replacement);
        }
        //извлечь список адресов из текста
        public List<IPAddress> ExtractIpAddress(string input)
        {
            //string pattern1 = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";//pattern retrieve between divide symbol <'>ip<'>            
            //string pattern2 = @"\d+\.\d+\.\d+\.\d+";//retrieve data as window.lo999.999.999.99999cation to => 999.999.999.99999
            string pattern = @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}";//clear ip retrieve

            Regex regexIp = new Regex(pattern);
            MatchCollection result = regexIp.Matches(input);
            List<IPAddress> list = new List<IPAddress>();
            IPAddress host;
            foreach (Match match in result)
            {
                string strAddress = match.ToString();
                if (IPAddress.TryParse(strAddress, out host))
                {
                    if (!list.Contains(host))
                    {
                        list.Add(host);
                    }
                }
            }
            return list;
        }

        #region UTILS
        //вернуть список совпадений по паттерну
        public ArrayList Separator(string input, string pattern)
        {
            Regex reg = new Regex(pattern);
            MatchCollection result = reg.Matches(input);
            ArrayList list = new ArrayList();

            foreach (Match match in result)
            {
                list.Add(match);
            }
            return list;

        }
        //извлечь индексы и размер строк
        public List<SubstringInfo> ExtractSubstringInfo(string input, string pattern)
        {
            MatchCollection result = Regex.Matches(input, pattern, RegexOptions.IgnoreCase);
            List<SubstringInfo> list = new List<SubstringInfo>();
            foreach (Match match in result)
            {
                SubstringInfo info = new SubstringInfo();
                info.Index = match.Index;
                info.Length = match.Length;
                list.Add(info);
            }
            return list;
        }
        #endregion

    }
    struct SubstringInfo
    {
        public int Index;
        public int Length;
    }
}

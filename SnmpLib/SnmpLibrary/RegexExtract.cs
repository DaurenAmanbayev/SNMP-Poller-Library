using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Collections;
using System.Threading.Tasks;

namespace SnmpLibrary
{
    class RegexExtract
    {
        private RegexExtract() { }
        //Singletone pattern
        private static RegexExtract _singletone;
        public static RegexExtract Singletone
        {
            get
            {
                if (_singletone == null)
                    _singletone = new RegexExtract();
                return _singletone;
            }
        }

        #region NODES
        //snmp community checking
        public string ParseCommunity(string input)
        {
            string pattern = @"(\S){1,}";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(input.Trim());
            return match.ToString();
        }       
        //replace pattern
        public string ReplacePattern(string input, string pattern, string replacement)
        {
            return Regex.Replace(input, pattern, replacement);
        }
        //extract list of address from text
        public List<IPAddress> ExtractIpAddress(string input)
        {           
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
                    list.Add(host);
                }
            }
            return list;
        }
        //extract address from text
        public string ExtractIP(string input)
        {
            string pattern = @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}";//clear ip retrieve

            Regex regex = new Regex(pattern);
            Match match = regex.Match(input.Trim());
            return match.ToString();
        }
        #endregion

        #region UTILS
        //list of matches by pattern
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
        //return index and string length from text by pattern
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

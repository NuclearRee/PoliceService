using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PoliceService.Tools
{
    public class IniFiles
    {
        private string filePath;
        private bool flag;
        public IniFiles(string filePath)
        {
            this.filePath = filePath;
        }
        #region 调用windowsAPI
        [DllImport("Kernel32.dll")]
        private static extern int GetPrivateProfileString(string strAppName, string strKeyName, string strDefault, StringBuilder sbReturnString, int nSize, string strFileName);
        [DllImport("Kernel32.dll")]
        private extern static int GetPrivateProfileStringA(string strAppName, string strKeyName, string sDefault, byte[] buffer, int nSize, string strFileName);
        [DllImport("Kernel32.dll")]
        private static extern int GetPrivateProfileInt(string strAppName, string strKeyName, int nDefault, string strFileName);
        //获取ini文件所有的section
        [DllImport("Kernel32.dll")]
        private extern static int GetPrivateProfileSectionNamesA(byte[] buffer, int iLen, string fileName);
        //获取指定Section的key和value        
        [DllImport("Kernel32.dll")]
        private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpReturnedString, int nSize, string lpFileName);
        //根据传入参数的不同进行写入或修改或删除操作（返回值 Long，非零表示成功，零表示失败）
        [DllImport("Kernel32.dll")]
        public static extern long WritePrivateProfileString(string strAppName, string strKeyName, string strKeyValue, string strFileName);
        //添加一个section内容列表
        [DllImport("Kernel32.dll")]
        public static extern long WritePrivateProfileSection(string strAppName, string strkeyandvalue, string strFileName);
        #endregion

        #region 供UI调用的方法
        //判断该ini文件是否存在如果不存在新建一个该文件
        public void FileExists()
        {
            try
            {
                if (!File.Exists(this.filePath))
                {

                    using (FileStream fs = File.Create(this.filePath))
                    {
                        fs.Close();
                    }
                }

            }
            catch (Exception e)
            {

            }
        }
        //返回该配置文件中所有Section名称的集合
        public ArrayList ReadSections()
        {
            byte[] buffer = new byte[65535];
            int rel = GetPrivateProfileSectionNamesA(buffer, buffer.GetUpperBound(0), this.filePath);
            int iCnt, iPos;
            ArrayList arrayList = new ArrayList();
            string tmp;
            if (rel > 0)
            {
                iCnt = 0; iPos = 0;
                for (iCnt = 0; iCnt < rel; iCnt++)
                {
                    if (buffer[iCnt] == 0x00)
                    {
                        tmp = System.Text.ASCIIEncoding.Default.GetString(buffer, iPos, iCnt - iPos).Trim();
                        iPos = iCnt + 1;
                        if (tmp != "")
                            arrayList.Add(tmp);
                    }
                }
            }
            return arrayList;
        }
        // 获取指定节点的所有KEY的名称
        public ArrayList ReadKeys(string sectionName)
        {

            byte[] buffer = new byte[5120];
            int rel = GetPrivateProfileStringA(sectionName, null, "", buffer, buffer.GetUpperBound(0), this.filePath);

            int iCnt, iPos;
            ArrayList arrayList = new ArrayList();
            string tmp;
            if (rel > 0)
            {
                iCnt = 0; iPos = 0;
                for (iCnt = 0; iCnt < rel; iCnt++)
                {
                    if (buffer[iCnt] == 0x00)
                    {
                        tmp = System.Text.ASCIIEncoding.Default.GetString(buffer, iPos, iCnt - iPos).Trim();
                        iPos = iCnt + 1;
                        if (tmp != "")
                            arrayList.Add(tmp);
                    }
                }
            }
            return arrayList;
        }

        //读取指定节点下的指定key的value返回string
        public string GetIniKeyValueForStr(string section, string key)
        {
            if (section.Trim().Length <= 0 || key.Trim().Length <= 0) return string.Empty;
            StringBuilder strTemp = new StringBuilder(256);
            GetPrivateProfileString(section, key, string.Empty, strTemp, 256, this.filePath);
            return strTemp.ToString().Trim();
        }
        //从指定的节点中获取一个整数值( Long，找到的key的值；如指定的key未找到，就返回默认值。如找到的数字不是一个合法的整数，函数会返回其中合法的一部分。如，对于“xyz=55zz”这个条目，函数返回55。)
        public int GetIniKeyValueForInt(string section, string key)
        {
            if (section.Trim().Length <= 0 || key.Trim().Length <= 0) return 0;
            return GetPrivateProfileInt(section, key, 0, this.filePath);
        }
        //读取指定节点下的所有key 和value
        public ArrayList GetIniSectionValue(string section)
        {
            byte[] buffer = new byte[5120];
            int rel = GetPrivateProfileSection(section, buffer, buffer.GetUpperBound(0), this.filePath);

            int iCnt, iPos;
            ArrayList arrayList = new ArrayList();
            string tmp;
            if (rel > 0)
            {
                iCnt = 0; iPos = 0;
                for (iCnt = 0; iCnt < rel; iCnt++)
                {
                    if (buffer[iCnt] == 0x00)
                    {
                        tmp = System.Text.ASCIIEncoding.Default.GetString(buffer, iPos, iCnt - iPos).Trim();
                        iPos = iCnt + 1;
                        if (tmp != "")
                            arrayList.Add(tmp);
                    }
                }
            }
            return arrayList;
        }
        //往指定section的key中写入value
        public bool WriteIniKey(string section, string key, string value)
        {
            try
            {
                if (section.Trim().Length <= 0 || key.Trim().Length <= 0 || value.Trim().Length <= 0)
                {
                    flag = false;
                }
                else
                {

                    if (WritePrivateProfileString(section, key, value, this.filePath) == 0)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
        //修改指定section的key的值
        public bool EditIniKey(string section, string key, string value)
        {
            try
            {
                if (section.Trim().Length <= 0 || key.Trim().Length <= 0 || value.Trim().Length <= 0)
                {
                    flag = false;
                }
                else
                {
                    if (WritePrivateProfileString(section, key, value, this.filePath) == 0)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
        //删除指定section的指定key
        public bool DeleteIniKey(string section, string key)
        {
            try
            {
                if (section.Trim().Length <= 0 || key.Trim().Length <= 0)
                {
                    flag = false;
                }
                else
                {
                    if (WritePrivateProfileString(section, key, null, this.filePath) == 0)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
        //删除指定section
        public bool DeleteIniSection(string section)
        {
            try
            {
                if (section.Trim().Length <= 0)
                {
                    flag = false;
                }
                else
                {
                    if (WritePrivateProfileString(section, null, null, this.filePath) == 0)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
        //给一个节点写入key和value列表
        public bool WriteIniSectionAndValue(string section, Hashtable ht)
        {
            string lpString = "";
            try
            {
                if (section.Trim().Length <= 0 || ht.Count == 0)
                {
                    flag = false;
                }
                else
                {
                    foreach (DictionaryEntry de in ht)
                    {
                        lpString += de.Key + "=" + de.Value;
                        lpString += "\r\n";
                    }
                    if (WritePrivateProfileSection(section, lpString, this.filePath) == 0)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }

                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
        //给一个节点写入key 列表
        public bool WriteIniSectionName(string section, List<string> lstKeyValue)
        {
            string lpString = "";
            try
            {
                if (section.Trim().Length <= 0 || lstKeyValue.Count == 0)
                {
                    flag = false;
                }
                else
                {
                    for (int i = 0; i < lstKeyValue.Count; ++i)
                    {
                        lpString += lstKeyValue[i];
                        lpString += "\r\n";
                    }
                    if (WritePrivateProfileSection(section, lpString, this.filePath) == 0)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }

                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
        #endregion
    }
}

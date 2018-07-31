using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoliceService.Tools
{ /// <summary>
  /// Base64 的摘要说明。
  /// </summary>
    public class Base64
    {
        /// <summary>
        /// 字符串转Base64位码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string StringToBase64(string data)
        {
            byte[] bytes = Encoding.Default.GetBytes(data);
            string str = Convert.ToBase64String(bytes);
            return str;
        }

        public string Base64ToString(string data)
        {
            byte[] outputb = Convert.FromBase64String(data);
            string orgStr = Encoding.Default.GetString(outputb);
            return orgStr;
        }

    }
}

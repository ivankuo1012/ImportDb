using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Net.Http;
using System.IO;
using System.Net.Mail;
using System.Security.Cryptography;

namespace ImportDb
{
    class Program
    {
        static void Main(string[] args)
        {
            
            const string sEncrKey = "8F916052C251C0B325318B1F3F2737EF";
            string sDataSource = System.Configuration.ConfigurationManager.AppSettings.Get("DataSource");
            string sCatalog = System.Configuration.ConfigurationManager.AppSettings.Get("Catalog");
            string sDbUser = System.Configuration.ConfigurationManager.AppSettings.Get("DbUser");
            string sDbPassword = DecryptStr(System.Configuration.ConfigurationManager.AppSettings.Get("DbPassword"), sEncrKey);

            string sPath = ".\\txt_files";
            string sFileNameEmpList = "NET_HRIS_EMP";
            string sFileNameUniList = "UNI_RES";

            string consString = "data source=" + sDataSource + "; initial catalog = " + sCatalog + "; user id = " + sDbUser + "; password = " + sDbPassword + "";

            

            SqlConnection conn = new SqlConnection(consString);

            //SqlConnection conn = new SqlConnection("data source=.\\SQLExpress; initial catalog = FUBON_DLP; user id = fubon_dlp; password = 1234");
            conn.Open();
            try
            {
               
               
                string[] dirs = Directory.GetFiles(sPath, sFileNameEmpList+"*");
                //Console.WriteLine("The number of files starting with c is {0}.", dirs.Length);
                if(dirs == null)
                {
                    Console.WriteLine("no file");
                }
                else
                {
                    /*
                    string sSqlCmdTruncate = "truncate table NET_HRIS_EMP";
                    SqlCommand cmdTruncate = new SqlCommand(sSqlCmdTruncate, conn);
                    cmdTruncate.ExecuteReader();
                    */
                    string csvPath = dirs[0];
                    Console.WriteLine(Path.GetFileName(dirs[0]));
                    string csvData = System.IO.File.ReadAllText(csvPath);
                    string sSqlInsert = "";
                    foreach (string row in csvData.Split('\n'))
                    {
                        if (!string.IsNullOrEmpty(row))
                        {

                           // Console.WriteLine(row);
                            int i = 0;
                            string[] cell = row.Split(',');
                            //Console.WriteLine(cell[2]+","+cell[5]+","+cell[19]+","+cell[27]);
                            sSqlInsert = $"INSERT INTO NET_HRIS_EMP values ('{cell[2]}','{cell[5]}','{cell[19]}','{cell[27]}')";
                            Console.WriteLine(sSqlInsert);
                            
                        }
                    }
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }

        }
         public static string DecryptStr(string SourceStr, string CryptoKey)
        {
            string decrypt = "";
            try
            {
                AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
                byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
                aes.Key = key;
                aes.IV = iv;

                byte[] dataByteArray = Convert.FromBase64String(SourceStr);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(dataByteArray, 0, dataByteArray.Length);
                        cs.FlushFinalBlock();
                        decrypt = Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return decrypt;
        }
    }
}

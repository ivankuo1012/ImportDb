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
using System.Runtime.CompilerServices;

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
                
               
                string[] EmpListdirs = Directory.GetFiles(sPath, sFileNameEmpList+"*");
                if(EmpListdirs == null || EmpListdirs.Length == 0)
                {
                    Console.WriteLine("no file");
                }
                else
                {
                    /*員工清單*/
                    string sSqlCmdTruncate = "truncate table " + sFileNameEmpList;
                    SqlCommand sqlTruncate = new SqlCommand(sSqlCmdTruncate, conn);
                   
                    sqlTruncate.ExecuteNonQuery();

                    string EmpListCsvPath = EmpListdirs[0];
                    Console.WriteLine(Path.GetFileName(EmpListdirs[0]));
                    string EmpListCsvData = System.IO.File.ReadAllText(EmpListCsvPath);
                    string sSqlInsert = "";
                    int i = 0;
                    foreach (string row in EmpListCsvData.Split('\n'))
                    {
                        if (!string.IsNullOrEmpty(row) & i!=0)
                        {
                            string[] cell = row.Split(',');
                            sSqlInsert += $"INSERT INTO { sFileNameEmpList } values ('{cell[2]}','{cell[5]}','{cell[19]}','{cell[27]}');";
                            
                        }
                        i++;
                    }
                    SqlCommand sqlInsert = new SqlCommand(sSqlInsert, conn);

                    int numberOfRecords = sqlInsert.ExecuteNonQuery();
                    Console.WriteLine(sFileNameEmpList+ " count: " + numberOfRecords);
                    File.Delete(EmpListCsvPath);

                   

                    
                }
                string[] UniListdirs = Directory.GetFiles(sPath, sFileNameUniList + "*");
                if (UniListdirs == null || UniListdirs.Length == 0)
                {
                    Console.WriteLine("no file");
                }
                else
                {
                    /*員工清單*/
                    string sSqlCmdTruncate = "truncate table " + sFileNameUniList;
                    SqlCommand sqlTruncate = new SqlCommand(sSqlCmdTruncate, conn);

                    sqlTruncate.ExecuteNonQuery();

                    string UniListCsvPath = UniListdirs[0];
                    Console.WriteLine(Path.GetFileName(UniListdirs[0]));
                    string UniListCsvData = System.IO.File.ReadAllText(UniListCsvPath);
                    string sSqlInsert = "";
                    int i = 0;
                    foreach (string row in UniListCsvData.Split('\n'))
                    {
                        // Console.WriteLine(row);
                        if (!string.IsNullOrEmpty(row) & i != 0)
                        {
                            string[] cell = row.Split(',');
                            sSqlInsert += $"INSERT INTO {sFileNameUniList} values ('{cell[0]}','{cell[1]}','{cell[2]}');";

                        }
                        i++;
                    }
                    SqlCommand sqlInsert = new SqlCommand(sSqlInsert, conn);

                    int numberOfRecords = sqlInsert.ExecuteNonQuery();
                    Console.WriteLine(sFileNameUniList + "count: " + numberOfRecords);
                    File.Delete(UniListCsvPath);







                    conn.Close();
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

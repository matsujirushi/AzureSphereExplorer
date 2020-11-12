using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace AzureSphereExplorer
{
    class CSVParser
    {
        public static List<string> Parse(string FilePath)
        {
            List<string> deviceIdList = new List<string>();
            try
            {
                StreamReader sr = new StreamReader(FilePath);
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] values = line.Split(',');
                    if (values[0].Length > 0) {
                        deviceIdList.Add(values[0].ToLower());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                MessageBox.Show("Load CSV file is failure.",
                    "Error", MessageBoxButtons.OK);
                return null;
            }
            return deviceIdList;
        }
    }
}

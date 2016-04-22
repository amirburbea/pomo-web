using System;
using System.IO;

namespace PoMo.Data
{
    public static class ConnectionStringMethods
    {
        public static string GetConnectionString()
        {
            string fileName;
            return ConnectionStringMethods.GetConnectionString(out fileName);
        }

        public static string GetConnectionString(out string fileName)
        {
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "PoMo");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return string.Concat("Data Source=\"", fileName = Path.Combine(directory, "db.sdf"), "\";Max Database Size=256;Persist Security Info=False");
        }
    }
}
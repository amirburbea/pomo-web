using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoMo.Data;
using PoMo.Data.Models;

namespace PoMo.Loader
{
    internal static class Program
    {
        private static readonly Portfolio[] _portfolios =
        {
            new Portfolio { Id = "BC", Name = "Blue Chips" },
            new Portfolio { Id = "CP", Name = "CP" },
            new Portfolio { Id = "HEDGE", Name = "Hedges" },
            new Portfolio { Id = "PORTACT", Name = "Port Accounts" },
            new Portfolio { Id = "SPEC", Name = "Special Sits" }
        };

        private static readonly string[] _tickers =
        {
            "GOOG", "MDSO", "MSFT", "TSLA", "T",
            "VZ", "IBM", "HPQ", "INTC", "BAC",
            "WFM", "DG", "TWC", "LVS", "WYNN",
            "RGC", "F", "GM", "DB", "JPM",
            "RBS", "DNKN", "PRU", "GS", "CAT",
            "GRMN", "AAPL", "HNZ", "UPS", "FCAU",
            "FB", "ALL", "ORCL", "RAD", "HD", "DAL",
            "CMCSA"
        };

        private static int ComparePropertyNames(string left, string right)
        {
            if (left == right)
            {
                return 0;
            }
            if (left == "Id")
            {
                return -1;
            }
            if (right == "Id")
            {
                return 1;
            }
            bool leftEndsWithId = left.EndsWith("Id", StringComparison.Ordinal);
            if (leftEndsWithId != right.EndsWith("Id", StringComparison.Ordinal))
            {
                return leftEndsWithId ? -1 : 1;
            }
            return string.Compare(left, right, StringComparison.Ordinal);
        }

        private static void CreateDatabase()
        {
            string fileName;
            string connectionString = ConnectionStringMethods.GetConnectionString(out fileName);
            using (File.Create(fileName))
            {
                //SqlCe is happy to work with a 0 byte file.
            }
            IComparer<string> propertyNameComparer = Comparer<string>.Create(Program.ComparePropertyNames);
            using (SqlCeConnection connection = new SqlCeConnection(connectionString))
            {
                connection.Open();
                foreach (PropertyInfo property in typeof(DataContext).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(property => property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)))
                {
                    Program.CreateTable(connection, property.PropertyType.GetGenericArguments()[0], propertyNameComparer);
                }
            }
        }

        private static void CreateTable(SqlCeConnection connection, Type type, IComparer<string> propertyNameComparer)
        {
            StringBuilder builder = new StringBuilder()
                .Append("CREATE TABLE [").Append(type.Name).Append(']').AppendLine()
                .Append('(').AppendLine();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => !property.PropertyType.IsClass || property.PropertyType == typeof(string))
                .OrderBy(property => property.Name, propertyNameComparer)
                .ToArray();
            for (int index = 0; index < properties.Length; index++)
            {
                PropertyInfo property = properties[index];
                builder.Append("\t[").Append(property.Name).Append("] ");
                Type dataType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                switch (Type.GetTypeCode(dataType))
                {
                    case TypeCode.Int32:
                        builder.Append("INT");
                        if (property.Name == nameof(IdentityObjectBase.Id))
                        {
                            builder.Append(" IDENTITY(1, 1)");
                        }
                        break;
                    case TypeCode.DateTime:
                        builder.Append("DATETIME");
                        break;
                    case TypeCode.Double:
                    case TypeCode.Single:
                    case TypeCode.Decimal:
                        builder.Append("DECIMAL(18,8)");
                        break;
                    case TypeCode.String:
                        builder.Append("NVARCHAR(255)");
                        break;
                }
                if (!dataType.IsClass && dataType == property.PropertyType ||
                    property.Name.EndsWith("Id", StringComparison.Ordinal))
                {
                    builder.Append(" NOT NULL");
                }
                if (property.Name == nameof(IdentityObjectBase.Id))
                {
                    builder.Append(" PRIMARY KEY");
                }
                if (index != properties.Length - 1)
                {
                    builder.Append(',');
                }
                builder.AppendLine();
            }
            builder.Append(')');
            using (SqlCeCommand command = connection.CreateCommand())
            {
                command.CommandText = builder.ToString();
                command.ExecuteNonQuery();
            }
        }

        private static void Main()
        {
            Program.CreateDatabase();
            string uri = "http://www.google.com/finance/info?infotype=infoquoteall&q=" + string.Join(",", Program._tickers);
            HttpWebRequest request = WebRequest.CreateHttp(uri);
            string text;
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    text = reader.ReadToEnd().Trim(' ', '/', '\r', '\n');
                }
            }
            JArray jArray = JsonConvert.DeserializeObject<JArray>(text);
            Security[] securities = new Security[jArray.Count];
            for (int index = 0; index < jArray.Count; index++)
            {
                JToken item = jArray[index];
                securities[index] = new Security
                {
                    Description = (string)item["name"],
                    Ticker = (string)item["t"],
                    OpeningPrice = (decimal)item["l"]
                };
            }
            using (DataContext context = new DataContext())
            {
                foreach (Portfolio portfolio in Program._portfolios)
                {
                    context.Portfolios.Add(portfolio);
                }
                Random random = new Random(Guid.NewGuid().ToByteArray().Sum(x => x));
                DateTime tradeDate = DateTime.Now;
                foreach (Security item in securities)
                {
                    Security security = context.Securities.Add(item);
                    foreach (Portfolio portfolio in Program._portfolios)
                    {
                        bool hasPosition = random.Next(2) == 1;
                        if (!hasPosition)
                        {
                            continue;
                        }
                        int quantity = random.Next(100, 5000);
                        bool isShort = random.Next(2) == 1;
                        context.Trades.Add(new Trade
                        {
                            Security = security,
                            SecurityId = security.Id,
                            PortfolioId = portfolio.Id,
                            Price = security.OpeningPrice,
                            Quantity = (isShort ? -1 : 1) * quantity,
                            TradeDate = tradeDate
                        });
                    }
                }
                context.SaveChanges();
            }
        }
    }
}
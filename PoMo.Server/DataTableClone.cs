using System;
using System.Data;

namespace PoMo.Server
{
    public static class DataTableClone
    {
        public static DataTable FullClone(this DataTable dataTable)
        {
            if (dataTable == null)
            {
                throw new ArgumentNullException(nameof(dataTable));
            }
            DataTable clone = dataTable.Clone();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                clone.Rows.Add(dataRow.ItemArray);
            }
            clone.AcceptChanges();
            return clone;
        }
    }
}
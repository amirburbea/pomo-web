using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PoMo.Client
{
    internal sealed class ReadOnlyDataRowCollectionWrapper : IReadOnlyList<DataRow>, ICollection<DataRow>, IList<DataRow>
    {
        private readonly DataRowCollection _dataRowCollection;

        public ReadOnlyDataRowCollectionWrapper(DataRowCollection dataRowCollection)
        {
            this._dataRowCollection = dataRowCollection;
        }

        public int Count => this._dataRowCollection.Count;

        bool ICollection<DataRow>.IsReadOnly => true;

        DataRow IList<DataRow>.this[int index]
        {
            get
            {
                return this._dataRowCollection[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public DataRow this[int index] => this._dataRowCollection[index];

        public bool Contains(DataRow item)
        {
            switch (item.Table.PrimaryKey.Length)
            {
                case 0:
                    return this._dataRowCollection.IndexOf(item) != -1;
                case 1:
                    return this._dataRowCollection.Contains(item[item.Table.PrimaryKey[0]]);
            }
            return this._dataRowCollection.Contains(Array.ConvertAll(item.Table.PrimaryKey, column => item[column]));
        }

        public void CopyTo(DataRow[] array, int arrayIndex)
        {
            this._dataRowCollection.CopyTo(array, arrayIndex);
        }

        public IEnumerator<DataRow> GetEnumerator()
        {
            return this._dataRowCollection.Cast<DataRow>().GetEnumerator();
        }

        void ICollection<DataRow>.Add(DataRow item)
        {
            throw new NotSupportedException();
        }

        void ICollection<DataRow>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<DataRow>.Remove(DataRow item)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._dataRowCollection.GetEnumerator();
        }

        void IList<DataRow>.Insert(int index, DataRow item)
        {
            throw new NotSupportedException();
        }

        void IList<DataRow>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public int IndexOf(DataRow item)
        {
            return this._dataRowCollection.IndexOf(item);
        }
    }
}
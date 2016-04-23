using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace PoMo.Client.DataBoundObjects
{
    public sealed class DataBoundObjectCollection : List<DataBoundObject>, IBindingList, IRaiseItemChangedEvents, ITypedList, IDisposable
    {
        private readonly DataTable _dataTable;
        private readonly Dictionary<DataRow, int> _rowIndices;

        public DataBoundObjectCollection(DataTable dataTable)
        {
            this._dataTable = dataTable;
            PropertyDescriptor[] properties = new PropertyDescriptor[dataTable.Columns.Count];
            for (int index = 0; index < dataTable.Columns.Count; index++)
            {
                DataColumn column = dataTable.Columns[index];
                properties[index] = new ColumnPropertyDescriptor(
                    column.ColumnName,
                    index,
                    !column.AllowDBNull || !column.DataType.IsValueType ? column.DataType : typeof(Nullable<>).MakeGenericType(column.DataType),
                    string.IsNullOrEmpty(column.Caption) ? column.ColumnName : column.Caption
                );
            }
            this.Properties = new PropertyDescriptorCollection(properties, true);
            this._rowIndices = new Dictionary<DataRow, int>(dataTable.Rows.Count);
            foreach (DataRow row in dataTable.Rows)
            {
                object[] values = new object[row.ItemArray.Length];
                Array.Copy(row.ItemArray, values, values.Length);
                this.Add(new DataBoundObject(this, values));
                this._rowIndices.Add(row, this._rowIndices.Count);
            }
            this.RaisesItemChangedEvents = true;
            this.CustomTypeDescriptor = new DataBoundTypeDescriptor(this.Properties);
            this._dataTable.TableNewRow += this.DataTable_TableNewRow;
            this._dataTable.TableCleared += this.DataTable_TableCleared;
            this._dataTable.RowDeleted += this.DataTable_RowDeleted;
            this._dataTable.ColumnChanged += this.DataTable_ColumnChanged;
        }

        public event ListChangedEventHandler ListChanged;

        public ICustomTypeDescriptor CustomTypeDescriptor
        {
            get;
        }

        bool IBindingList.AllowEdit => false;

        bool IBindingList.AllowNew => false;

        bool IBindingList.AllowRemove => false;

        bool IBindingList.IsSorted => false;

        ListSortDirection IBindingList.SortDirection => 0;

        PropertyDescriptor IBindingList.SortProperty => null;

        bool IBindingList.SupportsChangeNotification => true;

        bool IBindingList.SupportsSearching => false;

        bool IBindingList.SupportsSorting => false;

        public bool RaisesItemChangedEvents
        {
            get;
            set;
        }

        internal PropertyDescriptorCollection Properties
        {
            get;
        }

        public void Dispose()
        {
            this._dataTable.TableNewRow -= this.DataTable_TableNewRow;
            this._dataTable.TableCleared -= this.DataTable_TableCleared;
            this._dataTable.ColumnChanged -= this.DataTable_ColumnChanged;
            this._dataTable.RowDeleted -= this.DataTable_RowDeleted;
        }

        void IBindingList.AddIndex(PropertyDescriptor property)
        {
            throw new NotSupportedException();
        }

        object IBindingList.AddNew()
        {
            throw new NotSupportedException();
        }

        void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            throw new NotSupportedException();
        }

        int IBindingList.Find(PropertyDescriptor property, object key)
        {
            throw new NotSupportedException();
        }

        void IBindingList.RemoveIndex(PropertyDescriptor property)
        {
            throw new NotSupportedException();
        }

        void IBindingList.RemoveSort()
        {
            throw new NotSupportedException();
        }

        PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            return this.Properties;
        }

        string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
        {
            return null;
        }

        private void DataTable_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            int rowIndex;
            if (this._rowIndices.TryGetValue(e.Row, out rowIndex) && 
                this[rowIndex].OnPropertyChanged(e.Column.Ordinal, e.ProposedValue) &&
                this.RaisesItemChangedEvents)
            {
                this.OnListChanged(ListChangedType.ItemChanged, rowIndex, this.Properties[e.Column.Ordinal]);
            }
        }

        private void DataTable_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            int rowIndex;
            if (!this._rowIndices.TryGetValue(e.Row, out rowIndex))
            {
                return;
            }
            this.RemoveAt(rowIndex);
            this._rowIndices.Remove(e.Row);
            for (int index = rowIndex; index < this._dataTable.Rows.Count; index++)
            {
                DataRow row = this._dataTable.Rows[index];
                this._rowIndices[row] = index;
            }
            this.OnListChanged(ListChangedType.ItemDeleted, rowIndex);
        }

        private void DataTable_TableCleared(object sender, DataTableClearEventArgs e)
        {
            this.Clear();
            this._rowIndices.Clear();
            this.OnListChanged(ListChangedType.Reset, -1);
        }

        private void DataTable_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            int rowIndex = e.Row.Equals(this._dataTable.Rows[this._dataTable.Rows.Count - 1]) ? this._dataTable.Rows.Count - 1 : this._dataTable.Rows.IndexOf(e.Row);
            object[] values = new object[e.Row.ItemArray.Length];
            Array.Copy(e.Row.ItemArray, values, values.Length);
            this.Insert(rowIndex, new DataBoundObject(this, values));
            this._rowIndices.Add(e.Row, rowIndex);
            for (int index = rowIndex + 1; index < this._dataTable.Rows.Count; index++)
            {
                DataRow row = this._dataTable.Rows[index];
                this._rowIndices[row] = index;
            }
            this.OnListChanged(ListChangedType.ItemAdded, rowIndex);
        }

        private void OnListChanged(ListChangedType listChangedType, int rowIndex, PropertyDescriptor propertyDescriptor = null)
        {
            this.ListChanged?.Invoke(this, new ListChangedEventArgs(listChangedType, rowIndex, propertyDescriptor));
        }

        private sealed class ColumnPropertyDescriptor : PropertyDescriptor
        {
            private readonly int _ordinal;

            public ColumnPropertyDescriptor(string name, int ordinal, Type propertyType, string displayName)
                : base(name, null)
            {
                this._ordinal = ordinal;
                this.PropertyType = propertyType;
                this.Attributes = new AttributeCollection(new DisplayNameAttribute(displayName));
            }

            public override AttributeCollection Attributes
            {
                get;
            }

            public override Type ComponentType => typeof(DataBoundObject);

            public override bool IsReadOnly => true;

            public override Type PropertyType
            {
                get;
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override object GetValue(object component)
            {
                return ((DataBoundObject)component).GetValue(this._ordinal);
            }

            public override void ResetValue(object component)
            {
                throw new NotSupportedException();
            }

            public override void SetValue(object component, object value)
            {
                throw new NotSupportedException();
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }
        }

        private sealed class DataBoundTypeDescriptor : CustomTypeDescriptor
        {
            private readonly PropertyDescriptorCollection _properties;

            public DataBoundTypeDescriptor(PropertyDescriptorCollection properties)
            {
                this._properties = properties;
            }

            public override PropertyDescriptorCollection GetProperties()
            {
                return this._properties;
            }

            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                return this._properties;
            }
        }
    }
}
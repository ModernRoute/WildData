using System;

namespace ModernRoute.WildData.Helpers
{
    public class ColumnDescriptor
    {
        public int ColumnIndex
        {
            get;
            private set;
        }

        public ColumnInfo ColumnInfo
        {
            get;
            private set;
        }

        public ColumnDescriptor(int columnIndex, ColumnInfo columnInfo)
        {
            if (columnInfo == null)
            {
                throw new ArgumentNullException(nameof(columnInfo));
            }

            ColumnIndex = columnIndex;
            ColumnInfo = columnInfo;
        }
    }
}

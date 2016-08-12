namespace ModernRoute.WildData.Core
{
    class ColumnMemberInfo
    {
        public string MemberName
        {
            get;
            private set;
        }

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

        public ColumnMemberInfo(string memberName, int columnIndex, ColumnInfo columnInfo)
        {
            MemberName = memberName;
            ColumnIndex = columnIndex;
            ColumnInfo = columnInfo;
        }
    }
}

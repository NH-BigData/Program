namespace Query
{
    public abstract class QueryBase
    {
        protected const string DYNAMIC_STR = "#Dynamic#";

        public const string SQL_TYPE_SELECT = "_SELECT";
        public const string SQL_TYPE_INSERT = "_INSERT";
        public const string SQL_TYPE_UPDATE = "_UPDATE";
        public const string SQL_TYPE_DELETE = "_DELETE";
        public const string SQL_TYPE_PROCEDURE = "_PROCEDURE";

        public QueryBase()
        {
            SetSQL();
        }

        public string sqlText { get; set; }
        public string sqlType { get; set; }

        protected abstract void SetSQL();

        public void ApplyDynamicString(string value)
        {
            sqlText = sqlText.Replace(DYNAMIC_STR, value);
        }
    }
}

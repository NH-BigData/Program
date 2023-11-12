namespace Query
{
    public abstract class QueryCRUDBase
    {
        public bool isAuditTable = true;

        public QueryCRUDBase()
        {
            SetInsertSQL();
            SetSelectSQL();
            SetUpdateSQL();
            SetDeleteSQL();
        }

        public string insertSqlText { get; set; }
        public string selectSqlText { get; set; }
        public string updateSqlText { get; set; }
        public string deleteSqlText { get; set; }

        protected abstract void SetInsertSQL();
        protected abstract void SetSelectSQL();
        protected abstract void SetUpdateSQL();
        protected abstract void SetDeleteSQL();
    }
}

using Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Handler.DataBaseHandler
{

    public class SQLcrud : SQLBase
    {
        public SQLcrud(QueryCRUDBase query) : this(DBHandler.GetInstanse(), query)
        {
        }

        public SQLcrud(DBHandler handler, QueryCRUDBase query)
            : base(handler)
        {
            insertSqlText = query.insertSqlText;
            selectSqlText = query.selectSqlText;
            updateSqlText = query.updateSqlText;
            deleteSqlText = query.deleteSqlText;
        }

        public string insertSqlText { get; set; }
        public string selectSqlText { get; set; }
        public string updateSqlText { get; set; }
        public string deleteSqlText { get; set; }

        public DataTable ReadUseTranSactionToDataTable(object[] args)
        {
            if (args == null) args = new object[0];

            var ret = new DataTable();
            try
            {
                handler.OpenTransaction();
                var temp = Read(args);
                ret = handler.ReaderToDataTable(temp);

                if (temp != null)
                { 
                    while(!temp.IsClosed) temp.Close();
                    temp.Dispose();
                }

                handler.CommitTransaction();
            }
            catch (Exception ex)
            {
                handler.RollBackTransaction();
                MessageBox.Show(ex.StackTrace);
            }
            return ret;
        }

        public DataTable ReadUseNotTransactionToDataTable(object[] args)
        { 
            if(args == null) args = new object[0];

            var ret = new DataTable();
            try
            {
                var temp = Read(args);
                ret = handler.ReaderToDataTable(temp);
                if (!temp.IsClosed) temp.Close();
                temp.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
            return ret;
        }

        protected DbDataReader Read(object[] args)
        {
            try
            {
                return Read(handler.CreateCommand(selectSqlText, args));
            }
            catch (Exception ex)
            {
                ex = new Exception(new StringBuilder(ex.Message).ToString(), ex);
                throw ex;
            }
        }

        protected DbDataReader Read(DbCommand command)
        {
            try
            {
                var reader = command.ExecuteReader();
                command.Dispose();
                return reader;
            }
            catch (Exception ex)
            {
                if (null != command)
                    ex = new Exception(new StringBuilder(command.CommandText).AppendLine(ex.Message).ToString(), ex);
                throw ex;
            }
        }

        //TODO 현재 프로젝트는 Read만 필요한 상태 이후 필요하면 update insert delete 추가할 것
    }
}

using Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }


}

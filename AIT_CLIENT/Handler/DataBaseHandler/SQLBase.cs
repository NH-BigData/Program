namespace Handler.DataBaseHandler
{
    public class SQLBase
    {
        protected DBHandler handler;
        public SQLBase(DBHandler handler)
        {
            this.handler = handler;
        }

        public SQLBase() : this(DBHandler.GetInstanse())
        { 
        }
    }
}

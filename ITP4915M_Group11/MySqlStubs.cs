using System;
using System.Data;

// 注意：這些是為了編譯方便而新增的最小 stub 類別。
// 真正執行時應該安裝官方 MySql.Data/Oracle MySql Connector 套件並移除此檔案。
namespace MySql.Data.MySqlClient
{
    public class MySqlConnection : IDisposable
    {
        private readonly string _conn;
        public MySqlConnection(string connString)
        {
            _conn = connString;
        }

        public void Open() { }

        public MySqlTransaction BeginTransaction()
        {
            return new MySqlTransaction();
        }

        public void Dispose() { }
    }

    public class MySqlTransaction : IDisposable
    {
        public void Commit() { }
        public void Rollback() { }
        public void Dispose() { }
    }

    public class MySqlDataAdapter : IDisposable
    {
        public MySqlDataAdapter(string selectCommandText, MySqlConnection conn) { }
        public MySqlDataAdapter(MySqlCommand cmd) { }
        public int Fill(DataTable table) { return 0; }
        public void Dispose() { }
    }

    public class MySqlDataReader : IDisposable
    {
        public bool Read() { return false; }
        public object this[string name] { get { return null; } }
        public void Dispose() { }
    }

    public class MySqlCommand : IDisposable
    {
        private readonly string _cmd;
        public MySqlCommand(string cmdText, MySqlConnection conn) { _cmd = cmdText; Parameters = new MySqlParameterCollection(); }
        public MySqlCommand(string cmdText, MySqlConnection conn, MySqlTransaction trans) { _cmd = cmdText; Parameters = new MySqlParameterCollection(); }

        public MySqlParameterCollection Parameters { get; }

        public int ExecuteNonQuery() { return 0; }

        public MySqlDataReader ExecuteReader() { return new MySqlDataReader(); }

        public object ExecuteScalar() { return 0; }

        public void Dispose() { }
    }

    public class MySqlParameterCollection
    {
        public void AddWithValue(string name, object value) { }
    }
}

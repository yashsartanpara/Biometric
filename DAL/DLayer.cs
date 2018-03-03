using System;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace DAL
{
    public class DLayer
    {
        MySqlConnection get_connection()
        {
            //string connectionString = @"server=localhost;database=biometric_attendance;uid=root;password='';";
            //string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            string connectionString = ConfigurationManager.AppSettings["connectionString"].ToString();

            MySqlConnection conn = new MySqlConnection(connectionString);
            return conn;
        }
        public DataSet get_data(string query)
        {
            MySqlConnection conn = get_connection();
            conn.Open();
            if (conn != null && conn.State == ConnectionState.Open)
            {
                DataSet ds = new DataSet();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(ds);
                conn.Close();
                return ds;
            }
            else
                return null;
        }

        public int get_id(string query)
        {
            MySqlConnection conn = get_connection();
            conn.Open();
            if (conn != null && conn.State == ConnectionState.Open)
            {
                DataSet ds = new DataSet();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(ds);
                conn.Close();
                if (ds.Tables[0].Rows.Count != 0)
                {
                    string str = ds.Tables[0].Rows[0][0].ToString();
                    int id = Convert.ToInt32(str);
                    return id;
                }
            }
            return -1;
        }
        public int insert_update_data(string query)
        {
            MySqlConnection conn = get_connection();
            conn.Open();
            if (conn != null && conn.State == ConnectionState.Open)
            {
                int i;
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = query;
                i = cmd.ExecuteNonQuery();
                conn.Close();
                return i;
            }
            else
                return -1;
        }
    }
}

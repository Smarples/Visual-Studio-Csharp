using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToDoList
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new ToDoListForm());
            Application.Run(new LoginFormcs());
        }

    }
    //Class for public methods usually to do with interacting with the database
    public class methods 
    {
        public static SqlConnection con = new SqlConnection();
        public static SqlCommand command;

        //Methods to open and close the Connection to the database 
        public static void OpenConnection(string sql)
        {
            try
            {
                con.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Admin\\source\\repos\\ToDoList\\ToDoList\\ToDoList.mdf;Integrated Security=True";
                con.Open();
                //Run the query
                command = new SqlCommand(sql, con);
            }
            catch { }

        }
        public static void CloseConnection()
        {
            //Close the database
            command.Dispose();
            con.Close();
        }

        //Fills a Datatable with the requested data for display
        public static DataTable reloadData(string sql)
        {
            DataTable dt = null;
            OpenConnection(sql);
            try
            {
                SqlDataAdapter Adapter = new SqlDataAdapter(sql, con);
                dt = new DataTable();
                Adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Occured");
            }


            CloseConnection();

            return dt;
        }

        //Deletes the requested information from the database
        public static void Delete(string sql)
        {
            OpenConnection(sql);
            command.ExecuteNonQuery();
            CloseConnection();
        }

        //Fetches single piece of data from database such as UserID or ListID
        public static string FetchSingle(string sql)
        {
            OpenConnection(sql);
            string single = null;

            try
            {
                single = command.ExecuteScalar().ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


            CloseConnection();
            return single;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToDoList
{
    public partial class AddListForm : Form
    {
        public AddListForm()
        {
            InitializeComponent();
        }



        //Closes the form
        private void CancelButton_Click(object sender, EventArgs e)
        {
            //Closes the Form
            Hide();
        }

        //Adds a list to the database in the current location
        private void NewListButton_Click(object sender, EventArgs e)
        {

            //Check a value has been entered to create a list.
            string name = ListNameTextBox.Text;
            string parent = ParentListLabel.Text;
            string parentID = ToDoListForm.ParentID;
            string UUID = Guid.NewGuid().ToString();

            string count = methods.FetchSingle("Select Count(*) From Lists Where ListName = '" + name + "' AND ListParent = '" + parentID + "'");

            if (name == "")
            {
                MessageBox.Show("Please Enter a Name for your To Do List!", "Title Required");

            } else if (count == "0")
            {
                try
                {
                    //Set variables to open and query into the database
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sql = "";

                    if (parent == "Home")
                    {
                        //Query string to insert the information into the correct table
                        sql = "Insert into Lists (ListName, ListID) values('" + name + "', '" + UUID + "')";
                    }
                    else
                    {
                        //pass through the parent id because right now it doesnt have enough info in this form.
                        sql = "Insert into Lists (ListName, ListParent, ListID) values('" + name + "', '" + parentID + "', '" + UUID + "')";
                    }

                    //Run the query
                    methods.OpenConnection(sql);

                    methods.command = new SqlCommand(sql, methods.con);
                    adapter.InsertCommand = new SqlCommand(sql, methods.con);
                    adapter.InsertCommand.ExecuteNonQuery();

                    //Close the database and the AddList Form.
                    methods.CloseConnection();
                    Hide();
                }

                //Displays an error message when there is an SqlException
                catch (SqlException ex)
                {
                    //Error code for duplicate primary keys being entered. 
                    if (ex.Number == 2627)
                    {
                        MessageBox.Show("Please ensure this is a unique list name.", "Error Entering Data.");
                    }
                    else
                    {
                        MessageBox.Show(ex.Message, "Error Code: " + ex.Number.ToString());
                    }
                }
            } else
            {
                MessageBox.Show("You have another list with this name in this list. Please enter a unique name!", "Error Creating List.");
            }       
        }
    }
}

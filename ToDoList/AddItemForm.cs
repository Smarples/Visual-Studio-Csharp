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
    public partial class AddItemForm : Form
    {
        
        string sql = "";

        public AddItemForm()
        {
            InitializeComponent();

            //Sets the lists they can add a task to to more relevant lists and not display all options
            sql = "Select * from Lists Where ListID = '" + ToDoListForm.ParentID + "' OR ListParent = '" + ToDoListForm.ParentID + "'";

            methods.OpenConnection(sql);

            SqlDataAdapter adapter = new SqlDataAdapter(sql, methods.con);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            ListNameComboBox.DataSource = dt;
            methods.CloseConnection();

            string name = methods.FetchSingle("Select ListName From Lists Where ListID = '" + ToDoListForm.ParentID + "'");
            ListNameComboBox.Text = name;
            
        }

        private void AddItemForm_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'toDoListDataSet2.Lists' table. You can move, or remove it, as needed.
            this.listsTableAdapter.Fill(this.toDoListDataSet2.Lists);
        }




        //Adds the users informtation to the database
        private void NewItemButton_Click(object sender, EventArgs e)
        {
            //Check a value has been entered to create a task.
            string name = TaskNameTextbox.Text;
            string list = ListNameComboBox.Text;
            string details = TaskDetailsTextbox.Text;
            string date = (TaskDatePicker.Value.Date.Year + "/" + TaskDatePicker.Value.Date.Month + "/" + TaskDatePicker.Value.Date.Day);
            DateTime current = DateTime.Now;
            string format = "yyyy-MM-dd HH:mm:ss";
            string ct = current.ToString(format);
            string UUID = Guid.NewGuid().ToString();

            string ID = methods.FetchSingle("Select ListID From Lists Where ListName = '" + list + "'");
            string count = methods.FetchSingle("Select Count(*) From Tasks Where Task = '" + name + "' AND List = '" + ID + "'");
            if (count == "0")
            {
                if (DateCheckBox.Checked == false && PriorityCheck.Checked == false)
                {
                    //Query string to insert the information into the correct table
                    sql = "Insert into Tasks (TaskID, Task, Details, List, DateAdded) Values ('" + UUID + "', '" + name + "', '" + details + "', '" + ID + "', '" + ct + "')";
                }else if (DateCheckBox.Checked == true && PriorityCheck.Checked == true)
                {
                    //Query string to insert the information into the correct table
                    sql = "Insert into Tasks (TaskID, Task, Details, List, Priority, CompletionDate, DateAdded) Values ('" + UUID + "', '" + name + "', '" + details + "', '" + ID + "', '" + PriorityComboBox.Text + "', '" + date + "', '" + ct + "')";
                }
                else if (DateCheckBox.Checked == true)
                {
                    //Query string to insert the information into the correct table
                    sql = "Insert into Tasks (TaskID, Task, Details, List, CompletionDate, DateAdded) Values ('" + UUID + "', '" + name + "', '" + details + "', '" + ID + "', '" + date + "', '" + ct + "')";
                } else
                {
                    //Query string to insert the information into the correct table
                    sql = "Insert into Tasks (TaskID, Task, Details, List, Priority, DateAdded) Values ('" + UUID + "', '" + name + "', '" + details + "', '" + ID + "', '" + PriorityComboBox.Text + "', '" + ct + "')";
                }

                if (name == "")
                {
                    MessageBox.Show("Please Enter a Name for your Task!", "Title Required");
                }
                else
                {
                    try
                    {
                        methods.OpenConnection(sql);

                        SqlDataAdapter adapter = new SqlDataAdapter();
                        adapter.InsertCommand = new SqlCommand(sql, methods.con);
                        adapter.InsertCommand.ExecuteNonQuery();

                        Hide();
                    }
                    //Displays an error message when there is an SqlException
                    catch (SqlException ex)
                    {
                        //Error code for duplicate primary keys being entered. 
                        if (ex.Number == 2627)
                        {
                            MessageBox.Show("Please ensure this is a unique task name.", "Error Entering Data.");
                        }
                        //Error code for Foreign keys not existing/Matching
                        else if (ex.Number == 547)
                        {
                            MessageBox.Show("Please use an existing List Name.", "Error Entering Data.");
                        }
                        //Error Code for Varchar type not storing enough space for name. (could fix)
                        else if (ex.Number == 8152)
                        {
                            MessageBox.Show("Please Enter a shorter task Name.", "Error Entering Data.");
                        }
                        else
                        {
                            MessageBox.Show(ex.Message, "Error Code: " + ex.Number.ToString());
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("You have another task with this name in this list. Please enter a unique name!", "Error Creating Task.");
            }


            methods.CloseConnection();
        }

        //Cancel button closes the form. 
        private void CancelButton_Click(object sender, EventArgs e)
        {
            Hide();
        }

        //Displays the DatePicker if the user wants a completion date
        private void DateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (DateCheckBox.Checked == true)
            {
                TaskDatePicker.Visible = true;
            }
            else if (DateCheckBox.Checked == false)
            {
                TaskDatePicker.Visible = false;
            }
            
        }

        //Allows the user to edit their Task information.
        //Can update the tasks table to make the Task Name and List ID Unique identifiers which will let me clean up this code -----------------------------------
        //Update this because i think the finding of the list ID is bad and needs updated ----------------------------------
        private void UpdateButton_Click(object sender, EventArgs e)
        {

            //Check a value has been entered to create a task.
            string name = TaskNameTextbox.Text;
            string list = ListNameComboBox.Text;
            string details = TaskDetailsTextbox.Text;
            string date = (TaskDatePicker.Value.Date.Year + "/" + TaskDatePicker.Value.Date.Month + "/" + TaskDatePicker.Value.Date.Day);

            string ID = methods.FetchSingle("Select ListID From Lists Where ListName = '" + list + "'");

            //Count makes sure there is not another task with the same name in the list after editing
            string count = methods.FetchSingle("Select Count(*) from Tasks where List = '" + ID + "' AND Task = '" + name + "' AND TaskID != '" + ToDoListForm.user[0] + "'");

            if (count == "0")
            {
                if (DateCheckBox.Checked == false)
                {
                    //Query string to insert the information into the correct table
                    sql = "Update Tasks Set Task = '" + name + "', Details = '" + details + "', List = '" + ID + "' where TaskID = '" + ToDoListForm.user[0] + "'";
                }
                else
                {
                    //Query string to insert the information into the correct table
                    sql = "Update Tasks Set Task = '" + name + "', Details = '" + details + "', List = '" + ID + "', CompletionDate = '" + date + "' where TaskID = '" + ToDoListForm.user[0] + "'";
                }

                if (name == "")
                {
                    MessageBox.Show("Please Enter a Name for your Task!", "Title Required");
                }
                else
                {
                    try
                    {
                        methods.OpenConnection(sql);

                        SqlDataAdapter adapter = new SqlDataAdapter();
                        adapter.InsertCommand = new SqlCommand(sql, methods.con);
                        adapter.InsertCommand.ExecuteNonQuery();

                        Hide();
                    }
                    //Displays an error message when there is an SqlException
                    catch (SqlException ex)
                    {
                        //Error code for duplicate primary keys being entered. 
                        if (ex.Number == 2627)
                        {
                            MessageBox.Show("Please ensure this is a unique task name.", "Error Entering Data.");
                        }
                        //Error code for Foreign keys not existing/Matching
                        else if (ex.Number == 547)
                        {
                            MessageBox.Show("Please use an existing List Name.", "Error Entering Data.");
                        }
                        //Error Code for Varchar type not storing enough space for name. (could fix)
                        else if (ex.Number == 8152)
                        {
                            MessageBox.Show("Please Enter a shorter task Name.", "Error Entering Data.");
                        }
                        else
                        {
                            MessageBox.Show(ex.Message, "Error Code: " + ex.Number.ToString());
                        }
                    }
                }
                methods.CloseConnection();
            } else
            {
                MessageBox.Show("There is already a Task with that name in this List. Please Change the name or the list for this task.", "Error Editing Task!");
            }



        }

        //Displays the Priority Combobox if the user wants to add a priority to their task
        private void PriorityCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (PriorityCheck.Checked == true)
            {
                PriorityComboBox.Visible = true;
            }
            else if (PriorityCheck.Checked == false)
            {
                PriorityComboBox.Visible = false;
            }
        }
    }
}

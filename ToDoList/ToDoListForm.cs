using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToDoList
{
    public partial class ToDoListForm : Form
    {
        //user so the details can be passed between forms with "NameLabel.Text = ToDoListForm.user[0];" or something similar
        public static string[] user;
        //ParentID will be null if on home page otherwise contain the ID of the list you are currently in
        public static string ParentID = null;
        //path is a variable to store the string which will display path they have taken through the lists.
        string path = "Home";

        public ToDoListForm()
        {
            InitializeComponent();
        }
        private void ToDoListForm_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'toDoListDataSet4.Tasks' table. You can move, or remove it, as needed.
            tasksTableAdapter2.Fill(toDoListDataSet4.Tasks);

            setHomePage();

        }
        //Sets the Display to the homepage and loads the overdue tasks for the user to see.
        private void setHomePage()
        {
            ParentID = null;
            backButton.Visible = false;

            //Loads the homepage of lists where the lists with no parents will be displayed on the left
            DataTable load = methods.reloadData("Select ListName From Lists Where ListParent IS NULL");
            ListDataGrid.DataSource = load;

            path = "Home";
            PathLabel.Text = path;

            //Initialises the Current list label            
            CurrentListLabel.Text = "Home";

            //Initialise the task box with the tasks that are overdue
            DateTime current = DateTime.Now;
            string format = "MM/dd/yyyy HH:mm:ss";
            string ct = current.ToString(format);
            DataTable Completion = methods.reloadData("Select * From Tasks Where CompletionDate <= '" + ct + "'");
            dataGridView1.DataSource = Completion;
            dataGridView1.Columns[0].HeaderText = "Overdue Tasks";
            TaskNameCombo.DataSource = Completion;
        }

        //Retrieves the Task Data from the database
        private string[] Data(string sql, string key)
        {
            string[] test = new string[7];
            methods.OpenConnection(sql);

            SqlDataReader reader;
            methods.command.Parameters.Clear();
            methods.command.Parameters.AddWithValue("@Task", key);
            reader = methods.command.ExecuteReader();

            while (reader.Read())
            {
                //I now have the details of the task that needs editing. Now i must place these details somewhere for them to see and edit. 
                test[0] = reader["TaskID"].ToString();
                test[1] = reader["Task"].ToString();
                test[2] = reader["Details"].ToString();
                //list will contain the listID, i will need to use this to load the list name into the details
                test[3] = reader["List"].ToString();
                test[4] = reader["Priority"].ToString();
                test[5] = reader["CompletionDate"].ToString();
                test[6] = reader["DateAdded"].ToString();
            }


            methods.CloseConnection();

            return test;

        }




        //Click button to open new Task form
        private void AddTaskFormButton_Click(object sender, EventArgs e)
        {
            if (CurrentListLabel.Text == "Home")
            {
                MessageBox.Show("You cannot enter a task onto your home page. " + Environment.NewLine + "Please open a list to add a task.", "Error Adding Task");
            }
            else
            {
                AddItemForm AddTaskForm = new AddItemForm();
                AddTaskForm.ShowDialog();

                DataTable dt = methods.reloadData("Select * from Tasks Where List = '" + ParentID + "'");
                TaskNameCombo.DataSource = dt;
                dataGridView1.DataSource = dt;
            }
        }

        //Click button to open new List form, 
        private void AddListFormButton_Click(object sender, EventArgs e)
        {

            AddListForm AddListForm = new AddListForm();
            AddListForm.ParentListLabel.Text = CurrentListLabel.Text;
            AddListForm.ShowDialog();

            if (string.IsNullOrEmpty(ParentID))
            {
                setHomePage();
            }
            else
            {
                string load = "Select * From Tasks Where List = '" + ParentID + "'";
                string lload = "Select ListName From Lists Where ListParent = '" + ParentID + "'";
                DataTable dt = methods.reloadData(load);
                TaskNameCombo.DataSource = dt;
                dataGridView1.DataSource = dt;

                DataTable ldt = methods.reloadData(lload);
                ListDataGrid.DataSource = ldt;

                AddListForm.ParentListLabel.Text = "";
            }

        }

        //Marks a task as completed which deletes it from the database - Update to change task to completed 
        private void CompletedButton_Click(object sender, EventArgs e)
        {
            string task = TaskNameCombo.Text;
            DataGridViewRow row = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex];
            string tID = row.Cells[5].Value.ToString();
            ParentID = row.Cells[6].Value.ToString();

            string count = methods.FetchSingle("Select Count(*) from Tasks where TaskID = '" + tID + "'");
            
            if (count == "0")
            {
                MessageBox.Show("There is no task with this name. Please enter an existing task to complete.", "Error Completing Task.");
            }
            else
            {
                var confirm = MessageBox.Show("Are you sure you have completed: " + Environment.NewLine + Environment.NewLine + task + "?", "Are you sure?", MessageBoxButtons.YesNo);

                if (confirm == DialogResult.Yes)
                {
                    methods.Delete("Delete from Tasks where TaskID = '" + tID + "'");

                    if (CurrentListLabel.Text == "Home")
                    {
                        setHomePage();
                    }
                    else
                    {
                        DataTable dt = methods.reloadData("Select * From Tasks Where List = '" + ParentID + "'");
                        TaskNameCombo.DataSource = dt;
                        dataGridView1.DataSource = dt;
                    }
                    MessageBox.Show("You have completed the task: " + Environment.NewLine + Environment.NewLine + task + ".", "Congratulations, You have completed this task!");
                }
                else { }              
            }                                   
        }

        //Allows the user to edit the Task information
        private void EditButton_Click(object sender, EventArgs e)
        {
            string task = TaskNameCombo.Text;
            DataGridViewRow row = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex];
            string tID = row.Cells[5].Value.ToString();
            ParentID = row.Cells[6].Value.ToString();

            string sql = "Select * from Tasks Where TaskID = '" + tID + "'";
            user = Data(sql, task);

            //initialise the additemform here and then change the buttons visibilities and texts
            AddItemForm AddTaskForm = new AddItemForm();
            AddTaskForm.UpdateButton.Visible = true;
            AddTaskForm.NewItemButton.Visible = false;
            AddTaskForm.TaskNameTextbox.Text = user[1];
            AddTaskForm.TaskDetailsTextbox.Text = user[2];

            string listID = methods.FetchSingle("Select ListName From Lists Where ListID = '" + user[3] + "'");

            AddTaskForm.ListNameComboBox.Text = listID;

            if(user[5] == ""){} else
            {
                AddTaskForm.DateCheckBox.Checked = true;
                AddTaskForm.TaskDatePicker.Value = DateTime.ParseExact(user[5], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }
            
            AddTaskForm.ShowDialog();

            if (CurrentListLabel.Text == "Home")
            {
                setHomePage();
            } else
            {
                DataTable dt = methods.reloadData("Select * from Tasks Where List = '" + user[3] + "'");
                TaskNameCombo.DataSource = dt;
                dataGridView1.DataSource = dt;
            }
            
        }

        //Deletes the selected list from the database and any tasks or lists inside the selected list
        private void deleteListButton_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridViewRow row = ListDataGrid.Rows[ListDataGrid.CurrentCell.RowIndex];

                string list = row.Cells[0].Value.ToString();
                string sql;
                if (CurrentListLabel.Text == "Home")
                {
                    sql = "Select ListID From Lists Where ListName = '" + list + "'";
                }
                else
                {
                    sql = "Select ListID From Lists Where ListName = '" + list + "' AND ListParent = '" + ParentID + "'";
                }
                string ID = methods.FetchSingle(sql);

                var confirm = MessageBox.Show("Are you sure you want to delete the list: " + Environment.NewLine + Environment.NewLine + list + "?", "Are you sure?", MessageBoxButtons.YesNo);

                if (confirm == DialogResult.Yes)
                {
                    //If it is empty we can delete the list, if it is not, display to the user that the list still has X number of tasks in it and state that continuing with the deletion will delete these tasks.
                    string count = methods.FetchSingle("Select Count(*) from Tasks where List = '" + ID + "'");
                    string listcount = methods.FetchSingle("Select Count(*) From Lists Where ListParent = '" + ID + "'");

                    //Edit this if to check for number of lists and tasks and delete or show messages where appropriate ------------------------------------------------
                    if (count == "0" && listcount == "0")
                    {
                        methods.Delete("Delete from Lists where ListID = '" + ID + "'");
                        MessageBox.Show("You have deleted the list named: " + Environment.NewLine + Environment.NewLine + list + ".", "You have deleted a list!");
                    }

                    else if (listcount == "0")
                    {
                        var con = MessageBox.Show("There are still " + count + " tasks in this list." + Environment.NewLine + Environment.NewLine + " Are you sure you want to delete the list and all of the contents?", "Are you sure? ", MessageBoxButtons.YesNo);
                        if (con == DialogResult.Yes)
                        {
                            methods.Delete("Delete from Tasks where List = '" + ID + "'");
                            methods.Delete("Delete from Lists where ListID = '" + ID + "'");
                            MessageBox.Show("You have deleted the list named: " + Environment.NewLine + Environment.NewLine + list + ".", "You have deleted a list!");
                        }
                        else { }
                    }

                    else
                    {
                        var con = MessageBox.Show("There are still items in this list." + Environment.NewLine + Environment.NewLine + " Are you sure you want to delete the list and all of the contents?", "Are you sure? ", MessageBoxButtons.YesNo);

                        if (con == DialogResult.Yes)
                        {
                            string listID = methods.FetchSingle("Select ListID From Lists Where ListParent = '" + ID + "'");
                            string lowest = null;
                            while (listID != ID)
                            {
                                //while loop to get to bottom list
                                while (!string.IsNullOrEmpty(listID))
                                {
                                    lowest = listID;

                                    listID = methods.FetchSingle("Select ListID From Lists Where ListParent = '" + listID + "'");
                                }

                                //sets the listID to the parent of the newly deleted list
                                listID = methods.FetchSingle("Select ListParent From Lists Where ListID = '" + lowest + "'");

                                //check and delete possible tasks and then the list. 
                                string tCount = methods.FetchSingle("Select Count(*) from Tasks where List = '" + lowest + "'");
                                if (tCount == "0")
                                {
                                    methods.Delete("Delete from Lists where ListID = '" + lowest + "'");
                                }
                                else
                                {
                                    methods.Delete("Delete from Tasks where List = '" + lowest + "'");
                                    methods.Delete("Delete from Lists where ListID = '" + lowest + "'");
                                }

                            }
                            methods.Delete("Delete from Tasks where List = '" + ID + "'");
                            methods.Delete("Delete from Lists where ListID = '" + ID + "'");
                        }

                    }
                }
                else { }
            }
            //Catches the error if nothing is selected to delete.
            catch (NullReferenceException)
            {
                MessageBox.Show("You have no list selected to delete.", "Error Deleting List.");
            }

            //if to reset the view with the new information. 
            if (string.IsNullOrEmpty(ParentID))
            {
                setHomePage();
            }
            else
            {
                string load = "Select * From Tasks Where List = '" + ParentID + "'";
                string lload = "Select ListName From Lists Where ListParent = '" + ParentID + "'";
                DataTable dt = methods.reloadData(load);
                TaskNameCombo.DataSource = dt;
                dataGridView1.DataSource = dt;

                DataTable ldt = methods.reloadData(lload);
                ListDataGrid.DataSource = ldt;
            }
        }

        //Opens a list when double clicked and changes the views to display the newer information
        private void ListDataGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //With this if - i have the ListID of the parent to the current list saved. 
            DataGridViewRow row = ListDataGrid.Rows[ListDataGrid.CurrentCell.RowIndex];
            string Name = row.Cells[0].Value.ToString();
            if (CurrentListLabel.Text == "Home")
            {
                ParentID = methods.FetchSingle("Select ListID From Lists Where ListName = '" + Name + "' AND ListParent IS NULL");
                backButton.Visible = true;

            } else
            {
                ParentID = methods.FetchSingle("Select ListID From Lists Where ListName = '" + Name + "' AND ListParent = '" + ParentID  + "'");
            }
            path = path + "/" + Name;
            PathLabel.Text = path;

            //Update the label to show the parent of current list. 
            CurrentListLabel.Text = Name;

            //Loads the tasks into the view and combo
            DataTable dt = methods.reloadData("Select * from Tasks where List = '" + ParentID + "'");  
            
            dataGridView1.DataSource = dt;
            dataGridView1.Columns[0].HeaderText = "Tasks";
            if (dt.Rows.Count == 0) { TaskNameCombo.ResetText(); TaskNameCombo.DataSource = dt;} else { TaskNameCombo.DataSource = dt; }

            //loads the sublists of the currently selected list into the sublist view
            DataTable load = methods.reloadData("Select ListName from Lists Where ListParent = '" + ParentID + "'");
            ListDataGrid.DataSource = load;
        }

        //Moves the user back through their last selected lists. 
        private void backButton_Click(object sender, EventArgs e)
        {
            ParentID = methods.FetchSingle("Select ListParent From Lists Where ListID = '" + ParentID + "'");
            DataTable load;
            if (string.IsNullOrEmpty(ParentID))
            {
                setHomePage();
            }
            else
            {
                //Loads the tasks into the view and combo
                DataTable dt = methods.reloadData("Select * from Tasks Where List = '" + ParentID + "'");
                dataGridView1.DataSource = dt;
                if (dt.Rows.Count == 0) { TaskNameCombo.ResetText(); TaskNameCombo.DataSource = dt; } else { TaskNameCombo.DataSource = dt; }

                //loads the sublists of the currently selected list into the sublist view
                load = methods.reloadData("Select ListName from Lists Where ListParent = '" + ParentID + "'");
                ListDataGrid.DataSource = load;

                //Changes the Path text to take off the list name the user was just in
                path = path.Replace(( "/" + CurrentListLabel.Text), "");
                PathLabel.Text = path;
                CurrentListLabel.Text = methods.FetchSingle("Select ListName From Lists Where ListID = '" + ParentID + "'");


            }
        }

        //returns to the homepage and reset all the views
        private void homeButton_Click(object sender, EventArgs e)
        {
            setHomePage();
        }

    }
}

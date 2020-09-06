using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Windows.Forms;


namespace ToDoList
{
    public partial class NewAccountForm : Form
    {
        public NewAccountForm()
        {
            InitializeComponent();
        }

        // Plan to salt and hash passwords to be stored in the database
        private void hash()
        {

        }

        //should work except need to salt and hash passwords
        private void createAccountButton_Click(object sender, EventArgs e)
        {
            //Checks to ensure all textboxes that need a value have one.
            if (string.IsNullOrEmpty(emailTextbox.Text) || string.IsNullOrEmpty(fnameTextbox.Text) || string.IsNullOrEmpty(lnameTextbox.Text) || string.IsNullOrEmpty(passwordTextbox.Text) || string.IsNullOrEmpty(passwordconfTextbox.Text))
            {
                MessageBox.Show("Please ensure your details have been appropriately filled in!, Error Creating Account.");
            } else
            {
                //Checks that the passwords match if they have been entered
                if (passwordTextbox.Text != passwordconfTextbox.Text)
                {
                    MessageBox.Show("Your passwords do not match! Please Enter them again.", "Error Creating Account");
                    passwordconfTextbox.Text = "";
                    passwordTextbox.Text = "";
                }
                else
                {
                    string sql;
                    //Sets the sql for if the user has volunteered their phone number or not. 
                    if (string.IsNullOrEmpty(phoneTextbox.Text))
                    {
                        sql = "Insert into Users (Email, FName, LName, Password) Values ('" + emailTextbox.Text + "', '" + fnameTextbox.Text + "', '" + lnameTextbox.Text + "', '" + passwordTextbox.Text + "')";
                    }
                    else
                    {
                        sql = "Insert into Users (Email, FName, LName, Password, Salt, Phone) Values ('" + emailTextbox.Text + "', '" + fnameTextbox.Text + "', '" + lnameTextbox.Text + "', '" + passwordTextbox.Text + "', '" + phoneTextbox.Text + "')";
                    }
                    //Tries to insert the relevant data into the database
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
                            MessageBox.Show("This Email is already in use.", "Error Entering Data.");
                        }
                        //Error Code for Varchar type not storing enough space for name. (could fix)
                        else if (ex.Number == 8152)
                        {
                            MessageBox.Show("One or more of your values is too long, Please make it shorter.", "Error Entering Data.");
                        }
                        else
                        {
                            MessageBox.Show(ex.Message, "Error Code: " + ex.Number.ToString());
                            //MessageBox.Show("Error Code: " + ex.Number.ToString(), "There has been an Error.");
                        }
                    }
                }
            }
            methods.CloseConnection();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Hide();
        }
    }
}

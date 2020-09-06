using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToDoList
{
    public partial class LoginFormcs : Form
    {
        public LoginFormcs()
        {
            InitializeComponent();
        }

        //Lets the user login after checking their details. 
        //Will need to update when a salt and hash is added to the password --------------------------------------------
        private void loginButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(emailTextbox.Text))
            {
                MessageBox.Show("Please Enter your Email Address");
            } else
            {
                string password = methods.FetchSingle("Select Password From Users Where Email = '" + emailTextbox.Text + "'");
                if (password == passwordTextbox.Text)
                {
                    Hide();
                    ToDoListForm ToDoList = new ToDoListForm();
                    ToDoList.FormClosed += (s, args) => Close();
                    ToDoList.ShowDialog();
                } else
                {
                    MessageBox.Show("Your password or Email is Incorrect");
                }
            }
        }

        //Opens a the NewAccountForm to allow the user to create an account
        private void createNewButton_Click(object sender, EventArgs e)
        {
            NewAccountForm newAccount = new NewAccountForm();
            newAccount.ShowDialog();
        }
    }
}

﻿using Microsoft.VisualBasic.ApplicationServices;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Security.Cryptography;
using System.Security.Policy;
using TravelEase.PassengerDashboards;
using TravelEase.System_Admin;
using TravelEase;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Linq.Expressions;
namespace TravelEase
{
    public partial class FormLogIn : Form
    {
        SqlConnection conn;
        SqlCommand cmd;
        string QGetPassngCount = "SELECT COUNT(*) FROM PassengerTB WHERE userID = @userID";
        string QGetMAdminCount = "SELECT COUNT(*) FROM ModularAdminTB WHERE userID = @userID";
        string userInfoQuery = "SELECT u.*, l.userName, l.userPassword FROM UserTB u JOIN LoginCredentialsTB l ON u.userID = l.userID WHERE u.userID = @userID;";
        string Quid = "SELECT * FROM LoginCredentialsTB WHERE userName = @userName AND userPassword = @userPassword";
        //string QLogInfo = "select userName,userPassword from LoginCredentialsTB where"

        string connection = @"Data Source=.\SQLEXPRESS;Initial Catalog = TravelEaseDB; Integrated Security = True";
        public FormLogIn()
        {
            InitializeComponent();
            this.conn = new SqlConnection(connection);
            conn.Open();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBoxUsername.Text = string.Empty;
            textBoxPassword.Text = string.Empty;
            textBoxUsername.Focus();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxShowPassword.Checked)
                textBoxPassword.UseSystemPasswordChar = false;
            else
                textBoxPassword.UseSystemPasswordChar = true;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button_back_Click_1(object sender, EventArgs e)
        {
            FormHomePage formHomePage = new FormHomePage();
            formHomePage.Show();
            this.Hide();
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            if (textBoxUsername.Text == new TravelEase.Admin().AdminName && textBoxPassword.Text == new TravelEase.Admin().AdminPassword)
            {
                SystemAdminDashboard systemAdminDashboard = new SystemAdminDashboard();
                systemAdminDashboard.Show();
                this.Hide();
            }
            else
            {
                if (!(conn.State == ConnectionState.Open)) { conn.Open(); }
                if (!string.IsNullOrEmpty(textBoxUsername.Text) && !string.IsNullOrEmpty(textBoxPassword.Text))
                {
                    string uname = textBoxUsername.Text;
                    string upass = textBoxPassword.Text;
                    string uid = null;

                    using (cmd = new SqlCommand(Quid, conn))
                    {
                        cmd.Parameters.AddWithValue("@userName", uname);
                        cmd.Parameters.AddWithValue("@userPassword", upass);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                uid = reader["userID"].ToString();
                            }
                        }
                    }

                    if (uid != null)
                    {
                        int userCount;
                        using (cmd = new SqlCommand(QGetPassngCount, conn))
                        {
                            cmd.Parameters.AddWithValue("@userID", uid);
                            userCount = (int)cmd.ExecuteScalar();
                            if (userCount > 0)
                            {
                                MessageBox.Show($"Welcome {uname} to TravelEase");
                                PassengerInfoSingleton.Instance.CurrentPassenger = (Passenger)populateUserInfo(uid, "passenger");
                                PassengerDashboard passengBoard = new PassengerDashboard();
                                passengBoard.Show();
                                this.Hide();
                            }
                        }
                        using (cmd = new SqlCommand(QGetMAdminCount, conn))
                        {
                            cmd.Parameters.AddWithValue("@userID", uid);
                            userCount = (int)cmd.ExecuteScalar();
                            if (userCount > 0)
                            {
                                MessageBox.Show("User is ModularAdmin!");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid username or password!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Username or password cannot be empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                conn.Close();
                textBoxPassword.Clear();
                textBoxUsername.Clear();
                textBoxUsername.Focus();
            }
        }

        private object populateUserInfo(string uid, string type)
        {
            SqlCommand userInfoCmd = new SqlCommand(userInfoQuery, conn);
            userInfoCmd.Parameters.AddWithValue("@userID", uid);

            using (SqlDataReader userInfoReader = userInfoCmd.ExecuteReader())
            {
                try
                {
                    if (userInfoReader.Read())
                    {
                        string UserID = userInfoReader["userID"].ToString();
                        string FirstName = userInfoReader["fName"].ToString();
                        string LastName = userInfoReader["lName"].ToString();
                        string NID = userInfoReader["nid"].ToString();
                        string Gender = userInfoReader["gender"].ToString();
                        DateTime DateOfBirth = (DateTime)userInfoReader["dob"];
                        string Phone = userInfoReader["phone"].ToString();
                        string Email = userInfoReader["email"].ToString();
                        string Residence = userInfoReader["residence"].ToString();
                        int UserStatus = Convert.ToInt32(userInfoReader["userStatus"]);
                        string userName = userInfoReader["userName"].ToString();
                        string userPassword = userInfoReader["userPassword"].ToString();
                        if (type == "passenger")
                        {
                            Passenger passenger = new Passenger(userName, userPassword, FirstName, LastName, NID, DateOfBirth, Gender, Phone, Email, Residence, uid);
                            return passenger;
                        }
                        /*else if (type == "ModularAdmin")
                        {
                            ModularAdmin mod = new ModularAdmin(FirstName, LastName, NID, DateOfBirth, Gender, Phone, Email, Residence, uid);
                            return mod;
                        }*/
                    }
                }
                catch
                {
                    MessageBox.Show("Error retrieving user information!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBoxUsername.Clear();
                    textBoxPassword.Clear();
                    textBoxUsername.Focus();
                }
            }
            return null;
        }
    }
}

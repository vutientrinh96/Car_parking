using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Do_an_tot_nghiep
{
    public partial class Password : Form
    {
        string strConnection = @"Data Source=TIENTRINH_PC\VUTIENTRINH;Initial Catalog = DANG_NHAP; Integrated Security = True";
        SqlConnection conn;
        SqlCommand command;
        public bool txt1 = false;
        public bool txt2 = false;
        public Password()
        {
            InitializeComponent();
            textBox2.UseSystemPasswordChar = true;
        }
        private void Password_Load(object sender, EventArgs e)
        {

        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox2.UseSystemPasswordChar = false;
            }
            else
            {
                textBox2.UseSystemPasswordChar = true;
            }
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "Username")
            {
                textBox1.Text = "";
                textBox1.ForeColor = Color.Black;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                textBox1.Text = "Username";
                textBox1.ForeColor = Color.DarkGray;
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == "Password")
            {
                textBox2.Text = "";
                textBox2.ForeColor = Color.Black;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                textBox2.Text = "Password";
                textBox2.ForeColor = Color.DarkGray;
            }
        }
        private Main_menu mainMn;
        public void setmenu(Main_menu mainMn)
        {
            this.mainMn = mainMn;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = "Select Count(*) From [DANG_NHAP].[dbo].[Login] Where Taikhoan=@acc And Matkhau=@pass";
                conn = new SqlConnection(strConnection);
                conn.Open();
                command = new SqlCommand(sql, conn);
                command.Parameters.Add(new SqlParameter("@acc", textBox1.Text));
                command.Parameters.Add(new SqlParameter("@pass", textBox2.Text));
                int x = (int)command.ExecuteScalar();
                if (x == 1)
                {//đăng nhập thành công
                    this.Hide();
                    Set_the Set_the = new Set_the();
                    Set_the.Show();
                    Set_the.SetMainMenu(mainMn);                
                }
                else
                {
                    //đăng nhập thất bại
                    label3.Text = "Account or Password is not correct";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Password_Enter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            }
        }
    }
}
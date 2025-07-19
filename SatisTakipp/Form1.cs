using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Data.SQLite;

namespace SatisTakipp
{
    public partial class Form1 : Form
    {

        string dbFile = "mydatabase.db"; // Veritabanı dosyası
        public Form1()
        {
            InitializeComponent();

            //CreateDatabaseAndUser();

            
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (passwordİnput.Text == "Şifreyi Girin")
            {
                passwordİnput.Text = "";
                passwordİnput.ForeColor = Color.Black;  // Yazı rengini normale döndür
                passwordİnput.UseSystemPasswordChar = true; // Şifre karakteri olarak göster
            }
        }

        private void passwordİnput_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(passwordİnput.Text))
            {
                passwordİnput.Text = "Şifreyi Girin";
                passwordİnput.ForeColor = Color.Gray; // İpucu yazısı rengini açık yap
                passwordİnput.UseSystemPasswordChar = false; // İpucu yazısı şifre olarak gösterilmez
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                passwordİnput.UseSystemPasswordChar = false;
            }
            else
            {
                passwordİnput.UseSystemPasswordChar = true;  // Şifreyi gizle
            }
        }

        private void loginBTN_Click(object sender, EventArgs e)
        {
            string girilenSifre = passwordİnput.Text;

            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();

                string sql = "SELECT COUNT(*) FROM Users WHERE Password = @password";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@password", girilenSifre);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    if (count > 0)
                    {
                        // Şifre doğru, Form2'ye geç
                        Form2 form2 = new Form2();
                        form2.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Şifre yanlış!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void CreateDatabaseAndUser()
        {
            if (!System.IO.File.Exists(dbFile))
            {
               SQLiteConnection.CreateFile(dbFile);

                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string createTableSql = "CREATE TABLE Users (Id INTEGER PRIMARY KEY AUTOINCREMENT, Password TEXT)";
                    using (var cmd = new SQLiteCommand(createTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                   }

                    // Örnek şifreyi "12345" olarak ekliyoruz
                    string insertUserSql = "INSERT INTO Users (Password) VALUES (@password)";
                   using (var cmd = new SQLiteCommand(insertUserSql, conn))
                   {
                        cmd.Parameters.AddWithValue("@password", "ziya37");
                        cmd.ExecuteNonQuery();
                   }
              }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.Links.Clear();
            linkLabel1.Links.Add(12, 10, "https://bugraa.com");
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SatisTakipp
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            InitializeComponent();
            SetupUI();
            CreateDatabaseIfNotExists();
            LoadData();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private DataGridView dgvVeriler;
        private Button btnEkle, btnSil, btnGuncelle;
        private TextBox txtAdSoyad, txtFiyat, txtAra, telNo;
        private DateTimePicker dtpTarih;
        private Label lblToplamBorç;

        private string connectionString = "Data Source=satis.db;Version=3;";
        private DataTable currentDataTable; // Güncel veri tablosu

        
        

        private void SetupUI()
        {
            this.Text = "Veresiye Defteri";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            dgvVeriler = new DataGridView();
            dgvVeriler.Dock = DockStyle.Top;
            dgvVeriler.Height = 300;
            dgvVeriler.ReadOnly = true;
            dgvVeriler.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvVeriler.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvVeriler.MultiSelect = false;
            dgvVeriler.CellClick += DgvVeriler_CellClick;

            Panel inputPanel = new Panel();
            inputPanel.Dock = DockStyle.Top;
            inputPanel.Height = 210;
            inputPanel.Padding = new Padding(10);
            inputPanel.BackColor = Color.FromArgb(200, 200, 200);

            Label lblAdSoyad = new Label() { Text = "Ad Soyad:", Location = new Point(10, 10), Width = 70 };
            txtAdSoyad = new TextBox() { Location = new Point(90, 10), Width = 150, BackColor = Color.LightGreen, Font = new Font("Arial", 10, FontStyle.Bold) };


            Label lblTarih = new Label() { Text = "Tarih:", Location = new Point(10, 50), Width = 70 };
            dtpTarih = new DateTimePicker() { Location = new Point(90, 50), Width = 150, Format = DateTimePickerFormat.Short };

            Label lblFiyat = new Label() { Text = "Fiyat:", Location = new Point(10, 90), Width = 70 };
            txtFiyat = new TextBox() { Location = new Point(90, 90), Width = 150, BackColor = Color.LightBlue, Font = new Font("Arial", 10, FontStyle.Bold) };

            btnEkle = new Button() { Text = "Ekle", Location = new Point(270, 10), BackColor = Color.LightGreen, Width = 80 };
            btnSil = new Button() { Text = "Sil", Location = new Point(270, 50), BackColor = Color.IndianRed, Width = 80 };
            btnGuncelle = new Button() { Text = "Fiyatı Güncelle", Location = new Point(270, 90), BackColor = Color.Khaki, Width = 100 };

            btnEkle.Click += BtnEkle_Click;
            btnSil.Click += BtnSil_Click;
            btnGuncelle.Click += BtnGuncelle_Click;

            Label lblAra = new Label() { Text = "Ad ile Ara:", Location = new Point(10, 130), Width = 70 };
            txtAra = new TextBox() { Location = new Point(90, 130), Width = 200 };
            txtAra.TextChanged += TxtAra_TextChanged;

            lblToplamBorç = new Label() { Text = "Alacak: 0", Location = new Point(10, 170), AutoSize = true, Font = new Font("Arial", 12, FontStyle.Bold) };

            inputPanel.Controls.AddRange(new Control[] {
                lblAdSoyad, txtAdSoyad,
                lblTarih, dtpTarih,
                lblFiyat, txtFiyat,
                btnEkle, btnSil, btnGuncelle,
                lblAra, txtAra,
                lblToplamBorç
            });

            this.Controls.Add(dgvVeriler);
            this.Controls.Add(inputPanel);
        }

        private void CreateDatabaseIfNotExists()
        {
            if (!System.IO.File.Exists("satis.db"))
            {
                SQLiteConnection.CreateFile("satis.db");

                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string createTableQuery = @"
                        CREATE TABLE VeresiyeTablosu (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            AdSoyad TEXT,
                            AlinanTarih TEXT,
                            Ucret TEXT
                        );";
                    using (var cmd = new SQLiteCommand(createTableQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private void LoadData()
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Id, AdSoyad, AlinanTarih AS Tarih, Ucret AS Fiyat FROM VeresiyeTablosu";
                    using (var da = new SQLiteDataAdapter(query, conn))
                    {
                        currentDataTable = new DataTable();
                        da.Fill(currentDataTable);
                        dgvVeriler.DataSource = currentDataTable;
                        HesaplaToplamBorç();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Veri yüklenirken hata: " + ex.Message);
                }
            }
        }

        private void TxtAra_TextChanged(object sender, EventArgs e)
        {
            if (currentDataTable == null)
                return;

            string filtre = $"AdSoyad LIKE '%{txtAra.Text}%'";
            try
            {
                DataView dv = new DataView(currentDataTable);
                dv.RowFilter = filtre;
                dgvVeriler.DataSource = dv;
                HesaplaToplamBorç(dv.ToTable());
            }
            catch { }
        }

        private void DgvVeriler_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvVeriler.CurrentRow != null)
            {
                txtAdSoyad.Text = dgvVeriler.CurrentRow.Cells["AdSoyad"].Value.ToString();

                // Tarih için güvenli dönüşüm:
                var tarihValue = dgvVeriler.CurrentRow.Cells["Tarih"].Value;
                if (DateTime.TryParse(tarihValue?.ToString(), out DateTime tarih))
                {
                    dtpTarih.Value = tarih;
                }
                else
                {
                    dtpTarih.Value = DateTime.Today; // Geçersizse bugünün tarihi
                }

                txtFiyat.Text = dgvVeriler.CurrentRow.Cells["Fiyat"].Value.ToString();
            }
        }

        private void mainPage_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void BtnEkle_Click(object sender, EventArgs e)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string insertQuery = "INSERT INTO VeresiyeTablosu (AdSoyad, AlinanTarih, Ucret) VALUES (@adSoyad, @tarih, @ucret)";
                    using (var cmd = new SQLiteCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@adSoyad", txtAdSoyad.Text);
                        cmd.Parameters.AddWithValue("@tarih", dtpTarih.Value.ToShortDateString());
                        cmd.Parameters.AddWithValue("@ucret", txtFiyat.Text);
                        cmd.ExecuteNonQuery();
                    }
                    LoadData();
                    Temizle();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kayıt eklenirken hata: " + ex.Message);
                }
            }
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (dgvVeriler.CurrentRow != null)
            {
                int id = Convert.ToInt32(dgvVeriler.CurrentRow.Cells["Id"].Value);
                using (var conn = new SQLiteConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM VeresiyeTablosu WHERE Id = @id";
                        using (var cmd = new SQLiteCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }
                        LoadData();
                        Temizle();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Silme işlemi sırasında hata: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek istediğiniz kaydı seçin.");
            }
        }

        private void BtnGuncelle_Click(object sender, EventArgs e)
        {
            if (dgvVeriler.CurrentRow != null)
            {
                int id = Convert.ToInt32(dgvVeriler.CurrentRow.Cells["Id"].Value);
                using (var conn = new SQLiteConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string updateQuery = @"UPDATE VeresiyeTablosu
                                               SET Ucret = @ucret
                                               WHERE Id = @id";
                        using (var cmd = new SQLiteCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@ucret", txtFiyat.Text);
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }
                        LoadData();
                        Temizle();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Güncelleme işlemi sırasında hata: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek istediğiniz kaydı seçin.");
            }
        }

        private void Temizle()
        {
            txtAdSoyad.Text = "";
            dtpTarih.Value = DateTime.Today;
            txtFiyat.Text = "";
        }

        private void HesaplaToplamBorç(DataTable dt = null)
        {
            if (dt == null) dt = currentDataTable;
            if (dt == null) return;

            try
            {
                decimal toplam = 0;
                foreach (DataRow row in dt.Rows)
                {
                    string fiyatStr = row["Fiyat"].ToString();
                    if (decimal.TryParse(fiyatStr, out decimal fiyat))
                        toplam += fiyat;
                }
                lblToplamBorç.Text = $"Toplam Alacak: {toplam:C}";
            }
            catch
            {
                lblToplamBorç.Text = "Toplam Borç: Hesaplanamadı";
            }
        }
    }
}

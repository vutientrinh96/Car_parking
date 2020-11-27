using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ApplicationBlocks.Data;
using System.Data.SqlClient;
using AForge.Video;
using AForge.Video.DirectShow;
using System.IO;
using System.IO.Ports;

namespace Do_an_tot_nghiep
{
    public partial class Set_the : Form
    {
        //public delegate string MyDelegate();
        //delegate void SetTextCallback(string text); // Khai bao delegate SetTextCallBack voi tham so string
        string strCon = @"Data Source=TIENTRINH_PC\VUTIENTRINH;Initial Catalog=DANG_NHAP;Integrated Security=True";
        public Set_the()
        {
            InitializeComponent();
        }
        private FilterInfoCollection CaptureDevice;
        private Main_menu mainMn;

        public void SetMainMenu(Main_menu mainMn)
        {
            this.mainMn = mainMn;
        }
        private void rfid1(object sender, EventArgs e)
        {
            mainMn.ketnoirf1(comboBoxRF1.Text, comboBoxRF1.Enabled, ketnoi_rfid1.Text);
            comboBoxRF1.Enabled = false;
            ketnoi_rfid1.Text = "Ngắt kết nối";
        }
        private void rfid2(object sender, EventArgs e)
        {
            mainMn.ketnoirf2(comboBoxRF2.Text, comboBoxRF2.Enabled);
            comboBoxRF2.Enabled = false;
            ketnoi_rfid2.Text = "Ngắt kết nối";
        }
        private int count3 = 0;
        private void OnConnectClick(object sender, EventArgs e)
        {
            mainMn.KetNoiCamClick(comboBoxCAM1.SelectedIndex, comboBoxCAM2.SelectedIndex);

            if (comboBoxCAM1.SelectedIndex != comboBoxCAM2.SelectedIndex)
            {
                count3 += 1;
            }
            if (count3 == 2)
            {
                count3 = 0;
            }
            if (count3 == 1)
            {
                ketnoi_cam.Text = "Ngắt kết nối";
                comboBoxCAM1.Enabled = false;
                comboBoxCAM2.Enabled = false;
            }
            else if (count3 == 0)
            {
                ketnoi_cam.Text = "Kết nối camera";
                comboBoxCAM1.Enabled = true;
                comboBoxCAM2.Enabled = true;
            }


        }
     
        public void LoadData()
        {
            thongtin.DataSource = SqlHelper.ExecuteDataset(strCon, "MaThe_SelectAll").Tables[0];
        }
        public void SetValue(int index)
        {
            DataGridViewRow row = thongtin.Rows[index];
            txtMaThe.Text = row.Cells[1].Value.ToString();
        }
        private void thongtin_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            for (int i = 0; i < thongtin.RowCount; i++)
            {
                thongtin[0, i].Value = i + 1;
            }
        }
        private void thongtin_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            SetValue(e.RowIndex);
        }

        private void Set_the_Load(object sender, EventArgs e)
        {
            LoadData();
            CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo Device in CaptureDevice)
            {
                comboBoxCAM1.Items.Add(Device.Name);
                comboBoxCAM2.Items.Add(Device.Name);
            }
            comboBoxCAM1.SelectedIndex = 0;
            comboBoxCAM2.SelectedIndex = 0;

            string[] ComList = SerialPort.GetPortNames();
            int[] ComNumberList = new int[ComList.Length];
            for (int i = 0; i < ComList.Length; i++)
            {
                ComNumberList[i] = int.Parse(ComList[i].Substring(3));
            }

            Array.Sort(ComNumberList);

            foreach (int ComNumber in ComNumberList)
            {
                comboBoxRF1.Items.Add("COM" + ComNumber.ToString());
                comboBoxRF2.Items.Add("COM" + ComNumber.ToString());
            }

        }

        private void them_Click(object sender, EventArgs e)
        {
            try
            {
                string mathe = txtMaThe.Text.Trim();
                string ngayset = DateTime.Now.ToShortDateString();
                DataTable dt = SqlHelper.ExecuteDataset(strCon, "KTTMT", mathe).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    MessageBox.Show("Thẻ nhập bị trùng!");
                }
                else
                {
                    SqlHelper.ExecuteNonQuery(strCon, "MaThe_Insert", mathe, ngayset);
                    LoadData();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi:" + ex.ToString());
            }
        }

        private void xoa_Click_1(object sender, EventArgs e)
        {
            try
            {
                int CurrentIndex = thongtin.CurrentCell.RowIndex;
                string mathe = Convert.ToString(thongtin.Rows[CurrentIndex].Cells[1].Value.ToString());
                SqlHelper.ExecuteNonQuery(strCon, "MaThe_Delete", mathe);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi:" + ex.ToString());
            }
            //if (dgvMaThe.Rows.Count > 0)
            //    foreach (DataGridViewRow row in dgvMaThe.SelectedRows)
            //    {
            //        dgvMaThe.Rows.Remove(row);

            //    }   
        }
        private void thoat_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
        private void thongtin_RowEnter_1(object sender, DataGridViewCellEventArgs e)
        {
            SetValue(e.RowIndex);
        }
        private void thongtin_RowPrePaint_1(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            for (int i = 0; i < thongtin.RowCount; i++)
            {
                thongtin[0, i].Value = i + 1;
            }
        }
        private void Set_the_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Form1 form1 = new Form1();
            //form1.Show();
            this.Hide();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            txtMaThe.Text = GlobalVars.SerialData;
        }

        
    }
}

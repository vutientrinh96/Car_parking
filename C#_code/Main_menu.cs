using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;
using System.IO;
using System.IO.Ports;
using System.Xml;
using Microsoft.ApplicationBlocks.Data; //sqlhelper
using System.Data.SqlClient;
using WMPLib;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using tesseract;
using MySql.Data.MySqlClient;
using System.Threading;

namespace Do_an_tot_nghiep
{
    public partial class Main_menu : Form
    {
        #region ĐỊNH NGHĨA, KHỎI TẠO
        //WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();
        bool mode = false;
        bool EnableProcess = false;
        //bool setthe = false;
        string InputData1 = String.Empty; // Khai báo string buff dùng cho hiển thị dữ liệu sau này.
        string InputData2 = String.Empty; // Khai báo string buff dùng cho hiển thị dữ liệu sau này.
        delegate void SetTextCallback(string text); // Khai bao delegate SetTextCallBack voi tham so string
        string strCon = @"Data Source=TIENTRINH_PC\VUTIENTRINH;Initial Catalog=DANG_NHAP;Integrated Security=True";
        public Main_menu()
        {
            //this.WindowState = FormWindowState.Normal;
            //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Bounds = Screen.PrimaryScreen.Bounds;
            InitializeComponent();
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);
            serialPort2.DataReceived += new SerialDataReceivedEventHandler(serialPort2_DataReceived);
        }
        private FilterInfoCollection CaptureDevice;
        private VideoCaptureDevice FinalFrame1;
        private VideoCaptureDevice FinalFrame2;
        //private Set_the setthe;

        //******XU LY ANH*******

        List<Image<Bgr, byte>> PlateImagesList = new List<Image<Bgr, byte>>();
        Image Plate_Draw;
        List<string> PlateTextList = new List<string>();
        List<Rectangle> listRect = new List<Rectangle>();

        PictureBox[] box = new PictureBox[12];

        public TesseractProcessor full_tesseract = null;
        public TesseractProcessor ch_tesseract = null;
        public TesseractProcessor num_tesseract = null;
        private string m_path = Application.StartupPath + @"\data\";
        private List<string> lstimages = new List<string>();
        private const string m_lang = "eng";
        MySqlConnection connection;
        private void Main_menu_Load(object sender, EventArgs e)
        {
            #region    Connect Mysql Please
            try
            {
                MySqlConnectionStringBuilder conn_string = new MySqlConnectionStringBuilder();
                conn_string.Server = "den1.mysql5.gear.host";
                conn_string.UserID = "datn";
                conn_string.Password = "khanh-le-vu";
                conn_string.Database = "datn";
                connection = new MySqlConnection(conn_string.ToString());
                MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT * FROM datn.car_booking", connection);
                connection.Open();
                DataSet ds = new DataSet();
                adapter.Fill(ds, "users");
                dataGridView2.DataSource = ds.Tables["users"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            #endregion
            timer1.Start();
            CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            //foreach (FilterInfo Device in CaptureDevice)
            //{
            //    cbx1.Items.Add(Device.Name);
            //    cbx2.Items.Add(Device.Name);
            //}
            //cbx1.SelectedIndex = 0;
            //cbx2.SelectedIndex = 0;
            FinalFrame1 = new VideoCaptureDevice();
            FinalFrame2 = new VideoCaptureDevice();
            //////////////////////////////////////////////////
            //string[] ComList = SerialPort.GetPortNames();
            //int[] ComNumberList = new int[ComList.Length];
            //for (int i = 0; i < ComList.Length; i++)
            //{
            //    ComNumberList[i] = int.Parse(ComList[i].Substring(3));
            //}

            //Array.Sort(ComNumberList);

            //foreach (int ComNumber in ComNumberList)
            //{
            //    comboBox3.Items.Add("COM" + ComNumber.ToString());
            //    comboBox4.Items.Add("COM" + ComNumber.ToString());
            //}

            //XU LY ANH////
            full_tesseract = new TesseractProcessor();
            bool succeed = full_tesseract.Init(m_path, m_lang, 3);
            if (!succeed)
            {
                MessageBox.Show("Tesseract initialization failed. The application will exit.");
                Application.Exit();
            }
            full_tesseract.SetVariable("tessedit_char_whitelist", "ABCDEFHKLMNPRSTVXY1234567890").ToString();

            ch_tesseract = new TesseractProcessor();
            succeed = ch_tesseract.Init(m_path, m_lang, 3);
            if (!succeed)
            {
                MessageBox.Show("Tesseract initialization failed. The application will exit.");
                Application.Exit();
            }
            ch_tesseract.SetVariable("tessedit_char_whitelist", "ABCDEFHKLMNPRSTUVXY").ToString();

            num_tesseract = new TesseractProcessor();
            succeed = num_tesseract.Init(m_path, m_lang, 3);
            if (!succeed)
            {
                MessageBox.Show("Tesseract initialization failed. The application will exit.");
                Application.Exit();
            }
            num_tesseract.SetVariable("tessedit_char_whitelist", "1234567890").ToString();


            m_path = System.Environment.CurrentDirectory + "\\";

            for (int i = 0; i < box.Length; i++)
            {
                box[i] = new PictureBox();
            }
        }
        #endregion

        #region KẾT NỐI, CHỤP ẢNH, RFID, CSDL

        public void KetNoiCamClick(int cb1Index, int cb2Index)
        {
            if (cb1Index < 0 && cb2Index < 0)
            {
                MessageBox.Show("Chưa chọn Camera, vui lòng chọn lại!", "Lỗi!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //ketnoi_cam.Text = "Kết nối camera";
            }
            else if (cb1Index == cb2Index)
            {
                MessageBox.Show("Camera bị chọn trùng, vui lòng chọn lại! ", "Lỗi!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //ketnoi_cam.Text = "Kết nối camera";
            }
            else if (cb1Index < 0)
            {
                MessageBox.Show("Vui lòng chọn Camera ngõ vào! ", "Lỗi!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (cb2Index < 0)
            {
                MessageBox.Show("Vui lòng chọn Camera ngõ ra! ", "Lỗi!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                FinalFrame1 = new VideoCaptureDevice(CaptureDevice[cb1Index].MonikerString);
                FinalFrame1.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame1);
                FinalFrame1.Start();

                FinalFrame2 = new VideoCaptureDevice(CaptureDevice[cb2Index].MonikerString);
                FinalFrame2.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame2);
                FinalFrame2.Start();

                EnableProcess = true;

                //.Text = "Ngắt kết nối camera";
                //cbx1.Enabled = false;
                //cbx2.Enabled = false;
            }
        }
        void FinalFrame_NewFrame1(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }
        void FinalFrame_NewFrame2(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox3.Image = (Bitmap)eventArgs.Frame.Clone();
        }
        private void Main_menu_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort1.IsOpen) serialPort1.Close();
            if (serialPort2.IsOpen) serialPort2.Close();
        }
        public void ketnoirf1(string cbxtext, bool cbxenable, string btnrf1)
        {
            if (cbxtext == "")
            {
                MessageBox.Show("Vui lòng chọn cổng com", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                btnrf1 = "Kết nối RFID 1";
                cbxenable = true;
            }
            else
            {
                try
                {
                    serialPort1.PortName = cbxtext;
                    serialPort1.Open();
                    btnrf1 = "Ngắt kết nối";
                    MessageBox.Show("Kết nối thành công!");
                    cbxenable = false;
                }
                catch
                {
                    MessageBox.Show("Không thể mở cổng " + serialPort1.PortName, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            InputData1 = serialPort1.ReadExisting();
            if (InputData1 != String.Empty)
            {
                // textbox1 = InputData; // Ko dùng đc như thế này vì khác threads .
                SetText1(InputData1); // Chính vì vậy phải sử dụng ủy quyền tại đây. Gọi delegate đã khai báo trước đó.
            }
        }
        private void SetText1(string text)
        {
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText1); // khởi tạo 1 delegate mới gọi đến SetText
                this.Invoke(d, new object[] { text });
            }
            else
            {
                GlobalVars.SerialData = text.Substring(2);
                mode = true;
            }

            if (!string.IsNullOrEmpty(text) && EnableProcess == true && mode == true)
            {
                try
                {
                    string rec = text.Trim();
                    string mathe = rec.Substring(2);
                    if (rec.IndexOf("ci") != -1)
                    {
                        DataTable dt = SqlHelper.ExecuteDataset(strCon, "KTTMT", mathe).Tables[0];//Kiem Tra trung ma the
                        if (dt.Rows.Count > 0)
                        {
                            string giovao = DateTime.Now.ToString();
                            this.textBox1.Text = text;
                            rec = rec.Substring(2) + "_1";
                            pictureBox2.Image = (Bitmap)pictureBox1.Image.Clone();
                            Image xxx = pictureBox2.Image;
                            Bitmap luuanh = new Bitmap(pictureBox2.Image);                    
                            string file_path, file_name;
                            file_path = Path.GetDirectoryName(@"D:\Graduate project\Picture\"); //lấy thư mục của file từ đường dẫn file
                            file_name = Path.GetFileNameWithoutExtension(@"D:\Graduate project\Picture\"); //lấy tên file không bao gồm phần đuôi kiểu file                           
                            luuanh.Save(file_path + "\\" + rec + DateTime.Now.ToString("_MM-dd-yyyy_hh-mm-ss") + ".jpg");
                            luuanh.Save(file_path + "\\" + rec + ".jpg");                          
                            SqlHelper.ExecuteNonQuery(strCon, "Xoa_DL_Xe", mathe); // Đề phòng quét >2 lần 
                            SqlHelper.ExecuteNonQuery(strCon, "Insert_XeVao_Bai", mathe, giovao);
                            player.URL = @"D:\Graduate project\mp3\welcome,comeinplease.mp3"; // Đường dẫn đến file cần mở
                            player.Ctlcontrols.play();
                            serialPort2.Write("ci"); // gửi mã báo có xe vào "car in";
                            serialPort1.Write("1");
                            //**********XU LY ANH ****************
                            Image temp1;
                            string temp2, temp3;
                            Reconize(file_path + "\\" + rec + DateTime.Now.ToString("_MM-dd-yyyy_hh-mm-ss") + ".jpg", out temp1, out temp2, out temp3);
                            xxx = temp1;
                            if (temp3 == "")
                                text_BiensoVAO.Text = "ko nhận dạng dc biển số";
                            else
                                text_BiensoVAO.Text = temp3;
                            #region luu_BS
                            Bitmap luubs = new Bitmap(pictureBox_BiensoVAO.Image);
                            Bitmap luukt = new Bitmap(pictureBox_kytuVAO.Image);
                            
                            string duongdanbs, tenbs;
                            duongdanbs = Path.GetDirectoryName(@"D:\Graduate project\BSXE\"); //lấy thư mục của file từ đường dẫn file
                            tenbs = Path.GetFileNameWithoutExtension(@"D:\Graduate project\BSXE\");
                            //duongdankt = Path.GetDirectoryName(@"D:\Graduate project\BSXE\"); //lấy thư mục của file từ đường dẫn file
                            //tenkt = Path.GetFileNameWithoutExtension(@"D:\Graduate project\BSXE\");
                            luubs.Save(duongdanbs + "\\" + rec + "bs" + DateTime.Now.ToString("_MM-dd-yyyy_hh-mm-ss") + ".jpg");
                            luubs.Save(duongdanbs + "\\" + rec + "bs" + ".jpg");
                            luukt.Save(duongdanbs + "\\" + rec + "kt" + DateTime.Now.ToString("_MM-dd-yyyy_hh-mm-ss") + ".jpg");
                            luukt.Save(duongdanbs + "\\" + rec + "kt" + ".jpg");
                            #endregion
                            StreamWriter txtbs = new StreamWriter(duongdanbs + "\\" + rec + "txt" + ".txt");
                            txtbs.Write(text_BiensoVAO.Text);
                            txtbs.Close();
                            mode = false;
                        }
                        else
                        {
                            //serialPort1.Write("0");
                            MessageBox.Show("Thẻ không có trong hệ thống!");
                            mode = false;
                        }
                    }
                    else { }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi:" + ex.ToString());
                }
            }
            else if (EnableProcess == false /*&& setthe == true*/ ) { MessageBox.Show("Camera chưa được kết nối! ", "Lỗi!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        public void ketnoirf2(string cbx4, bool cbxy)
        {
            if (cbx4 == "")
            {
                MessageBox.Show("Vui lòng chọn cổng COM", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (serialPort2.IsOpen)
            {
                serialPort2.Close();
                //ketnoi_rfid2.Text = "Kết nối RFID 2";
                cbxy = true;
            }
            else
            {
                try
                {
                    serialPort2.PortName = cbx4;
                    serialPort2.Open();
                    serialPort2.Write("ACK");
                    //ketnoi_rfid2.Text = "Ngắt kết nối";
                    MessageBox.Show("Kết nối thành công!");
                    cbxy = false;
                }
                catch
                {
                    MessageBox.Show("Không thể mở cổng " + serialPort2.PortName, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void serialPort2_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            InputData2 = serialPort2.ReadExisting();

            if (InputData2 != String.Empty)
            {
                // textbox1 = InputData; // Ko dùng đc như thế này vì khác threads .
                SetText2(InputData2); // Chính vì vậy phải sử dụng ủy quyền tại đây. Gọi delegate đã khai báo trước đó.
            }
        }
        private void SetText2(string text2)
        {
            if (this.textBox2.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText2); // khởi tạo 1 delegate mới gọi đến SetText

                this.Invoke(d, new object[] { text2 });
            }
            else
            {
                mode = true;
            }
            if (text2 != "" && EnableProcess == true && mode == true)
            {
                try
                {
                    string rec = text2.Trim();
                    string mathe = rec.Substring(2);
                    this.textBox2.Text = mathe;

                    if (rec.IndexOf("co") != -1)
                    {
                        DataTable dt0 = SqlHelper.ExecuteDataset(strCon, "KTTMT", mathe).Tables[0];
                        if (dt0.Rows.Count > 0)
                        {
                            DataTable dt = SqlHelper.ExecuteDataset(strCon, "Kiem_Tra_Xe_TB", mathe).Tables[0];
                            if (dt.Rows.Count <= 0)
                            {
                                serialPort2.Write("0");
                                MessageBox.Show("Xe không có trong bãi!");
                                mode = false;
                            }
                            else
                            {
                                try
                                {
                                    SqlParameter[] arParms = new SqlParameter[2];
                                    // @PersonID Input Parameter
                                    arParms[0] = new SqlParameter("@MaThe", SqlDbType.NVarChar, 50);
                                    arParms[0].Value = mathe;
                                    // @ProductName Output Parameter
                                    arParms[1] = new SqlParameter("@GioVao", SqlDbType.NVarChar, 50);
                                    arParms[1].Direction = ParameterDirection.Output;
                                    // Execute the stored procedure
                                    SqlHelper.ExecuteNonQuery(strCon, CommandType.StoredProcedure, "Return_Gio_Vao", arParms);
                                    // create a string array of return values and assign values returned from stored procedure
                                    string[] arReturnParms = new string[1];
                                    arReturnParms[0] = arParms[1].Value.ToString();
                                    //arReturnParms[1] = arParms[2].Value.ToString();
                                    string giovao = arReturnParms[0];
                                    DateTime startdate = DateTime.Parse(giovao);
                                    DateTime now = DateTime.Now;
                                    TimeSpan elapsed = now.Subtract(startdate);
                                    label_tongthoigian.Text = elapsed.ToString(@"hh\:mm\:ss");
                                    double days = (int)elapsed.TotalDays;
                                    var hours = (int)elapsed.TotalHours;
                                    if (hours <= 0) hours = 1;
                                    double sotien = hours * 5000;
                                    label_giatien.Text = sotien.ToString();
                                    ///////////////////////////////////////
                                    rec = rec.Substring(2) + "_1";
                                    pictureBox4.Image = (Bitmap)pictureBox3.Image.Clone();
                                    Image yyy = pictureBox2.Image;
                                    string file_path, file_name;
                                    file_path = Path.GetDirectoryName(@"D:\Graduate project\Picture\"); //lấy thư mục của file từ đường dẫn file
                                    file_name = Path.GetFileNameWithoutExtension(@"D:\Graduate project\Picture\"); //lấy tên file không bao gồm phần đuôi kiểu file
                              
                                    Bitmap luuanhra = new Bitmap(pictureBox4.Image);
                                    luuanhra.Save(file_path + "\\" + "ra" + rec + DateTime.Now.ToString("_MM-dd-yyyy_hh-mm-ss") + ".jpg");
                                    luuanhra.Save(file_path + "\\" + "ra" + rec + ".jpg");
                                    Bitmap moanh = new Bitmap(Image.FromFile(file_path + "\\" + rec + ".jpg"));
                                   
                                    pictureBox2.Image = moanh;
                                    
                                    player.URL = @"D:\Graduate project\mp3\thankyou,haveaniceday.mp3"; // Đường dẫn đến file cần mở
                                    player.Ctlcontrols.play();
                                    //Xoá DL Xe khi đã ra
                                    SqlHelper.ExecuteNonQuery(strCon, "Xoa_DL_Xe", mathe);
                                    serialPort2.Write("co"); // gửi mã báo có xe ra "car out";
                                                             //serialPort2.Write("1");
                                    Image temp11;
                                    string temp22, temp33;
                                    Reconize_1(file_path + "\\" + "ra" + rec + DateTime.Now.ToString("_MM-dd-yyyy_hh-mm-ss") + ".jpg", out temp11, out temp22, out temp33);
                                    yyy = temp11;
                                    if (temp33 == "")
                                        text_BiensoRA.Text = "ko nhận dạng dc biển số";
                                    else
                                        text_BiensoRA.Text = temp33;
                                    #region testmobs
                                    string duongdanbs, tenbs;
                                    duongdanbs = Path.GetDirectoryName(@"D:\Graduate project\BSXE\"); //lấy thư mục của file từ đường dẫn file
                                    tenbs = Path.GetFileNameWithoutExtension(@"D:\Graduate project\BSXE\");
                                    //duongdankt = Path.GetDirectoryName(@"D:\Sinh vien nam 4\Do an 2\Do_an\doan\KTXE\"); //lấy thư mục của file từ đường dẫn file
                                    //tenkt = Path.GetFileNameWithoutExtension(@"D:\Sinh vien nam 4\Do an 2\Do_an\doan\KTXE\");
                                    Bitmap mobs = new Bitmap(Image.FromFile(duongdanbs + "\\" + rec + "bs" + ".jpg"));
                                    Bitmap mokt = new Bitmap(Image.FromFile(duongdanbs + "\\" + rec + "kt" + ".jpg"));
                                    pictureBox_BiensoVAO.Image = mobs;
                                    pictureBox_kytuVAO.Image = mokt;
                                    #endregion
                                    ////StreamReader txtbs = new StreamReader(duongdanbs + "\\" + rec + "txt" + ".txt");
                                    //text_BiensoRA.Text = txtbs.Read();
                                    mode = false;
                                }
                                catch (Exception er)
                                {
                                    MessageBox.Show("Lỗi:" + er.ToString());
                                }
                            }
                        }
                        else
                        {
                            //serialPort2.Write("0");
                            MessageBox.Show("Thẻ không có trong hệ thống!");
                            mode = false;
                        }
                    }
                    else { }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi:" + ex.ToString());
                }
            }
            else if (EnableProcess == false) { MessageBox.Show("Camera chưa được kết nối! ", "Lỗi!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            // if (text2 != "" && EnableProcess == true && mode2 == true)
            //  {
            //     string rec2 = text2.Trim();
            //     string giodat = "";
            //      string bienso = "";
            //      if (rec2.IndexOf("dc") != -1)
            //      {
            //          bienso = rec2.Substring(2);
            //         giodat = DateTime.Now.ToString();
            //          SqlHelper.ExecuteNonQuery(strCon, "Insert_Xe_Dat_Cho", bienso, giodat);
            //         mode2 = false;
            //      }
            // }
        }
        private void Main_menu_FormClosed(object sender, FormClosedEventArgs e)
        {
            serialPort1.Close();
            serialPort2.Close();
        }
        #endregion

        #region MENU
        private void setTheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Password Password = new Password();
            Password.Show();
            Password.setmenu(this);
            Set_the Set_the = new Set_the();
            Set_the.SetMainMenu(this);
            //Set_the.Show();
        }
        private void aboutUsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("    ** Design by Trinh and Khanh ** \n                    ############ \n ** Support number: 0942-726-725 **");
        }

        int x = 96, y = 0, a = 1;
        Random random = new Random();
        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = DateTime.Now.ToString("hh:mm:ss tt");
            try
            {
                x += a;
                labelchaychu.Location = new System.Drawing.Point(x, y);
                if (x >= 576)
                {
                    a = -1;
                    labelchaychu.ForeColor = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
                }
                if (x <= 96)
                {
                    a = 1;
                    labelchaychu.ForeColor = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
                }
            }
            catch (Exception)
            { }
        }

        private void fullscreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            pictureBox1.Location = new System.Drawing.Point(0, 0);
            pictureBox1.Size = new System.Drawing.Size(683, 384);
            pictureBox1.BringToFront();
            pictureBox2.Location = new System.Drawing.Point(683, 0);
            pictureBox2.Size = new System.Drawing.Size(683, 384);
            pictureBox2.BringToFront();
            pictureBox3.Location = new System.Drawing.Point(0, 384);
            pictureBox3.Size = new System.Drawing.Size(683, 384);
            pictureBox3.BringToFront();
            pictureBox4.Location = new System.Drawing.Point(683, 384);
            pictureBox4.Size = new System.Drawing.Size(683, 384);
            pictureBox4.BringToFront();
        }
        private void Main_menu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.WindowState = FormWindowState.Normal;
                pictureBox1.Location = new System.Drawing.Point(2, 23);
                pictureBox1.Size = new System.Drawing.Size(420, 340);
                pictureBox2.Location = new System.Drawing.Point(439, 23);
                pictureBox2.Size = new System.Drawing.Size(420, 340);
                pictureBox3.Location = new System.Drawing.Point(2, 387);
                pictureBox3.Size = new System.Drawing.Size(420, 340);
                pictureBox4.Location = new System.Drawing.Point(439, 387);
                pictureBox4.Size = new System.Drawing.Size(420, 340);
            }
            else if (e.KeyCode == Keys.M)
            {
                serialPort2.Write("1");
            }
            else
            { }
        }
        #endregion

        #region XU LY BS VAO
        public void ProcessImage(string urlImage)
        {
            System.Diagnostics.Debug.WriteLine("ProcessImage");
            PlateImagesList.Clear();
            PlateTextList.Clear();
            //FileStream fs = new FileStream(urlImage, FileMode.Open, FileAccess.Read);
            Image img = pictureBox2.Image;
            Bitmap image = new Bitmap(img);
            //fs.Close();

            FindLicensePlate4(image, out Plate_Draw);
        }
        private string Ocr(Bitmap image_s, bool isFull, bool isNum = false)
        {
            System.Diagnostics.Debug.WriteLine("ORC BIT MAP");
            string temp = "";
            Image<Gray, byte> src = new Image<Gray, byte>(image_s);
            double ratio = 1;
            while (true)
            {
                ratio = (double)CvInvoke.cvCountNonZero(src) / (src.Width * src.Height);
                if (ratio > 0.5) break;
                src = src.Dilate(2);
            }
            Bitmap image = src.ToBitmap();

            TesseractProcessor ocr;
            if (isFull)
                ocr = full_tesseract;
            else if (isNum)
                ocr = num_tesseract;
            else
                ocr = ch_tesseract;

            int cou = 0;
            ocr.Clear();
            ocr.ClearAdaptiveClassifier();
            temp = ocr.Apply(image);
            while (temp.Length > 3)
            {
                Image<Gray, byte> temp2 = new Image<Gray, byte>(image);
                temp2 = temp2.Erode(2);
                image = temp2.ToBitmap();
                ocr.Clear();
                ocr.ClearAdaptiveClassifier();
                temp = ocr.Apply(image);
                cou++;
                if (cou > 10)
                {
                    temp = "";
                    break;
                }
            }
            return temp;
        }
        public void FindLicensePlate4(Bitmap image, out Image plateDraw)
        {
            System.Diagnostics.Debug.WriteLine("Find license plate 4");
            plateDraw = null;
            Image<Bgr, byte> frame;
            bool isface = false;
            Bitmap src;
            Image dst = image;
            HaarCascade haar = new HaarCascade(@"C:\Users\vutri\OneDrive\Máy tính\test\giao dien C# _ 2\Do_an_2\Do_an_2\bin\Debug\output-hv-33-x25.xml");

            System.Diagnostics.Debug.WriteLine("Find license plate 4 - 676");
            for (float i = 0; i <= 20; i = i + 3)
            {
                for (float s = -1; s <= 1 && s + i != 1; s += 2)
                {
                    src = RotateImage(dst, i * s);
                    PlateImagesList.Clear();
                    frame = new Image<Bgr, byte>(src);
                    using (Image<Gray, byte> grayframe = new Image<Gray, byte>(src))
                    {
                        var faces = grayframe.DetectHaarCascade(haar, 1.1, 8, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(0, 0))[0];

                        foreach (var face in faces)
                        {
                            Image<Bgr, byte> tmp = frame.Copy();
                            tmp.ROI = face.rect;

                            frame.Draw(face.rect, new Bgr(Color.Blue), 2);

                            PlateImagesList.Add(tmp);

                            isface = true;
                        }
                        if (isface)
                        {
                            Image<Bgr, byte> showimg = frame.Clone();
                            if (PlateImagesList.Count > 1)
                            {
                                for (int k = 1; k < PlateImagesList.Count; k++)
                                {
                                    if (PlateImagesList[0].Width < PlateImagesList[k].Width)
                                    {
                                        PlateImagesList[0] = PlateImagesList[k];
                                    }
                                }
                            }
                            PlateImagesList[0] = PlateImagesList[0].Resize(400, 400, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
                            return;
                        }
                    }
                }
            }
        }
        public static Bitmap RotateImage(Image image, float angle)
        {
            System.Diagnostics.Debug.WriteLine("Rotate image return bitmap");
            if (image == null)
                throw new ArgumentNullException("image");

            PointF offset = new PointF((float)image.Width / 2, (float)image.Height / 2);

            //create a new empty bitmap to hold rotated image
            Bitmap rotatedBmp = new Bitmap(image.Width, image.Height);
            rotatedBmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //make a graphics object from the empty bitmap
            Graphics g = Graphics.FromImage(rotatedBmp);

            //Put the rotation point in the center of the image
            g.TranslateTransform(offset.X, offset.Y);

            //rotate the image
            g.RotateTransform(angle);

            //move the image back
            g.TranslateTransform(-offset.X, -offset.Y);

            //draw passed in image onto graphics object
            g.DrawImage(image, new PointF(0, 0));

            return rotatedBmp;
        }
        private void Reconize(string link, out Image hinhbienso, out string bienso, out string bienso_text)
        {
            System.Diagnostics.Debug.WriteLine("Reconize");
            for (int i = 0; i < box.Length; i++)
            {
                this.Controls.Remove(box[i]);
            }

            hinhbienso = null;
            bienso = "";
            bienso_text = "";
            ProcessImage(link);
            if (PlateImagesList.Count != 0)
            {
                Image<Bgr, byte> src = new Image<Bgr, byte>(PlateImagesList[0].ToBitmap());
                Bitmap grayframe;
                FindContours con = new FindContours();
                Bitmap color;
                int c = con.IdentifyContours(src.ToBitmap(), 50, false, out grayframe, out color, out listRect);
                //int z = con.count;
                pictureBox_BiensoVAO.Image = color;
                hinhbienso = Plate_Draw;
                pictureBox_kytuVAO.Image = grayframe;
                Image<Gray, byte> dst = new Image<Gray, byte>(grayframe);
                grayframe = dst.ToBitmap();
                string zz = "";

                //lọc và sắp xếp số
                List<Bitmap> bmp = new List<Bitmap>();
                List<int> erode = new List<int>();
                List<Rectangle> up = new List<Rectangle>();
                List<Rectangle> dow = new List<Rectangle>();
                int up_y = 0, dow_y = 0;
                bool flag_up = false;

                int di = 0;

                if (listRect == null) return;

                for (int i = 0; i < listRect.Count; i++)
                {
                    Bitmap ch = grayframe.Clone(listRect[i], grayframe.PixelFormat);
                    int cou = 0;
                    full_tesseract.Clear();
                    full_tesseract.ClearAdaptiveClassifier();
                    string temp = full_tesseract.Apply(ch);
                    while (temp.Length > 3)
                    {
                        Image<Gray, byte> temp2 = new Image<Gray, byte>(ch);
                        temp2 = temp2.Erode(2);
                        ch = temp2.ToBitmap();
                        full_tesseract.Clear();
                        full_tesseract.ClearAdaptiveClassifier();
                        temp = full_tesseract.Apply(ch);
                        cou++;
                        if (cou > 10)
                        {
                            listRect.RemoveAt(i);
                            i--;
                            di = 0;
                            break;
                        }
                        di = cou;
                    }
                }

                for (int i = 0; i < listRect.Count; i++)
                {
                    for (int j = i; j < listRect.Count; j++)
                    {
                        if (listRect[i].Y > listRect[j].Y + 100)
                        {
                            flag_up = true;
                            up_y = listRect[j].Y;
                            dow_y = listRect[i].Y;
                            break;
                        }
                        else if (listRect[j].Y > listRect[i].Y + 100)
                        {
                            flag_up = true;
                            up_y = listRect[i].Y;
                            dow_y = listRect[j].Y;
                            break;
                        }
                        if (flag_up == true) break;
                    }
                }

                for (int i = 0; i < listRect.Count; i++)
                {
                    if (listRect[i].Y < up_y + 50 && listRect[i].Y > up_y - 50)
                    {
                        up.Add(listRect[i]);
                    }
                    else if (listRect[i].Y < dow_y + 50 && listRect[i].Y > dow_y - 50)
                    {
                        dow.Add(listRect[i]);
                    }
                }

                if (flag_up == false) dow = listRect;

                for (int i = 0; i < up.Count; i++)
                {
                    for (int j = i; j < up.Count; j++)
                    {
                        if (up[i].X > up[j].X)
                        {
                            Rectangle w = up[i];
                            up[i] = up[j];
                            up[j] = w;
                        }
                    }
                }
                for (int i = 0; i < dow.Count; i++)
                {
                    for (int j = i; j < dow.Count; j++)
                    {
                        if (dow[i].X > dow[j].X)
                        {
                            Rectangle w = dow[i];
                            dow[i] = dow[j];
                            dow[j] = w;
                        }
                    }
                }

                int x = 12;
                int c_x = 0;

                for (int i = 0; i < up.Count; i++)
                {
                    Bitmap ch = grayframe.Clone(up[i], grayframe.PixelFormat);
                    Bitmap o = ch;
                    //ch = con.Erodetion(ch);
                    string temp;
                    if (i < 2)
                    {
                        temp = Ocr(ch, false, true); // nhan dien so
                    }
                    else
                    {
                        temp = Ocr(ch, false, false);// nhan dien chu
                    }

                    zz += temp;
                    box[i].Location = new System.Drawing.Point(x + i * 50, 290);
                    box[i].Size = new Size(50, 100);
                    box[i].SizeMode = PictureBoxSizeMode.StretchImage;
                    box[i].Image = ch;
                    box[i].Update();
                    //this.Controls.Add(box[i]);
                    c_x++;
                }
                zz += "\r\n";
                for (int i = 0; i < dow.Count; i++)
                {
                    Bitmap ch = grayframe.Clone(dow[i], grayframe.PixelFormat);
                    //ch = con.Erodetion(ch);
                    string temp = Ocr(ch, false, true); // nhan dien so
                    zz += temp;
                    box[i + c_x].Location = new System.Drawing.Point(x + i * 50, 390);
                    box[i + c_x].Size = new Size(50, 100);
                    box[i + c_x].SizeMode = PictureBoxSizeMode.StretchImage;
                    box[i + c_x].Image = ch;
                    box[i + c_x].Update();
                    //this.Controls.Add(box[i + c_x]);
                }
                bienso = zz.Replace("\n", "");
                bienso = bienso.Replace("\r", "");
                bienso_text = zz;

            }
        }
        #endregion

        #region XU LY BS RA
        public void ProcessImage_1(string urlImage)
        {
            System.Diagnostics.Debug.WriteLine("ProcessImage");
            PlateImagesList.Clear();
            PlateTextList.Clear();
            //FileStream fs = new FileStream(urlImage, FileMode.Open, FileAccess.Read);
            Image img1 = pictureBox4.Image;
            Bitmap image1 = new Bitmap(img1);
            //fs.Close();

            FindLicensePlate4_1(image1, out Plate_Draw);
        }
        private string Ocr_1(Bitmap image_s, bool isFull, bool isNum = false)
        {
            System.Diagnostics.Debug.WriteLine("ORC BIT MAP 1");
            string temp = "";
            Image<Gray, byte> src = new Image<Gray, byte>(image_s);
            double ratio = 1;
            while (true)
            {
                ratio = (double)CvInvoke.cvCountNonZero(src) / (src.Width * src.Height);
                if (ratio > 0.5) break;
                src = src.Dilate(2);
            }
            Bitmap image = src.ToBitmap();

            TesseractProcessor ocr;
            if (isFull)
                ocr = full_tesseract;
            else if (isNum)
                ocr = num_tesseract;
            else
                ocr = ch_tesseract;

            int cou = 0;
            ocr.Clear();
            ocr.ClearAdaptiveClassifier();
            temp = ocr.Apply(image);
            while (temp.Length > 3)
            {
                Image<Gray, byte> temp2 = new Image<Gray, byte>(image);
                temp2 = temp2.Erode(2);
                image = temp2.ToBitmap();
                ocr.Clear();
                ocr.ClearAdaptiveClassifier();
                temp = ocr.Apply(image);
                cou++;
                if (cou > 10)
                {
                    temp = "";
                    break;
                }
            }
            return temp;
        }
        public void FindLicensePlate4_1(Bitmap image1, out Image plateDraw)
        {
            System.Diagnostics.Debug.WriteLine("Find license plate 4");
            plateDraw = null;
            Image<Bgr, byte> frame;
            bool isface = false;
            Bitmap src;
            Image dst1 = image1;
            HaarCascade haar = new HaarCascade(@"C:\Users\vutri\OneDrive\Máy tính\test\giao dien C# _ 2\Do_an_2\Do_an_2\bin\Debug\output-hv-33-x25.xml");

            System.Diagnostics.Debug.WriteLine("Find license plate 4 - 676");
            for (float i = 0; i <= 20; i = i + 3)
            {
                for (float s = -1; s <= 1 && s + i != 1; s += 2)
                {
                    src = RotateImage_1(dst1, i * s);
                    PlateImagesList.Clear();
                    frame = new Image<Bgr, byte>(src);
                    using (Image<Gray, byte> grayframe = new Image<Gray, byte>(src))
                    {
                        var faces = grayframe.DetectHaarCascade(haar, 1.1, 8, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(0, 0))[0];

                        foreach (var face in faces)
                        {
                            Image<Bgr, byte> tmp = frame.Copy();
                            tmp.ROI = face.rect;

                            frame.Draw(face.rect, new Bgr(Color.Blue), 2);

                            PlateImagesList.Add(tmp);

                            isface = true;
                        }
                        if (isface)
                        {
                            Image<Bgr, byte> showimg = frame.Clone();
                            if (PlateImagesList.Count > 1)
                            {
                                for (int k = 1; k < PlateImagesList.Count; k++)
                                {
                                    if (PlateImagesList[0].Width < PlateImagesList[k].Width)
                                    {
                                        PlateImagesList[0] = PlateImagesList[k];
                                    }
                                }
                            }
                            PlateImagesList[0] = PlateImagesList[0].Resize(400, 400, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
                            return;
                        }
                    }
                }
            }
        }
        public static Bitmap RotateImage_1(Image image, float angle)
        {
            System.Diagnostics.Debug.WriteLine("Rotate image return bitmap");
            if (image == null)
                throw new ArgumentNullException("image");

            PointF offset = new PointF((float)image.Width / 2, (float)image.Height / 2);

            //create a new empty bitmap to hold rotated image
            Bitmap rotatedBmp = new Bitmap(image.Width, image.Height);
            rotatedBmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //make a graphics object from the empty bitmap
            Graphics g = Graphics.FromImage(rotatedBmp);

            //Put the rotation point in the center of the image
            g.TranslateTransform(offset.X, offset.Y);

            //rotate the image
            g.RotateTransform(angle);

            //move the image back
            g.TranslateTransform(-offset.X, -offset.Y);

            //draw passed in image onto graphics object
            g.DrawImage(image, new PointF(0, 0));

            return rotatedBmp;
        }
        private void Reconize_1(string link1, out Image hinhbienso1, out string bienso1, out string bienso_text1)
        {
            System.Diagnostics.Debug.WriteLine("Reconize");
            for (int i = 0; i < box.Length; i++)
            {
                this.Controls.Remove(box[i]);
            }

            hinhbienso1 = null;
            bienso1 = "";
            bienso_text1 = "";
            ProcessImage_1(link1);
            if (PlateImagesList.Count != 0)
            {
                Image<Bgr, byte> src = new Image<Bgr, byte>(PlateImagesList[0].ToBitmap());
                Bitmap grayframe;
                FindContours con = new FindContours();
                Bitmap color;
                int c = con.IdentifyContours(src.ToBitmap(), 50, false, out grayframe, out color, out listRect);
                //int z = con.count;
                pictureBox_BiensoRA.Image = color;
                hinhbienso1 = Plate_Draw;
                pictureBox_kytuRA.Image = grayframe;
                Image<Gray, byte> dst = new Image<Gray, byte>(grayframe);
                grayframe = dst.ToBitmap();
                string zz = "";

                //lọc và sắp xếp số
                List<Bitmap> bmp = new List<Bitmap>();
                List<int> erode = new List<int>();
                List<Rectangle> up = new List<Rectangle>();
                List<Rectangle> dow = new List<Rectangle>();
                int up_y = 0, dow_y = 0;
                bool flag_up = false;

                int di = 0;

                if (listRect == null) return;

                for (int i = 0; i < listRect.Count; i++)
                {
                    Bitmap ch = grayframe.Clone(listRect[i], grayframe.PixelFormat);
                    int cou = 0;
                    full_tesseract.Clear();
                    full_tesseract.ClearAdaptiveClassifier();
                    string temp = full_tesseract.Apply(ch);
                    while (temp.Length > 3)
                    {
                        Image<Gray, byte> temp2 = new Image<Gray, byte>(ch);
                        temp2 = temp2.Erode(2);
                        ch = temp2.ToBitmap();
                        full_tesseract.Clear();
                        full_tesseract.ClearAdaptiveClassifier();
                        temp = full_tesseract.Apply(ch);
                        cou++;
                        if (cou > 10)
                        {
                            listRect.RemoveAt(i);
                            i--;
                            di = 0;
                            break;
                        }
                        di = cou;
                    }
                }

                for (int i = 0; i < listRect.Count; i++)
                {
                    for (int j = i; j < listRect.Count; j++)
                    {
                        if (listRect[i].Y > listRect[j].Y + 100)
                        {
                            flag_up = true;
                            up_y = listRect[j].Y;
                            dow_y = listRect[i].Y;
                            break;
                        }
                        else if (listRect[j].Y > listRect[i].Y + 100)
                        {
                            flag_up = true;
                            up_y = listRect[i].Y;
                            dow_y = listRect[j].Y;
                            break;
                        }
                        if (flag_up == true) break;
                    }
                }

                for (int i = 0; i < listRect.Count; i++)
                {
                    if (listRect[i].Y < up_y + 50 && listRect[i].Y > up_y - 50)
                    {
                        up.Add(listRect[i]);
                    }
                    else if (listRect[i].Y < dow_y + 50 && listRect[i].Y > dow_y - 50)
                    {
                        dow.Add(listRect[i]);
                    }
                }

                if (flag_up == false) dow = listRect;

                for (int i = 0; i < up.Count; i++)
                {
                    for (int j = i; j < up.Count; j++)
                    {
                        if (up[i].X > up[j].X)
                        {
                            Rectangle w = up[i];
                            up[i] = up[j];
                            up[j] = w;
                        }
                    }
                }
                for (int i = 0; i < dow.Count; i++)
                {
                    for (int j = i; j < dow.Count; j++)
                    {
                        if (dow[i].X > dow[j].X)
                        {
                            Rectangle w = dow[i];
                            dow[i] = dow[j];
                            dow[j] = w;
                        }
                    }
                }

                int x = 12;
                int c_x = 0;

                for (int i = 0; i < up.Count; i++)
                {
                    Bitmap ch = grayframe.Clone(up[i], grayframe.PixelFormat);
                    Bitmap o = ch;
                    //ch = con.Erodetion(ch);
                    string temp;
                    if (i < 2)
                    {
                        temp = Ocr_1(ch, false, true); // nhan dien so
                    }
                    else
                    {
                        temp = Ocr_1(ch, false, false);// nhan dien chu
                    }

                    zz += temp;
                    box[i].Location = new System.Drawing.Point(x + i * 50, 290);
                    box[i].Size = new Size(50, 100);
                    box[i].SizeMode = PictureBoxSizeMode.StretchImage;
                    box[i].Image = ch;
                    box[i].Update();
                    //this.Controls.Add(box[i]);
                    c_x++;
                }
                zz += "\r\n";
                for (int i = 0; i < dow.Count; i++)
                {
                    Bitmap ch = grayframe.Clone(dow[i], grayframe.PixelFormat);
                    //ch = con.Erodetion(ch);
                    string temp = Ocr_1(ch, false, true); // nhan dien so
                    zz += temp;
                    box[i + c_x].Location = new System.Drawing.Point(x + i * 50, 390);
                    box[i + c_x].Size = new Size(50, 100);
                    box[i + c_x].SizeMode = PictureBoxSizeMode.StretchImage;
                    box[i + c_x].Image = ch;
                    box[i + c_x].Update();
                    //this.Controls.Add(box[i + c_x]);
                }
                bienso1 = zz.Replace("\n", "");
                bienso1 = bienso1.Replace("\r", "");
                bienso_text1 = zz;

            }
        }
        #endregion
    }
}

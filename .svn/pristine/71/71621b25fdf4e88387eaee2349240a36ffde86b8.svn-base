using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Navigation;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;



namespace bien4read
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        List<FileInfo> m_pathList = new List<FileInfo>();
        String MY_FOLDER = "b4r";
        //String TMP_FOLDER = "b4r_tmp";
        String REG_KEY = "b4r";
        String REG_KEY_PATH = "path";
        String PATH_DEFAULT = @"D:\test";
        RegistryKey m_reg;
        private string m_msg;
        iniUtil m_ini;
        GlobalValue gv;

        public MainWindow()
        {
            InitializeComponent();

            gv = new GlobalValue();

            Dictionary<int, string> dicSplit = new Dictionary<int, string> { { 1, "1→2" }, { 2, "2←1" } };
            cbSplit.ItemsSource = dicSplit;
            cbSplit.DisplayMemberPath = "Value";
            cbSplit.SelectedValuePath = "Key";
            cbSplit.SelectedValue = 1;

            //현재 프로그램이 실행되고 있는정보 가져오기: 디버깅 모드라면 bin/debug/프로그램명.exe
            FileInfo exefileinfo = new FileInfo(System.Windows.Forms.Application.ExecutablePath);
            string path = exefileinfo.Directory.FullName.ToString();  //프로그램 실행되고 있는데 path 가져오기
            string fileName = @"\config.ini";  //파일명
            //만약 현재 실행 되는 경로가 아닌 특정한 위치를 원한다면 위에 과정 상관없이 바로  경로셋팅 해 주면 된다. (예: c:\config.ini) 
            string filePath = path + fileName;   //ini 파일 경로 

            m_ini = new iniUtil(filePath);   // 만들어 놓았던 iniUtil 객체 생성(생성자 인자로 파일경로 정보 넘겨줌)
            String lastPath = m_ini.GetIniValue(gv.SECT_PATH, gv.KEY_PATH);

            if (lastPath.Length == 0) lastPath = PATH_DEFAULT;

            //string regPath = readRegPath();
            tbPath.Text = lastPath;

            addMsg("### 프로그램이 시작되었습니다. ###");
        }

        private string readRegPath()
        {
            m_reg = Registry.LocalMachine.CreateSubKey("Software").CreateSubKey(REG_KEY);

            String path = Convert.ToString(m_reg.GetValue(REG_KEY_PATH, PATH_DEFAULT));
            return path;
        }

        public delegate void AddMsgDelegate(string msg);

        public void addMsgInvoke(string msg)
        {
            object parmaMsg = new object();
            parmaMsg = msg;
            //tbMsg.Dispatcher.BeginInvoke(new AddMsgDelegate(addMsg), parmaMsg);
            tbMsg.Dispatcher.Invoke(new AddMsgDelegate(addMsg), parmaMsg);
            Thread.Sleep(10);
        }

        public void addMsg(string msg)
        {
            if (tbMsg.Dispatcher.CheckAccess())
            {
                // 현재시각 구하기
                String t = System.DateTime.Now.ToString("[hh:mm:ss]:");
                string newMsg = t + msg + "\n";
                tbMsg.Text = tbMsg.Text.Insert(0, newMsg);
                //tbMsg.SelectedText = newMsg;
                //tbMsg.Text = t + v + "\n" + tbMsg.Text;

                int lineIndex = tbMsg.GetLineIndexFromCharacterIndex(tbMsg.CaretIndex);
                tbMsg.ScrollToLine(lineIndex);

                //this.Dispatcher.Invoke((ThreadStart)(() => { }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
            else
            {
                object parmaMsg = new object();
                parmaMsg = msg;
                tbMsg.Dispatcher.Invoke(new AddMsgDelegate(addMsg), parmaMsg);
                Thread.Sleep(10);
            }
        }

        private void btnPath_Click(object sender, RoutedEventArgs e)
        {
            //경로를 선택할 수 있는 창을 보여준다.
            FolderBrowserDialog d = new FolderBrowserDialog();
            d.SelectedPath = tbPath.Text;
            if (d.ShowDialog() != System.Windows.Forms.DialogResult.OK) {
                return;
            }

            String selPath = d.SelectedPath;
            changePath(selPath);
            //tbPath.Text = selPath;
        }

        private void changePath(string selPath)
        {
            m_ini.SetIniValue(gv.SECT_PATH, gv.KEY_PATH, selPath);
            //m_reg.SetValue(REG_KEY_PATH, selPath);
            tbPath.Text = selPath;
        }

        private async void btnChange_Click(object sender, RoutedEventArgs e)
        {
            // 해당 경로가 유효한지 확인
            String selPath = tbPath.Text;

            // 선택한 경로 이하의 모든 폴더 및 파일을 읽는다.
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(selPath);
            if (di.Exists == false) {
                System.Windows.MessageBox.Show("유효한경로를 선택하세요");
                return;
            }


            // 기존 정보 모두 삭제
            //m_pathList.Clear();

            ControlFile cf = new ControlFile();

            // settings for read file list
            cf.clear();
            cf.setMain(this);
            cf.setDirectoryInfo(di);


            // settings for transform
            cf.setAddEmptyLine(tbAddEmptyLine.Text);
            cf.setMargin(tbMarginUp.Text, tbMarginLR.Text, tbMarginDown.Text, tbMarginMin.Text);
            cf.setCbSplit(cbSplit.SelectedValue);

            
            //Thread readThread = new Thread(new ThreadStart(cf.readThead));
            // Start the thread
            //readThread.Start();

            Task readTask = new Task(new Action(cf.processThead));
            readTask.Start();
            //readTask.Wait();


            //addFilesToList(di);
            
            // Spin for a while waiting for the started thread to become
            // alive:
            //while (readThread.IsAlive) {
            //    Thread.Sleep(100);
            //};

            //BackgroundWorker worker = new BackgroundWorker();
            //worker.DoWork += new DoWorkEventHandler(yourEventHandler);            
            //worker.RunWorkerAsync(cf);
            //Task.Factory.StartNew(() =>
            //{
            //    cf.processThead();
            //});

            //await Task.Factory.StartNew(() => cf.processThead());
            //Task.Factory.StartNew(cf.processThead);


            return;
            //m_pathList = cf.getPathList();


            //Thread transformThread = new Thread(new ThreadStart(cf.transformFiles));
           
            //transformThread.Start();

            //while (transformThread.IsAlive) {
            //    Thread.Sleep(100);
            //};

            //transformThread.Join();
        }

        void yourEventHandler(object sender, DoWorkEventArgs e) {
            
            ControlFile cf = e.Argument as ControlFile;
            cf.processThead();
        }

        
        private Encoding findFileEncoding(FileInfo f)
        {
            FileStream fs = null;
            int bomByteLen = 4;

            try
            {
                fs = File.OpenRead(f.FullName);
                byte[] BOMBytes = new byte[bomByteLen];
                fs.Read(BOMBytes, 0, bomByteLen);

                if (BOMBytes == null)
                    throw new ArgumentNullException("Must provide a valid BOM byte array!", "BOMBytes");

                if (BOMBytes.Length < 2)
                    return null;

                if (BOMBytes[0] == 0xff
                    && BOMBytes[1] == 0xfe
                    && (BOMBytes.Length < 4
                        || BOMBytes[2] != 0
                        || BOMBytes[3] != 0
                        )
                    )
                    return Encoding.Unicode;

                if (BOMBytes[0] == 0xfe
                    && BOMBytes[1] == 0xff
                    )
                    return Encoding.BigEndianUnicode;

                if (BOMBytes.Length < 3)
                    return null;

                if (BOMBytes[0] == 0xef && BOMBytes[1] == 0xbb && BOMBytes[2] == 0xbf)
                    return Encoding.UTF8;

                if (BOMBytes[0] == 0x2b && BOMBytes[1] == 0x2f && BOMBytes[2] == 0x76)
                    return Encoding.UTF7;

                if (BOMBytes.Length < 4)
                    return null;

                if (BOMBytes[0] == 0xff && BOMBytes[1] == 0xfe && BOMBytes[2] == 0 && BOMBytes[3] == 0)
                    return Encoding.UTF32;

                if (BOMBytes[0] == 0 && BOMBytes[1] == 0 && BOMBytes[2] == 0xfe && BOMBytes[3] == 0xff)
                    return Encoding.GetEncoding(12001);

                return null;

            }
            finally {
                if (fs != null) {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }

       
        /**
         * 드래그 드랍을 위한 코드
         * */
        private void tbPath_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
            {
                string[] droppedPaths = e.Data.GetData(System.Windows.DataFormats.FileDrop, true) as string[];
                string path = droppedPaths[0];

                DirectoryInfo di = new DirectoryInfo(path);
                string rstPath = string.Empty;

                if (di.Exists)
                {                    
                    rstPath = path;
                }
                else {
                    FileInfo fi = new FileInfo(path);
                    rstPath = fi.DirectoryName;
                }

                changePath(rstPath);
            }
        }

        private void tbPath_PreviewDragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
        }

        private void tbPath_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Handled = true;
        }

        /**
         * XAML 코드에서, 숫자 만을 받기 위한 확인 코드
         **/
        private void NumericOnly(System.Object sender, System.Windows.Input.TextCompositionEventArgs e) {
            e.Handled = IsTextNumeric(e.Text);
        }

        private bool IsTextNumeric(string str) {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");
            return reg.IsMatch(str);
        }


      
    }
}

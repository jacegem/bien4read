using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace bien4read
{
    class ControlFile
    {
        Boolean m_bRead = true;
        private System.IO.DirectoryInfo m_di;
        String MY_FOLDER = "b4r";
        List<FileInfo> m_pathList = new List<FileInfo>();
        private System.Windows.Controls.TextBox tbMsg;
        private MainWindow m_mainWindow;
        private string m_addEmptyLine;
        Bitmap m_bm;
        Image m_img;
        BitmapData m_bitmapData;
        Stream m_stream;
        CropPoint m_cpLeft;
        CropPoint m_cpRight;
        byte[] m_pixels;
        private int m_marginUp;
        private int m_marginLR;
        private int m_marginDown;
        private int m_marginMin;
        private object m_cbSplitSelectedValue;

        public void readFileList() {
            m_bRead = true;
           
            addFilesToList(m_di);
            m_bRead = false;

        }

        internal void setDirectoryInfo(System.IO.DirectoryInfo di)
        {
            m_di = di;
        }

        private void addFilesToList(DirectoryInfo di)
        {
            // 마지막 경로가 이 프로그램을 통해서 만든 경로라면 처리하지 않는다.->->
            int totalLen = di.FullName.Length;
            int myLen = MY_FOLDER.Length;

            if (totalLen > myLen)
            {
                String lastDirectory = di.FullName.Substring(totalLen - myLen, myLen);
                if (lastDirectory == MY_FOLDER)
                    return;
            }

            // 처리 대상 확장자
            string[] exts = { "txt", "zip" };

            // 파일 정보 가져오기
            foreach (System.IO.FileInfo f in di.GetFiles())
            {
                foreach (string ext in exts)
                {
                    if (f.Extension.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                    {
                        m_pathList.Add(f);
                        addMsg(f.FullName + " 파일 추가");
                    }
                }
            }

            // 폴더 정보 가져오기
            foreach (System.IO.DirectoryInfo d in di.GetDirectories())
            {
                addFilesToList(d);
            }
        }

        internal void clear()
        {
            m_pathList.Clear();
        }

        private void addMsg(string v)
        {

            m_mainWindow.addMsgInvoke(v);
            //Thread.Sleep(100);
        }

        internal void setTextBoxMsg(System.Windows.Controls.TextBox tbMsg)
        {
            this.tbMsg = tbMsg;
        }

        internal void setMain(MainWindow mainWindow)
        {
            m_mainWindow = mainWindow;
        }

        public List<FileInfo> getPathList()
        {
            return m_pathList;
        }

        private string getSavePath(FileInfo f)
        {
            return f.DirectoryName + "\\" + MY_FOLDER + "\\";
        }

        private void createDirectory(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists == false)
                di.Create();
        }

        private void deleteDirectory(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            di.Delete(true);
        }

        private String readFileAsUtf8(string filename)
        {
            Encoding encoding = Encoding.Default;
            String original = String.Empty;

            using (StreamReader sr = new StreamReader(filename, Encoding.Default))
            {
                original = sr.ReadToEnd();
                encoding = sr.CurrentEncoding;
                sr.Close();
            }

            if (encoding == Encoding.UTF8)
                return original;

            byte[] encBytes = encoding.GetBytes(original);
            byte[] utf8Bytes = Encoding.Convert(encoding, Encoding.UTF8, encBytes);
            return Encoding.UTF8.GetString(utf8Bytes);
        }

        private string removeNewLine(string utf8Text)
        {
            string rst = string.Empty;
            string[] stringSeperators = new string[] { "\r\n" };
            string[] lines = utf8Text.Split(stringSeperators, StringSplitOptions.None);

            int insertNewLine = Convert.ToInt32(m_addEmptyLine);
            int addedLine = 0;

            for (int i = 0; i < lines.Length - 1; i++)
            {
                string sCur = lines[i];
                string sNext = lines[i + 1];

                string spacePattern = @"(?<=\w)\s+";
                Regex reg = new Regex(spacePattern);
                sCur = reg.Replace(sCur, " ");

                bool bAddNewLine = false;

                string pat = @"^\w";
                Regex r = new Regex(pat, RegexOptions.IgnoreCase);
                Match m = r.Match(sNext);

                if (m.Success == false || sCur.Length < 30)
                {
                    bAddNewLine = true;
                }
                else
                {
                    addedLine++;
                }

                if (bAddNewLine)
                {
                    sCur += "\r\n";

                    if (addedLine != 0 && addedLine > insertNewLine)
                    {
                        sCur += "\r\n";
                    }
                    addedLine = 0;
                }

                rst += sCur;
            }

            rst += lines[lines.Length - 1];

            return rst;
        }



        private void proccessZipTxt(FileInfo f, ZipArchiveEntry entry)
        {
            String savePath = getSavePath(f);
            String utf8Text = readEntryAsUtf8(entry.Open());
            utf8Text = removeNewLine(utf8Text);
            System.IO.File.WriteAllText(savePath + f.Name + "_" + entry.Name, utf8Text, Encoding.UTF8);
            addMsg(f.Name + " : " + entry.Name + " 파일 변환");
        }

        private string readEntryAsUtf8(Stream s)
        {
            Encoding encoding = Encoding.Default;
            String original = String.Empty;

            using (StreamReader sr = new StreamReader(s, Encoding.Default))
            {
                original = sr.ReadToEnd();
                encoding = sr.CurrentEncoding;
                sr.Close();
            }

            if (encoding == Encoding.UTF8)
                return original;

            byte[] encBytes = encoding.GetBytes(original);
            byte[] utf8Bytes = Encoding.Convert(encoding, Encoding.UTF8, encBytes);
            return Encoding.UTF8.GetString(utf8Bytes);
        }

        private bool isImageFile(ZipArchiveEntry entry)
        {
            string[] exts = { "jpg", "jpeg", "bmp", "gif" };

            foreach (string ext in exts)
            {
                if (isExtFile(entry.FullName, ext))
                    return true;
            }

            return false;
        }

        private bool isExtFile(string name, string ext)
        {
            if (name.EndsWith("." + ext, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }


        internal void transformFiles()
        {
            // 리스트에 있는 것들을 하나씩 꺼내서, 처리
            Parallel.ForEach(m_pathList, f => transform(f));
            
            //foreach (FileInfo f in m_pathList) transform(f);
            
            addMsg("### 변환 완료 ###");
        }

        private void transform(FileInfo f)
        {
            

            // 변환 폴더 생성
            String savePath = getSavePath(f);
            createDirectory(savePath);

            // txt 파일 처리
            if (f.Extension.Equals(".txt"))
            {
                //addMsg(f.Name + " is txt file");

                String utf8Text = readFileAsUtf8(f.FullName);
                utf8Text = removeNewLine(utf8Text);

                //Console.WriteLine("===============rst==============");
                //Console.WriteLine(utf8Text);
                //string name = System.IO.Path.GetFileNameWithoutExtension(f.Name);

                System.IO.File.WriteAllText(savePath + f.Name, utf8Text, Encoding.UTF8);


            }
            // zip 파일 처리
            else if (f.Extension.Equals(".zip"))
            {
                string tempDirectory = savePath + f.Name + "_TMP\\";
                
                Boolean bNeddTempProccess = false;

                try
                {

                    using (ZipArchive archive = ZipFile.OpenRead(f.FullName))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            // txt 파일이면,
                            if (isExtFile(entry.FullName, "txt"))
                            {
                                proccessZipTxt(f, entry);
                            }
                            // 이미지 파일이면,
                            // 참고: http://stackoverflow.com/questions/734930/how-to-crop-an-image-using-c
                            if (isImageFile(entry))
                            {
                                if (bNeddTempProccess == false) {
                                    createDirectory(tempDirectory);
                                }
                             
                                bNeddTempProccess = true;

                                proccessZipImage(f, entry, tempDirectory);
                            }
                        }
                    }
                }
                catch
                {
                    return;
                }

                if (bNeddTempProccess)
                {
                    // 기존 파일 제거
                    string rstFile = getSavePath(f) + getResultArchiveName(f.Name);
                    deleteFile(rstFile);
                    // zip 파일 생성
                    ZipFile.CreateFromDirectory(tempDirectory, rstFile);
                    // 임시폴더 처리하고 삭제
                    deleteDirectory(tempDirectory);    
                }
            }

            addMsg(f.Name + " 파일 변환 완료");
        }

        private string getResultArchiveName(string name)
        {
            return Path.GetFileNameWithoutExtension(name) + ".cbz";
        }

        private void deleteFile(string filePath)
        {
            FileInfo f = new FileInfo(filePath);
            if (f.Exists)
                f.Delete();
        }

        internal void setAddEmptyLine(string p)
        {
            m_addEmptyLine = p;
        }

        private void proccessZipImage(FileInfo f, ZipArchiveEntry entry, string tempPath)
        {
            //addMsg(f.Name + " : " + entry.Name + " 처리중");

            try
            {
                string ext = System.IO.Path.GetExtension(entry.FullName);
                Stream stream = entry.Open();

                Image img = Image.FromStream(stream);
                Bitmap bm = new Bitmap(img);

                stream.Close();

                //bm.Save(savePath + f.Name + "_" + entry.Name, System.Drawing.Imaging.ImageFormat.Gif);
                //bmOriginal.Save(getSavePath(f) + f.Name + "_org_" + entry.Name, System.Drawing.Imaging.ImageFormat.Gif);
                //bm.Save(getSavePath(f) + f.Name + "_clone_" + entry.Name, System.Drawing.Imaging.ImageFormat.Gif);

                // Get width and height of bitmap
                int width = bm.Width;
                int height = bm.Height;

                // get total locked pixels count
                int pixelCount = width * height;

                // get source bitmap pixel format size
                int depth = System.Drawing.Bitmap.GetPixelFormatSize(bm.PixelFormat);

                // Create rectangle to lock
                Rectangle rect = new Rectangle(0, 0, width, height);

                // Check if bpp (Bits Per Pixel) is 8, 24, or 32
                if (depth != 8 && depth != 24 && depth != 32)
                {
                    throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
                }

                // Lock bitmap and return bitmap data
                //m_bitmapData = null;
                m_bitmapData = bm.LockBits(rect, ImageLockMode.ReadOnly, bm.PixelFormat);


                // create byte array to copy pixel values
                int step = depth / 8;

                
                byte[] pixels = new byte[pixelCount * step];

                IntPtr Iptr = m_bitmapData.Scan0;

                // Copy data from pointer to array
                Marshal.Copy(Iptr, pixels, 0, pixels.Length);

                //m_pixels = imageToByteArray(m_bm);


                for (int i = 0; i < pixelCount * step; i++)
                {
                    if (pixels[i] < 5)
                    {
                        //Console.WriteLine("NOT ZERO: {0}", i);
                        //break;
                    }
                }

                // unlockbits
                //m_bm.UnlockBits(m_bitmapData);

                //Bitmap bm = bmOriginal.Clone(new Rectangle(0, 0, width, height), PixelFormat.Format24bppRgb);

                ImageFormat imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;

                if (ext.Equals(".gif", StringComparison.OrdinalIgnoreCase))
                    imageFormat = System.Drawing.Imaging.ImageFormat.Gif;
                else if (ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase))
                    imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                else if (ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase))
                    imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                else if (ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
                    imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                else if (ext.Equals(".png", StringComparison.OrdinalIgnoreCase))
                    imageFormat = System.Drawing.Imaging.ImageFormat.Png;


                string savePath = tempPath;
                //string savePath = getSavePath(f);

                int searchValue = 5;

                // 반으로 나눠야 하는가?
                if (width > height)
                {
                    CropPoint cpLeft = new CropPoint(pixels, 0, 0, width / 2, height, depth, searchValue, width, height, m_marginMin);
                    CropPoint cpRight = new CropPoint(pixels, width / 2, 0, width, height, depth, searchValue, width, height, m_marginMin);

                    cpLeft.addMargin(m_marginUp, m_marginLR, m_marginDown);
                    cpRight.addMargin(m_marginUp, m_marginLR, m_marginDown);

                    Image img1, img2;

                    // 순서에 따라 저장
                    if (m_cbSplitSelectedValue.Equals(1))
                    {
                        //img1 = cropImage(m_img, m_cpLeft.getRectangle());
                        //img2 = cropImage(m_img, m_cpRight.getRectangle());
                        img1 = bm.Clone(cpLeft.getRectangle(), img.PixelFormat);
                        img2 = bm.Clone(cpRight.getRectangle(), bm.PixelFormat);
                    }
                    else
                    {
                        //img1 = cropImage(m_img, m_cpRight.getRectangle());
                        //img2 = cropImage(m_img, m_cpLeft.getRectangle());
                        img1 = bm.Clone(cpRight.getRectangle(), bm.PixelFormat);
                        img2 = bm.Clone(cpLeft.getRectangle(), bm.PixelFormat);
                    }

                    cpLeft =  cpRight = null;

                    img1.Save(savePath + Path.GetFileNameWithoutExtension(f.Name) + modifyName(entry.Name, "_c1_"), imageFormat);
                    img2.Save(savePath + Path.GetFileNameWithoutExtension(f.Name) + modifyName(entry.Name, "_c2_"), imageFormat);
                }

                else
                {
                    CropPoint cp = new CropPoint(pixels, 0, 0, width, height, depth, searchValue, width, height, m_marginMin);
                    cp.addMargin(m_marginUp, m_marginLR, m_marginDown);

                    Image imgCrop = cropImage(img, cp.getRectangle());
                    //Bitmap bmCrop = m_bm.Clone(cp.getRectangle(), m_bm.PixelFormat);
                    imgCrop.Save(savePath + f.Name + modifyName(entry.Name, "_c2_"), imageFormat);
                }

            }
            catch (Exception e){
                Console.WriteLine("================================================");
                Console.WriteLine(e.ToString());
            }
        }

        private static Image cropImage(Image img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }


        private string modifyName(string name, string ins)
        {
            string filename = Path.GetFileNameWithoutExtension(name);
            string extentsion = Path.GetExtension(name);
            return filename + ins + extentsion;
        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            return ms.ToArray();
        }


        internal void setMargin(string marginUp, string marginLR, string marginDown, string marginMin)
        {
            m_marginUp = Convert.ToInt32(marginUp);
            m_marginLR = Convert.ToInt32(marginLR);
            m_marginDown = Convert.ToInt32(marginDown);
            m_marginMin = Convert.ToInt32(marginMin);

        }

        internal void setCbSplit(object p)
        {
            m_cbSplitSelectedValue = p;
        }


        internal async void processThead()
        {
            readFileList();

            transformFiles();
        }
    }
}

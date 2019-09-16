using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows.Forms;
using VisualUIAVerify.Controls;

namespace VisualUIAVerify.Forms
{
    public partial class FileCreator : Form
    {
        string fileDirPath = @"C:\CFW";
        public string xmlLocation;
        string strSearchDirectory;
        string txtFileLocation = null;
        string txtFileName = null;
        bool createButton = true;
        private const string notAllowedChar = @"/';}{~`!^*?<>|""";

        public FileCreator()
        {
            InitializeComponent();
            FileLocation.Text = fileDirPath;
            createButton = true;
        }

        public string NewFileLocation
        {
            get { return xmlLocation; }
        }
        private bool NewMethod()
        {
            string temp = FileName.Text;
            if (!FileName.Text.ToUpper().Contains(".XML"))
               FileName.Text = FileName.Text + ".xml";
            if (!IsValidPath(FileLocation.Text))
            {
                MessageBox.Show("Invalid folder path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
            else if (string.IsNullOrEmpty(FileName.Text))
            {
                MessageBox.Show("File name cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                FileName.Text = temp;
                return true;
            }
            else if (string.IsNullOrEmpty(Path.GetExtension(FileName.Text)))
            {
                MessageBox.Show("Invalid file name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                FileName.Text = temp;
                return true;
            }
            else if (!Path.GetExtension(FileName.Text).ToUpper().Equals(".XML"))
            {
                MessageBox.Show("Invalid file name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                FileName.Text = temp;
                return true;
            }
            else if (notAllowedChar.Any(s => FileName.Text.Contains(s)))
            {
                MessageBox.Show("A file name cannot contain any of the following characters " + @"/';}{~`!^*?<>|""",
                                    "Error",
                                   MessageBoxButtons.OK,
                                  MessageBoxIcon.Error,
                                  MessageBoxDefaultButton.Button3);
                FileName.Text = txtFileName;
                return true;
            }
            else if (!ValidateFileName(FileName.Text))
            {
                MessageBox.Show("Invalid file name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                FileName.Text = temp;
                return true;
            }
            else
            {
                var vf = FileLocation.Text.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
                if (vf[vf.Length - 1].Equals(Path.GetFileNameWithoutExtension(FileName.Text)))
                    return false;
                FileLocation.Text = FileLocation.Text + @"\" + Path.GetFileNameWithoutExtension(FileName.Text);
                txtFileName = FileName.Text;
                txtFileLocation = FileLocation.Text;
            }
            return false;
        }
        private void Create_Button(object sender, EventArgs e)
        {
            if (NewMethod())
                return;
            if(IsValidPath(FileLocation.Text))
            createButton = false;
            //FileLocation.Text = FileLocation.Text + @"\" + Path.GetFileNameWithoutExtension(FileName.Text);
            //txtFileName = FileName.Text;
            //txtFileLocation = FileLocation.Text;
            
            if (string.IsNullOrEmpty(FileName.Text) || string.IsNullOrEmpty(FileLocation.Text))
            {
                MessageBox.Show("File Name and Folder Location should not be empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (!ValidateFileName(FileName.Text) || !TryGetFullPath(FileLocation.Text))
            {
                if (!ValidateFileName(FileName.Text))
                    MessageBox.Show("Invalid file name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (!TryGetFullPath(FileLocation.Text))
                    MessageBox.Show("Invalid folder path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string folderName = string.Concat(FileLocation.Text.Substring(FileLocation.Text.LastIndexOf("\\"),
                FileLocation.Text.Length - FileLocation.Text.LastIndexOf("\\")).Replace(@"\", ""));

            FileLocation.Text = Path.GetFullPath(FileLocation.Text);

            if (FileLocation.Text == fileDirPath)
            {
                DialogResult dr = MessageBox.Show("Folder Name: '" + folderName + "' already exists. \nClick OK to enter File Location", "Error", MessageBoxButtons.OK);
                FileLocation.Focus();
                return;
            }
            try
            {
                // check File not created
                bool fileExists = File.Exists(FileLocation.Text + @"\" + FileName.Text);
                bool folderExists = Directory.Exists(FileLocation.Text);
                if (fileExists)
                {
                    MessageBox.Show("Folder &File already Exists", "Warining",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Exclamation,
                                  MessageBoxDefaultButton.Button1);
                    xmlLocation = FileLocation.Text + "\\" + FileName.Text;
                    return;
                }
                else if (folderExists)
                {
                    DialogResult dr = MessageBox.Show("Folder already exists but file not exists.\nDo you want to create xml file as folder already exists? ", "Warining",
                                  MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Exclamation,
                                  MessageBoxDefaultButton.Button1);

                    if (dr == DialogResult.Yes)
                    {
                        File.Create(FileLocation.Text + "\\" + FileName.Text.Split('.')[0] + ".xml");
                        //Strore file location
                        xmlLocation = FileLocation.Text + "\\" + FileName.Text;
                        DialogResult d = MessageBox.Show("File creation successful. File Name: " + xmlLocation /*string.Concat(FileLocation.Text, @"\", FileName.Text)*/, "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (d == DialogResult.OK)
                        {
                            this.Close();
                        }
                    }
                    else if (dr == DialogResult.No)
                    {
                        FileLocation.Focus();
                    }
                }
                else
                {
                    Directory.CreateDirectory(FileLocation.Text);
                    //create xml file 
                    File.Create(FileLocation.Text + "\\" + FileName.Text.Split('.')[0] + ".xml");
                    //Strore file location
                    xmlLocation = FileLocation.Text + "\\" + FileName.Text;
                    DialogResult d = MessageBox.Show("'File Created Successfully:' " + xmlLocation /*string.Concat(FileLocation.Text, @"\", FileName.Text)*/, "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (d == DialogResult.OK)
                    {
                        this.Close();
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error Cannot Access Location", "Warining",
                                 MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation,
                                MessageBoxDefaultButton.Button1);
            }
            AutomationElementTreeControl aetc = new AutomationElementTreeControl();
            aetc.FileName = NewFileLocation;
        }

        public static bool ValidateFileName(string fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName)) { return false; }
            if (fileName.Contains(" ")) { return false; }
            string temp = notAllowedChar + ".,+_)(*&^%$#@!~";
            bool flag = true;
            string extention = Path.GetExtension(fileName);
            string fName = Path.GetFileNameWithoutExtension(fileName);
            try
            {
                //flag &= extention != "";
                //flag &= extention.ToUpper().Equals(".XML");
                flag &= !temp.Any(s => fName.Contains(s));
                flag &= !string.IsNullOrEmpty(fName);
                return flag;
            }
            catch { return false; }
        }
        /// <summary>
        /// Gets a value that indicates whether <paramref name="path"/>
        /// is a valid path.
        /// </summary>
        /// <returns>Returns <c>true</c> if <paramref name="path"/> is a
        /// valid path; <c>false</c> otherwise. Also returns <c>false</c> if
        /// the caller does not have the required permissions to access
        /// <paramref name="path"/>.
        /// </returns>
        /// <seealso cref="Path.GetFullPath"/>
        /// <seealso cref="TryGetFullPath"/>
        public static bool IsValidPath(string path)
        {
            string result;
            return TryGetFullPath(path, out result);
        }

        /// <summary>
        /// Returns the absolute path for the specified path string. A return
        /// value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain absolute
        /// path information.
        /// </param>
        /// <param name="result">When this method returns, contains the absolute
        /// path representation of <paramref name="path"/>, if the conversion
        /// succeeded, or <see cref="String.Empty"/> if the conversion failed.
        /// The conversion fails if <paramref name="path"/> is null or
        /// <see cref="String.Empty"/>, or is not of the correct format. This
        /// parameter is passed uninitialized; any value originally supplied
        /// in <paramref name="result"/> will be overwritten.
        /// </param>
        /// <returns><c>true</c> if <paramref name="path"/> was converted
        /// to an absolute path successfully; otherwise, false.
        /// </returns>
        /// <seealso cref="Path.GetFullPath"/>
        /// <seealso cref="IsValidPath"/>
        public static bool TryGetFullPath(string path, out string result)
        {
            result = String.Empty;
            if (String.IsNullOrWhiteSpace(path)) { return false; }
            bool status = false;

            try
            {
                result = Path.GetFullPath(path);
                status = Directory.Exists(Path.GetPathRoot(path));
            }
            catch (ArgumentException) { }
            catch (SecurityException) { }
            catch (NotSupportedException) { }
            catch (PathTooLongException) { }

            return status;
        }
        private static bool TryGetFullPath(string path, bool exactPath = true)
        {
            if (String.IsNullOrWhiteSpace(path)) { return false; }
            bool isValid;
            try
            {
                string fullPath = Path.GetFullPath(path);
                if (exactPath)
                {
                    string root = Path.GetPathRoot(path);
                    isValid = string.IsNullOrEmpty(root.Trim(new char[] { '\\', '/' })) == false;
                }
                else
                {
                    isValid = Path.IsPathRooted(path);
                }
            }
            catch (Exception)
            {
                isValid = false;
            }
            return isValid;
        }
        public void BtnBrowser_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(fileDirPath))
                {
                    Directory.CreateDirectory(fileDirPath);
                }
                FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
                folderBrowser.SelectedPath = fileDirPath;
                if (folderBrowser.ShowDialog() == DialogResult.OK)
                {
                    strSearchDirectory = folderBrowser.SelectedPath;
                    FileLocation.Text = strSearchDirectory;
                }
            }
            catch
            {
                MessageBox.Show("Error Cannot Access Location", "Warining",
                                   MessageBoxButtons.OK,
                                  MessageBoxIcon.Exclamation,
                                  MessageBoxDefaultButton.Button1);
            }
        }

        private void FileLocation_Enter(object sender, EventArgs e)
        {
            txtFileLocation = FileLocation.Text;
        }

        private void FileName_Enter(object sender, EventArgs e)
        {
            txtFileName = FileName.Text;
        }

        private void FileName_Leave(object sender, EventArgs e)
        {
            
        }
        private void FileCreator_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form fc = Application.OpenForms["MainWindow"];
            if (createButton && fc == null)
            {
                MainWindow mWindow = new MainWindow();
                this.Dispose();
                mWindow.SelectXmlFile();
            }
        }

        private void FileLocation_Leave(object sender, EventArgs e)
        {
            if (!IsValidPath(FileLocation.Text))
            {
                MessageBox.Show("Invalid folder path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                FileLocation.Focus();
                return;
            }
        }
    }
}

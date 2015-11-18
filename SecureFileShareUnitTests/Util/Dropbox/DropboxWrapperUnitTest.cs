using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureFileShare.Util.Dropbox;

namespace SecureFileShareUnitTests.Util.Dropbox
{
    /*
     * The tests were run with my private access token from dropbox's app console.
     * After replacing the token, these tests shouldn't run.
     */

    //[TestClass]
    public class DropboxWrapperUnitTest
    {
        //[TestMethod]
        public void GetAccountTest()
        {
            var dropboxClient = new DropboxWrapper();
            Task task = dropboxClient.GetAccount();
            task.Wait();

            Assert.IsTrue(dropboxClient.Account != null);
        }

        //[TestMethod]
        public void UploadTest()
        {
            const string folder = "";
            const string file = "test.sfs";

            byte[] content;
            using (FileStream fileStream = File.OpenRead("dropboxTest.sfs"))
            {
                content = new byte[fileStream.Length];

                fileStream.Read(content, 0, (int) fileStream.Length);
                fileStream.Close();
            }

            var dropboxClient = new DropboxWrapper();
            Task task = dropboxClient.Upload(folder, file, content, delegate {  });
            task.Wait();
        }

        //[TestMethod]
        public void LoadFileListTest()
        {
            var dropboxClient = new DropboxWrapper();
            Task task = dropboxClient.LoadFileList();
            task.Wait();

            Assert.IsTrue(dropboxClient.Files.Count == 1);
        }

        //[TestMethod]
        public void DownloadTest()
        {
            const string folder = "";
            const string file = "test.sfs";

            var dropboxClient = new DropboxWrapper();
            Task task = dropboxClient.Download(folder, file);
            task.Wait();

            Assert.IsTrue(dropboxClient.LastDownloadedFile != null);
            Assert.IsTrue(dropboxClient.LastDownloadedFile.Length == 709);

            File.WriteAllBytes("dropboxTest2.sfs", dropboxClient.LastDownloadedFile);

            Assert.IsTrue(FileEquals("dropboxTest.sfs", "dropboxTest2.sfs"));
        }

        private static bool FileEquals(string path1, string path2)
        {
            byte[] file1 = File.ReadAllBytes(path1);
            byte[] file2 = File.ReadAllBytes(path2);

            if (file1.Length == file2.Length)
            {
                return !file1.Where((t, i) => t != file2[i]).Any();
            }

            return false;
        }
    }
}
using System.IO;
using System.Drawing.Imaging;
using System;
using System.Text.RegularExpressions;
using System.Text;
using System.Drawing;

namespace PhotoRename
{
    class Program
    {
        static void Main(string[] args)
        {
            var curentDirectory = Directory.GetCurrentDirectory();
            var files = Directory.GetFiles(curentDirectory, "File*.jpeg");
            var length = files.Length;
            Console.WriteLine(string.Format("Found {0} file(s)", length));

            var i = 1;
            foreach (var file in files)
            {
                Console.WriteLine(string.Format("Processing file {0} of {1}", i, length));
                var dateTime = GetDateTakenFromImage(file);
                var newFile = GetNewFilename(dateTime);
                var output = String.Format("Old: {0}\tNew: {1}", file, newFile);
                //Console.Write(output);
                if (!File.Exists(newFile))
                {
                    File.Move(file, newFile);
                }
                i++;
            }
            
        }

        private static string GetNewFilename(DateTime dt)
        {
            var output = string.Format("{0}{1}{2}_{3}{4}{5}", dt.Year, dt.Month,dt.Day, dt.Hour, dt.Minute, dt.Second.ToString("D2"));
            output = output + ".jpg";
            return output;
        }

        private static Regex r = new Regex(":");

        public static DateTime GetDateTakenFromImage(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (Image myImage = Image.FromStream(fs, false, false))
            {
                PropertyItem propItem = myImage.GetPropertyItem(36867);
                string dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                return DateTime.Parse(dateTaken);
            }
        }
    }
}

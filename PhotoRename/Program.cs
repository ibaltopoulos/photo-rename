using System.IO;
using System.Drawing.Imaging;
using System;
using System.Text.RegularExpressions;
using System.Text;
using System.Drawing;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Linq;

namespace PhotoRename
{
    class Program
    {
        static void Main(string[] args)
        {
            var curentDirectory = System.IO.Directory.GetCurrentDirectory();

            var pattern = args.Length > 0 ? args[0] : "File*.jpeg";

            var files = System.IO.Directory.GetFiles(curentDirectory, pattern);
            var length = files.Length;
            Console.WriteLine(string.Format("Found {0} file(s)", length));

            var i = 1;
            foreach (var file in files)
            {
                Console.WriteLine(string.Format("Processing file {0} of {1}. [{2}]", i, length, Path.GetFileName(file)));
                var dateTime = GetDateTakenFromImage(file);
                Console.WriteLine(string.Format("Detected datetime [{0}]", GetDateTakenFromExif(file)));

                var newFile = GetNewFilename(dateTime);
                var output = string.Format("Old: {0}\tNew: {1}", file, newFile);
                //Console.Write(output);
                if (!File.Exists(newFile))
                {
                    File.Move(file, newFile);
                }
                i++;
            }
            
        }
        
        private static DateTime GetDateTakenFromExif(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                // Read all metadata from the image
                var directories = ImageMetadataReader.ReadMetadata(fs);

                // Find the so-called Exif "SubIFD" (which may be null)
                var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

                // Read the DateTime tag value
                var dateTime = subIfdDirectory?.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);
                
                return dateTime.HasValue ? dateTime.Value : GetDateTakenFromFileName(path);
            }
        }

        private static string ToZeroPaddedString(int num)
        {
            // Info about using D2. http://stackoverflow.com/questions/13043521/whats-the-difference-between-tostringd2-tostring00
            var paddedInteger = num.ToString("D2");
            return paddedInteger;
        }

        private static string GetNewFilename(DateTime dt)
        {
            var date = string.Format("{0}{1}{2}", ToZeroPaddedString(dt.Year), ToZeroPaddedString(dt.Month), ToZeroPaddedString(dt.Day));
            var time = string.Format("{0}{1}{2}", ToZeroPaddedString(dt.Hour), ToZeroPaddedString(dt.Minute), ToZeroPaddedString(dt.Second));

            var output = string.Format("{0}_{1}.jpg", date, time);
            return output;
        }

        private static Regex r = new Regex(":");

        public static DateTime GetDateTakenFromImage(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (Image myImage = Image.FromStream(fs, false, false))
            {
                try {
                    PropertyItem propItem = myImage.GetPropertyItem(36867);
                    string dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                    return DateTime.Parse(dateTaken);
                } catch (ArgumentException e)
                {
                    return GetDateTakenFromFileName(path);
                }
            }
        }
        // File 25-10-2015, 22 37 37.jpeg
        private static Regex re = new Regex(@"(\d{2})-(\d{2})-(\d{4}),\s(\d{2})\s(\d{2})\s(\d{2})");
        

        public static DateTime GetDateTakenFromFileName(string path)
        {
            var filename = Path.GetFileName(path);
            var m = re.Match(filename);
            
            var day = GetNextItem(m, 1);
            var month = GetNextItem(m, 2);
            var year = GetNextItem(m, 3);
            var hour = GetNextItem(m, 4);
            var minute = GetNextItem(m, 5);
            var second = GetNextItem(m, 6);

            return new DateTime(year, month, day, hour, minute, second);
            
        }

        private static int GetNextItem(Match m, int index)
        {            
            return int.Parse(m.Groups[index].Value);
            
        }
    }
}

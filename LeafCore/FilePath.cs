using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafCore
{
    public class FilePath
    {
        private string filePath = null;
        
        public FilePath(string filepath)
        {
            this.filePath = filepath;
        }
        public List<String> getFileInList()
        {
            
            string sourceDirectory = this.filePath;
            IEnumerable<String> Files = Directory.EnumerateFiles(sourceDirectory, "*.jpg", SearchOption.AllDirectories);
            List<String> eachLeafImageFolder = Directory.GetDirectories(sourceDirectory).ToList();
            return eachLeafImageFolder;
        }

    }
}

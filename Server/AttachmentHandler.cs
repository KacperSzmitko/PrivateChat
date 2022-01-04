using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;

namespace Server
{
    class AttachmentHandler
    {
        private static String attachmentFolder = "";
        public static void CheckAttachmentFolder()
        {
            String folder = ConfigurationManager.AppSettings.Get("server-attachment-storage");

            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            attachmentFolder = folder;
        }
        public static Boolean checkIfFileExist(String name)
        {
            if(File.Exists(attachmentFolder+Path.DirectorySeparatorChar+name))
                return true;
            else
                return false;
        }

        public static Boolean saveFile(String name, Byte[] fileBytes)
        {
            File.WriteAllBytes(attachmentFolder + Path.DirectorySeparatorChar + name, fileBytes);
            return true;
        }

        public static (Boolean readOk, Byte[] fileBytes) readFile(String name)
        {
            if(!File.Exists(attachmentFolder + Path.DirectorySeparatorChar + name))
            {
                return (false, null);
            }
            Byte[] fileBytes = File.ReadAllBytes(attachmentFolder + Path.DirectorySeparatorChar + name);
            return (true, fileBytes);
        }
    }
}

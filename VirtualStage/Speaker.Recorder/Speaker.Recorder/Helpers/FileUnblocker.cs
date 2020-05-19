using System.Runtime.InteropServices;

namespace Speaker.Recorder.Helpers
{
    public class FileUnblocker
    {
        const long INVALID_FILE_ATTRIBUTES = -1;

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteFile(string name);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetFileAttributes(string name);

        public static bool Unblock(string fileName)
        {
            if (IsBlocked(fileName))
            {
                return DeleteFile(fileName + ":Zone.Identifier");
            }

            return true;
        }

        public static bool IsBlocked(string fileName)
        {
            int attributes = GetFileAttributes(fileName + ":Zone.Identifier");
            return attributes != INVALID_FILE_ATTRIBUTES;
        }
    }
}

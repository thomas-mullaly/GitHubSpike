using System;

namespace GitHubSpike
{
    // http://bytes.com/topic/c-sharp/answers/569205-how-show-star-when-entering-characters-console-application
    public class PasswordEntry
    {
        [System.Runtime.InteropServices.DllImport("kernel32 ")]
        private static extern int SetConsoleMode(IntPtr hConsoleHandle, int dwMode);

        [System.Runtime.InteropServices.DllImport("kernel32 ")]
        private static extern int GetConsoleMode(IntPtr hConsoleHandle, ref int dwMode);

        private const int ENABLE_ECHO_INPUT = 4;

        private const int CONIN = 3;

        public string ReadPassword()
        {
            IntPtr hStdIn = new IntPtr(CONIN);
            int mode = 0;

            GetConsoleMode(hStdIn, ref mode);
            mode = (mode & ~ENABLE_ECHO_INPUT);
            SetConsoleMode(hStdIn, mode);

            string password = Console.ReadLine();

            SetConsoleMode(hStdIn, mode | ENABLE_ECHO_INPUT);
            return password;
        }
    }
}
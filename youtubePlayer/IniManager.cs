using System;
using System.Runtime.InteropServices;	// Add
using System.Text;

/*
 * using System.Runtime.InteropServices를 선언하지 않을 경우
 * 밑에 보이는 1, 2번 문장 중에 1번으로 사용해야 하며
 * 위와 같이 using문으로 선언이 된 경우 2번 문장을 사용해야 한다.
 * using문을 선언하고 1번 문장을 사용해도 상관은 없다.
 * 단 using문을 선언하지 않고 2번 문장과 같이 사용해서는 안된다.
 */

// 1 [System.Runtime.InteropServices.DllImport("kernel32")]
// 2 [DllImport("kernel32")]

namespace IniManager
{
	/// <summary>
	/// Create a New INI file to store or load data
	/// </summary>
	public class iniClass : IDisposable
	{
        public void Dispose()
        {

        }
		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section,
			string key, string def, StringBuilder retVal, int size, string filePath);

		[DllImport("kernel32")]
		private static extern long WritePrivateProfileString(string section,
			string key, string val, string filePath);

		// INI 값 읽기
		public String GetIniValue(String Section, String Key, String iniPath)
		{
			StringBuilder temp = new StringBuilder(255);
			int i = GetPrivateProfileString(Section, Key, "", temp, 255, iniPath);
			return temp.ToString();
		}

		// INI 값 설정
		public void SetIniValue(String Section, String Key, String Value, String iniPath)
		{
			WritePrivateProfileString(Section, Key, Value, iniPath);
		}
	}
}
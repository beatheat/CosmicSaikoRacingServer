
namespace CSRServer
{
	internal class Logger
	{
		private static StreamWriter? _stream = null;
		private static Thread? _flushThread = null;

		public static void Load(string path)
		{
			try
			{
				Console.WriteLine("[Logger] Opening log stream...");
				_stream = new StreamWriter(path, append: true);
				_flushThread = new Thread(() =>
				{
					while (_stream != null)
					{
						Thread.Sleep(3 * 60 * 1000);
						_stream?.Flush();
					}
				});
				_flushThread.Start();
				ClearLine();
				Console.WriteLine("[Logger] Log stream open. Log thread is running");
			}
			catch (Exception e)
			{
				throw new Exception("Logger::Load - Cannot create log stream \n" + e.Message);
			}
		}

		public static void Close()
		{
			_flushThread?.Interrupt();
			if (_stream != null)
			{
				_stream.Close();
				_stream = null;
			}
		}


		public static void ClearLine()
		{
			Console.SetCursorPosition(0, Console.CursorTop - 1);
			Console.Write(new string(' ', Console.BufferWidth));
			Console.SetCursorPosition(0, Console.CursorTop - 1);
		}
		
		/// <summary>
		/// 게임서버 로그를 출력함
		/// </summary>
		public static void Log(string log)
		{
			Console.WriteLine("[GameServer]"+log);
			_stream?.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff") + "]" + log);
		}
		
		public static void LogWithClear(string log)
		{
			ClearLine();
			Console.WriteLine("[GameServer]"+log);
			_stream?.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff") + "]" + log);
		}
	}
}

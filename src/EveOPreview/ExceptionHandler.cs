using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace EveOPreview;

internal sealed class ExceptionHandler
{
	private const string EXCEPTION_DUMP_FILE_NAME = "EVE-O Preview.log";

	private const string EXCEPTION_MESSAGE = "EVE-O Preview has encountered a problem and needs to close. Additional information has been saved in the crash log file.";

	public void SetupExceptionHandlers()
	{
		if (!Debugger.IsAttached)
		{
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			Application.ThreadException += (object sender, ThreadExceptionEventArgs e) =>
			{
				ExceptionEventHandler(e.Exception);
			};
			AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
			{
				ExceptionEventHandler(e.ExceptionObject as Exception);
			};
		}
	}

	private void ExceptionEventHandler(Exception exception)
	{
		try
		{
			string contents = exception.ToString();
			File.WriteAllText("EVE-O Preview.log", contents);
			MessageBox.Show("EVE-O Preview has encountered a problem and needs to close. Additional information has been saved in the crash log file.", "EVE-O Preview", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		catch
		{
		}
		Environment.Exit(1);
	}
}

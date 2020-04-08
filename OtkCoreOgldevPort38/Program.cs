using OpenToolkit.Windowing.Desktop;
using System;

namespace OtkCoreOgldevPort38
{
	class Program
	{
		static void Main(string[] args)
		{
			var windowSettings = new NativeWindowSettings();
			windowSettings.Size = new OpenToolkit.Mathematics.Vector2i(1264, 1008);

			new MainWindow(GameWindowSettings.Default,
				windowSettings)
				.Run();
		}
	}
}

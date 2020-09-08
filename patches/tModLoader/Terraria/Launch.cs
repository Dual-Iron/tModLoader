using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Terraria;

internal static class Launch
{
	private static void Main(string[] args) {
		AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs sargs) {
			string resourceName = new AssemblyName(sargs.Name).Name + ".dll";
			string text = Array.Find(typeof(Program).Assembly.GetManifestResourceNames(), (string element) => element.EndsWith(resourceName));
			if (text == null)
				return null;

			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(text)) {
				byte[] array = new byte[stream.Length];
				stream.Read(array, 0, array.Length);
				return Assembly.Load(array);
			}
		};

		NativeLibrary.SetDllImportResolver(typeof(Game).Assembly, (name, assembly, searchPath) => {
			try {
				var files = Directory.GetFiles("deps", name + ".*");
				var match = files.FirstOrDefault(s => File.Exists(s));
				if (match != null && NativeLibrary.TryLoad(match, out var handle)) {
					return handle;
				}
				return IntPtr.Zero;
			} catch (DirectoryNotFoundException e) {
				throw new DirectoryNotFoundException("A needed library file was missing from the tModLoader/deps directory. " + e.Message, e);
			}
		});

		Environment.SetEnvironmentVariable("FNA_WORKAROUND_WINDOW_RESIZABLE", "1");
		Program.LaunchGame(args);
	}
}
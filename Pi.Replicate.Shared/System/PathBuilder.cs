using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared.System
{
	public static class PathBuilder
	{
		private static string _rootFolder;

		public static void Initialize(string rootFolder)
		{
			_rootFolder = rootFolder;
		}

		//todo test
		public static string Build(params string[] parts)
		{
			var newParts = new string[parts.Length + 1];
			newParts[0] = _rootFolder;
			for (var i = 1; i < newParts.Length; i++)
				newParts[i] = parts[i - 1];
			return Path.Combine(newParts);
		}
	}
}

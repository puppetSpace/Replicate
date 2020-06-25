using System.Collections.Generic;

namespace Pi.Replicate.Worker.Host.Processing
{
	internal sealed class FolderCrawler
	{

		public IList<System.IO.FileInfo> GetFiles(string path)
		{
			var files = new List<System.IO.FileInfo>();
			if (string.IsNullOrWhiteSpace(path) || !System.IO.Directory.Exists(path))
			{
				WorkerLog.Instance.Warning($"Given path, '{path}' is not a directory. Returning empty list");
				return files;
			}

			WorkerLog.Instance.Debug($"Traversing '{path}'");

			foreach (var file in System.IO.Directory.GetFiles(path))
				files.Add(new System.IO.FileInfo(file));

			foreach (var dir in System.IO.Directory.GetDirectories(path))
				files.AddRange(GetFiles(dir));

			return files;
		}
	}
}

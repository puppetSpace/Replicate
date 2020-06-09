using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;

namespace Pi.Replicate.Worker.Host.Services
{
    public class FileConflictService
    {
		private readonly ConflictRepository _conflictRepository;

		public FileConflictService(ConflictRepository conflictRepository)
		{
			_conflictRepository = conflictRepository;
		}

        public async Task<bool> Check(File currentFile, ICollection<File> otherVersions)
		{
			var hasConflicts = false;
			var maxVersion = otherVersions.Max(x => x.Version);
			var sumOfVersions = otherVersions.Sum(x => x.Version);
			if(otherVersions.Count(x=>x.Version == currentFile.Version) > 1)
			{
				WorkerLog.Instance.Information($"Conflict found while assembling file '{currentFile.Path}': There is already version {currentFile.Version} for this file.");
				var conflict = FileConflict.Create(currentFile.Id, $"There is already version {currentFile.Version} for this file.");
				await _conflictRepository.AddConflict(conflict);
				hasConflicts = true;
			}
			else if(currentFile.Version < maxVersion)
			{
				WorkerLog.Instance.Information($"Conflict found while assembling file '{currentFile.Path}': There is a higher version than {currentFile.Version} for this file.");
				var conflict = FileConflict.Create(currentFile.Id, $"There is a higher version than {currentFile.Version} for this file.");
				await _conflictRepository.AddConflict(conflict);
				hasConflicts = true;
			}
			else if(maxVersion * (maxVersion+1)/2 != sumOfVersions)
			{
				WorkerLog.Instance.Information($"Conflict found while assembling file '{currentFile.Path}': The file seems to be missing a version.");
				var conflict = FileConflict.Create(currentFile.Id, "The file seems to be missing a version.");
				await _conflictRepository.AddConflict(conflict);
				hasConflicts = true;
			}

			return hasConflicts;
		}
    }
}

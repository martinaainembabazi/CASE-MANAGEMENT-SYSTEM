using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Core.Repository.Common
{
	public class RepositoryResult
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public List<string> Errors { get; set; } = new List<string>();

		public static RepositoryResult Successful(string message = "Operation completed successfully")
		{
			return new RepositoryResult { Success = true, Message = message };
		}

		public static RepositoryResult Failed(string message, List<string> errors = null)
		{
			return new RepositoryResult
			{
				Success = false,
				Message = message,
				Errors = errors ?? new List<string>()
			};
		}
	}
}

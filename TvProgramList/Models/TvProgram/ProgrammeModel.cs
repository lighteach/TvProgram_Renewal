using System;
using System.Collections.Generic;
using System.Linq;

namespace TvProgramList.Models.TvProgram
{
	public class ProgrammeModel
	{
		public string Start { get; set; }
		public string Stop { get; set; }
		public string Title { get; set; }
		public string Language { get; set; }
		public string Category { get; set; }
		public string Episode { get; set; }
		public string PreviouslyShown { get; set; }
		public string Description { get; set; }
		public string RatingSystem { get; set; }
		public string Rating { get; set; }
		public bool BeOnAir { get; set; }
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TvProgramList.Models.YouTube
{
	public class YouTubeApiInfoModel
	{
		public string ApiKey { get; set; }
		public ApiURIModel ApiURI { get; set; }
	}

	public class ApiURIModel
	{
		public string Top3VideoList { get; set; }
	}
}
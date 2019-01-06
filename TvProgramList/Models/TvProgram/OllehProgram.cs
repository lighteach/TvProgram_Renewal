using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TvProgramList.Models.TvProgram
{
	public class OllehProgram
	{
		public string time { get; set; }
		public string title { get; set; }
		public string ageLimit { get; set; }
		public string classify { get; set; }
		public bool BeOnAir { get; set; }
	}

	public class OllehChannel
	{
		/// <summary>
		/// 채널 번호
		/// </summary>
		public string chNum { get; set; }
		/// <summary>
		/// 채널 명
		/// </summary>
		public string chName { get; set; }
	}
}
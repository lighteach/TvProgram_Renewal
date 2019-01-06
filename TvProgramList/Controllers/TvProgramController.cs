#region 작업일지
// [2017-03-13]
// - Test MVC 메쏘드 만들고 서버에 올렸음 
// [2017-03-31]
// - Olleh 편성표 뽀려다가 뿌려주는 작업 중
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Xml;
using System.Web.Mvc;
using System.Configuration;
using TvProgramList.Models.TvProgram;
using TvProgramList.Libs;

namespace TvProgramList.Controllers
{
	public class TvProgramController : Controller
	{
		// GET: TvProgram
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Test()
		{
			return View();
		}

		#region 기존 XML 파싱 활용 편성표 메쏘드

		#region GetXmlPath
		protected string GetXmlPath
		{
			get
			{
				string xmlFilename = "tv_program_" + DateTime.Now.ToString("yyyyMMdd") + ".xml";
				string path = ConfigurationManager.AppSettings["TV_CHANNEL_XML_PATH"];
				path = Path.Combine(path, xmlFilename);
				return path;
			}
		} 
		#endregion

		#region GetChannelList
		public JsonResult GetChannelList()
		{
			List<ChannelModel> lstRtn = new List<ChannelModel>();

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(GetXmlPath);

			XmlNodeList channels = xmlDoc.LastChild.SelectNodes("//channel");
			if (channels != null)
			{
				foreach (XmlNode node in channels)
				{
					ChannelModel cm = new ChannelModel();
					cm.Id = node.Attributes["id"].Value;
					//XmlNode displayName = node.SelectSingleNode("//display-name");
					// XPATH로 찾았더니 맨 상단 채널 display-name이 자꾸 나와서 어쩔 수 없이 F얼스트촤일드로 찾음
					XmlNode displayName = node.FirstChild;
					//XmlNode icon = node.SelectSingleNode("//icon");
					XmlNode icon = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name.Equals("icon"));
					cm.DisplayName = (displayName != null ? displayName.InnerText : "");
					cm.Icon = (icon != null ? icon.Attributes["src"].Value : "");
					lstRtn.Add(cm);
				}
			}
			return Json(lstRtn, JsonRequestBehavior.AllowGet);
		} 
		#endregion

		#region GetProgrammeList
		public JsonResult GetProgrammeList(string channelId)
		{
			channelId = channelId.Trim();
			//ProgrammeModel
			List<ProgrammeModel> lstRtn = new List<ProgrammeModel>();

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(GetXmlPath);

			XmlNodeList programmes = xmlDoc.LastChild.SelectNodes("//programme[@channel='" + channelId + "']");
			if (programmes != null)
			{
				foreach (XmlNode node in programmes)
				{
					ProgrammeModel pm = new ProgrammeModel();
					DateTime dtNow = DateTime.Now;
					//DateTime dtNow = Convert.ToDateTime("2016-10-28 18:21:00");
					DateTime dtStart = ConvertProgrammeDateToDateTime(node.Attributes["start"].Value);
					DateTime dtStop = ConvertProgrammeDateToDateTime(node.Attributes["stop"].Value);
					pm.BeOnAir = false;
					if ((dtStart == dtNow) || (dtStart < dtNow && dtStop > dtNow))
						pm.BeOnAir = true;
					string strStart = dtStart.ToString("HH:mm");
					string strStop = dtStop.ToString("HH:mm");
					pm.Start = strStart;
					pm.Stop = strStop;
					node.ChildNodes.Cast<XmlNode>().ToList().ForEach(n1 =>
					{
						if (n1.Name.Equals("title")) pm.Title = n1.InnerText;
						else if (n1.Name.Equals("language")) pm.Language = n1.InnerText;
						else if (n1.Name.Equals("episode-num")) pm.Episode = n1.InnerText;
						else if (n1.Name.Equals("previously-shown")) pm.PreviouslyShown = n1.InnerText;
						else if (n1.Name.Equals("rating"))
						{
							pm.RatingSystem = n1.Attributes["system"].Value;
							pm.Rating = n1.SelectSingleNode("//value").InnerText;
						}
					});

					lstRtn.Add(pm);
				}
			}

			return Json(lstRtn, JsonRequestBehavior.AllowGet);
		} 
		#endregion

		#endregion

		public JsonResult GetOllehMainClassifies()
		{
			// [2017-10-06]
			// 문제발생!! : 기존 긁어오던 URI가 변경되어 URI 및 파싱 로직을 다시 짜야한다!!
			// http://tv.olleh.com/tvinfo/liveCH/skylife.asp 이 ==> http://tv.kt.com/ 으로 바뀌었음!!
			OllehTvProgram otp = new OllehTvProgram();
			string[] arr = otp.GetMainClassifies();

			return Json(arr, JsonRequestBehavior.AllowGet);
		}

		public JsonResult GetOllehChannelList(int idx)
		{
			OllehTvProgram otp = new OllehTvProgram();
			object[] arrObj = otp.GetChannels(idx);

			return Json(arrObj, JsonRequestBehavior.AllowGet);
		}

		public JsonResult GetOllehProgramList(string chNum, string chName)
		{
			OllehTvProgram otp = new OllehTvProgram();
			List<OllehProgram> lstObj = otp.GetProgramList(chNum, chName);

			DateTime dtNow = DateTime.Now;
			int intCnt = 0;
			foreach (OllehProgram obj in lstObj)
			{
				int nextCnt = intCnt + 1;
				if (nextCnt < lstObj.Count)
				{
					OllehProgram opNext = lstObj[nextCnt];

					string strStartDt = dtNow.ToString("yyyy-MM-dd") + " " + obj.time + ":00";
					string strEndDt = dtNow.ToString("yyyy-MM-dd") + " " + opNext.time + ":00";
					DateTime dtStart = Convert.ToDateTime(strStartDt);
					DateTime dtStop = Convert.ToDateTime(strEndDt);

					obj.BeOnAir = false;
					if ((dtStart == dtNow) || (dtStart < dtNow && dtStop > dtNow))
						obj.BeOnAir = true;
				}
				intCnt++;
			}

			return Json(lstObj, JsonRequestBehavior.AllowGet);
		}

		public JsonResult SearchProgram(string searchText)
		{
			#region 리턴 될 데이터들
			string result = "Empty";
			List<OllehChannel> foundList = new List<OllehChannel>(); 
			#endregion

			OllehTvProgramByHtml ollHtml = new OllehTvProgramByHtml();

			OllehTvProgram ollTv = new OllehTvProgram();
			OllehChannel[] channels = ollTv.GetChannels(0); // 파라미터를 0으로 하여 전체 채널 리스트를 가져온다.
			// 전체 채널들을 차례대로 돌아가며,
			foreach (OllehChannel channel in channels)
			{
				// 채널명에도 찾고자 하는 말이 포함되어 있으면 찾은 리스트에 추가한다.
				if (channel.chName.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) != -1)
					foundList.Add(channel);

				List<OllehProgram> prgList = ollHtml.GetProgramList(channel.chNum, channel.chName);
				if (prgList.Count > 0)
				{
					// 찾을 말이 포함되어 있는 프로그램 리스트가 있으면 찾은 리스트에 추가해준다.
					foreach (OllehProgram prg in prgList)
					{
						if (prg.title.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) != -1)
						{
							foundList.Add(channel);
						}
					}
				}
			}

			if (foundList.Count > 0)
				result = "OK";

			var rtn = new
			{
				Result = result
				, FoundList = foundList
			};
			return Json(rtn, JsonRequestBehavior.AllowGet);
		}


		private DateTime ConvertProgrammeDateToDateTime(string prgDt)
		{
			DateTime dtRtn = new DateTime();
			//20161019000000 +0900
			string ymdhms = prgDt.Split(' ')[0];
			int[] arrSplitLen = { 4, 2, 2, 2, 2, 2 };
			string[] savedInfo = new string[6];

			int shift = 0;
			for (int i = 0; i < arrSplitLen.Length; i++)
			{
				int start = shift;
				int end = arrSplitLen[i];
				savedInfo[i] = ymdhms.Substring(start, end);
				shift = shift + end;
			}

			string ymd = string.Join("-", savedInfo.Take(3));
			string hms = string.Join(":", savedInfo.Skip(3));
			string strRtn = string.Format("{0} {1}", ymd, hms);
			dtRtn = Convert.ToDateTime(strRtn);
			return dtRtn;
		}
	}
}
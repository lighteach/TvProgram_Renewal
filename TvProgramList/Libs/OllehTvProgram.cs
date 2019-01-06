using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Web;
using HtmlAgilityPack;
using TvProgramList.Models.TvProgram;

namespace TvProgramList.Libs
{
	public class OllehTvProgram
	{
		private const string m_Url = "https://tv.kt.com/";
		private const string m_ChannelListUrl = "/tv/channel/pChList.asp";
		private const string m_ProgramListUrl = "/tv/channel/pSchedule.asp";

		private List<int[]> m_ChnCodes = new List<int[]>();

		#region OllehTvProgram : 생성자, 여기서 각 카테고리 별 바인딩 되어야 할 채널 리스트를 바인드 한다.
		public OllehTvProgram()
		{
			// 전체
			m_ChnCodes.Add(new int[] { -999 });
			// 지상파/종편/홈쇼핑
			m_ChnCodes.Add(new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 21, 22, 26, 28, 30, 31, 33, 34, 36 });
			// 드라마/오락/음악
			m_ChnCodes.Add(new int[] { 1, 20, 27, 29, 32, 35, 37, 38, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 55, 57, 80, 85, 86, 87, 89, 99, 112, 113, 114, 116, 117, 118, 119, 125, 136, 139, 143, 144, });
			// 영화/시리즈
			m_ChnCodes.Add(new int[] { 54, 63, 64, 65, 66, 68, 69, 70, 71, 72, 73, 74, 75, 77, 78, 79, 88, 160 });
			// 스포츠/레져
			m_ChnCodes.Add(new int[] { 56, 81, 83, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 120, 121, 122, 123, 134, 140, 141, 142, 159, 174, 176 });
			// 애니/유아/교육
			m_ChnCodes.Add(new int[] { 145, 146, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 168, 169, 170 });
			// 다큐/교양/종교
			m_ChnCodes.Add(new int[] { 126, 127, 128, 129, 130, 131, 132, 135, 180, 181, 182, 183, 184, 185, 187 });
			// 뉴스/경제
			m_ChnCodes.Add(new int[] { 23, 24, 25, 91, 92, 93, 94, 95, 96, 97, 171, 172, 173, 177 });
			// 공공/공익/정보
			m_ChnCodes.Add(new int[] { 90, 98, 115, 137, 138, 161, 162, 163, 164, 165, 167, 175, 178, 179, 186, 188, 189 });
			// 오픈
			m_ChnCodes.Add(new int[] { 0 });
			// 유료
			m_ChnCodes.Add(new int[] { 61, 62, 147, 300, 301, 302 });
			// 오디오
			m_ChnCodes.Add(new int[] { 199, 200, 310, 311, 312, 313, 314, 315, 316, 317, 318, 319, 320, 321, 322, 323, 324, 325, 331, 332, 333, 334, 335, 336, 337, 338, 339, 340, 341, 342, 343, 344, 345 });
		} 
		#endregion

		#region GetMainClassifies
		public string[] GetMainClassifies()
		{
			HtmlDocument doc = GetDocument(m_Url);

			// channelGroup 가져오기
			HtmlNode tabBtns = GetDivByClassName(doc.DocumentNode, "tab_btns")[0];
			HtmlNode[] channelGroup = tabBtns.ChildNodes.Where(n => n.Name.Equals("a")).ToArray<HtmlNode>();

			string[] arrRtn = tabBtns.ChildNodes.Where(n => n.Name.Equals("a")).Select(n => n.InnerText).ToArray<string>();

			return arrRtn;
		}
		#endregion

		#region GetChannels
		public OllehChannel[] GetChannels(int idx)
		{
			#region 예전 코드
			//string searchClsNm = "channelList" + idx.ToString();
			//List<object> lstTmp = new List<object>();

			//HtmlDocument doc = GetDocument(m_Url);

			//// 채널 가져오기
			//// channelGroup 가져오기
			//HtmlNode[] channelGroup = GetDivByClassName(doc.DocumentNode, "channelGroup");

			//HtmlNode found = channelGroup.FirstOrDefault(
			//	grp => grp.ChildNodes.Any(cn => cn.Attributes.Contains("class") && cn.Attributes["class"].Value.Contains(searchClsNm)));

			//if (found != null)
			//{
			//	HtmlNode channelList = found.ChildNodes.SingleOrDefault(n => n.Attributes.Contains("class") && n.Attributes["class"].Value.Contains(searchClsNm));
			//	HtmlNode ul = channelList.SelectSingleNode("ul");
			//	HtmlNodeCollection collLi = ul.SelectNodes("li");
			//	foreach (HtmlNode hn in collLi)
			//	{
			//		string channelNum = hn.SelectSingleNode("span[@class=\"sel\"]").InnerText.Trim();
			//		string channelName = hn.SelectSingleNode("a").InnerText.Trim();

			//		lstTmp.Add(new { chNum = channelNum, chName = channelName });
			//	}
			//}

			//return lstTmp.ToArray(); 
			#endregion

			List<OllehChannel> lstTmp = new List<OllehChannel>();

			string reqUri = (new Uri((new Uri(m_Url)), m_ChannelListUrl)).ToString();
			string sendIdx = idx.ToString();
			// 맨 마지막 탭(오디오)인 11번이 들어올 경우 272번으로 바꿔준다.
			// 이 미친놈들이 그렇게 만들어놔서 그렇게 해야함
			if (idx.Equals(11)) sendIdx = "272";
			string postParams = "ch_type=2&parent_menu_id={0}&product_cd=&option_cd_list=";
			postParams = string.Format(postParams, sendIdx);

			HtmlDocument doc = GetDocument(reqUri, postParams);
			HtmlNode[] arrUlChannel = GetUlByClassName(doc.DocumentNode, "channel");
			if (arrUlChannel.Length > 0)
			{
				foreach (HtmlNode ulChnl in arrUlChannel)
				{
					HtmlNodeCollection arrLiChnl = ulChnl.SelectNodes("li");
					if (arrLiChnl != null)
					{
						foreach (HtmlNode liChnl in arrLiChnl)
						{
							HtmlNode anchor = liChnl.SelectSingleNode("a");
							string channelNum = anchor.Attributes["id"].Value.Replace("linkChannel", "");
							string channelName = anchor.InnerText.Replace("\n", "").Replace("\r", "").Replace("\t", "");
							channelName = HttpContext.Current.Server.HtmlDecode(channelName);

							lstTmp.Add(new OllehChannel { chNum = channelNum, chName = channelName });

							#region 브라우저 설정이 되지 않은 경우
							// ※ 현재는 브라우저 설정이 되어 있어 이 코드를 쓰지 않음
							//		KT 스카이라이프 편성표를 긁어오려면 UserAgent 등의 브라우저 속성 설정들도 제대로 해줘야만
							//		스크래핑이 가능하다. 그런 설정들이 되어있지 않은 상태에서 가져오면 전체 채널 리스트만 뱉기 때문에
							//		별도의 채널 바인딩 처리를 해준 코드이다.
							//int[] arrChnList = m_ChnCodes[idx];
							//if (arrChnList[0].Equals(-999))	// 전체 탭이면 무조건 쎄려박는다
							//{
							//	lstTmp.Add(new { chNum = channelNum, chName = channelName });
							//}
							//else    // 전체 탭이 아니면 걸러내서 잡아 쳐넣는다
							//{
							//	if (arrChnList.Contains(int.Parse(channelNum)))
							//	{
							//		lstTmp.Add(new { chNum = channelNum, chName = channelName });
							//	}
							//} 
							#endregion
						}
					}
				}
			}
			return lstTmp.ToArray<OllehChannel>();
		}
		#endregion

		#region GetProgramList
		public List<OllehProgram> GetProgramList(string chNum, string chName)
		{
			#region 예전 코드
			//List<OllehProgram> lstTmp = new List<OllehProgram>();

			//string nowDate = DateTime.Now.ToString("yyyyMMdd");
			//chName = HttpContext.Current.Server.UrlEncode(chName);

			//// http://tv.olleh.com/renewal_sub/liveTv/pop_schedule_week.asp?chtype=2&ch_no=4&ch_name=CJ%uC624%uC1FC%uD551&nowdate=20170401
			//string url = string.Format(m_ProgramListUrl, chNum, chName, nowDate);

			//HtmlDocument doc = GetDocument(url);

			//HtmlNode[] arrNode = GetTableById(doc.DocumentNode, "pop_day");
			//if (arrNode.Length > 0)
			//{
			//	HtmlNode table = arrNode[0];
			//	HtmlNode tbody = table.SelectSingleNode("tbody");
			//	if (tbody != null)
			//	{
			//		HtmlNodeCollection trColl = tbody.SelectNodes("tr");
			//		if (trColl != null)
			//		{
			//			foreach (HtmlNode hn in trColl)
			//			{
			//				HtmlNodeCollection tdColl = hn.SelectNodes("td");
			//				string time = tdColl[0].InnerText.Trim();
			//				string title = tdColl[1].InnerText.Trim();
			//				string ageLimit = tdColl[2].SelectSingleNode("span").SelectSingleNode("em").InnerText.Trim();
			//				if (ageLimit.IndexOf("all") != -1) ageLimit = "전체 관람가";
			//				string classify = tdColl[4].InnerText.Trim();
			//				lstTmp.Add(new OllehProgram { time = time, title = title, ageLimit = ageLimit, classify = classify, BeOnAir = false });
			//			}
			//		}
			//	}
			//}

			//return lstTmp; 
			#endregion

			List<OllehProgram> lstTmp = new List<OllehProgram>();

			string reqUri = (new Uri((new Uri(m_Url)), m_ProgramListUrl)).ToString();
			string postParams = "ch_type=2&service_ch_no={0}&view_type=1";
			postParams = string.Format(postParams, chNum);

			HtmlDocument doc = GetDocument(reqUri, postParams);
			HtmlNode table = doc.DocumentNode.Descendants("table").FirstOrDefault(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("tb_schedule"));
			if (table != null)
			{
				HtmlNode tbody = table.Descendants("tbody").FirstOrDefault();
				HtmlNode[] arrTr = tbody.ChildNodes.Where(t => t.Name.Equals("tr")).ToArray<HtmlNode>();
				foreach (HtmlNode tr in arrTr)
				{
					#region 참고사항
					// [2017-10-07]
					// 7번 KBS2 프로그램 리스트 가져올때 
					//< td class="program">
					//				<p>추석특집 리얼맛집 검증쇼<100인의 선택>
					//					<b><img src = "/src/images/kt_tv/icons/icon_20x20_15.png" alt="15세 이상"/></b>
					//					<b><img src = "/src/images/kt_tv/icons/icon_30x18_Hd.png" alt="Hd"/></b>
					//				</p>
					//</td>
					// 이 태그를 HtmlAgility가 가져올때 <100인의 선택> 부분을 태그로 인식하여 임의로 <100인의 선택=""> 이딴식으로
					// 바꿔버려서 pInfo를 가져오지 못해 오류를 발생하였다 이 부분을 해결해야 한다.. 씨발 어떻하지.. ㅠㅠ
					// [2017-10-10] raw 데이터를 가져오는 부분에서 그냥 태그를 조져버려서 해결했다 씨발..
					#endregion

					HtmlNode tdProgram = GetNodeByClassName(tr, "td", "program").First();
					HtmlNode[] arrTime = GetNodeByClassName(tr, "td", "time");

					string timeFormat = "{0}:{1}";
					string time = "";
					string title = "";
					string ageLimit = "";
					HtmlNode hnStrong = null;

					HtmlNode[] arrPInfo = tdProgram.ChildNodes.Where(n => n.Name.Equals("p")).ToArray<HtmlNode>();
					HtmlNode pInfo = null;
					if (arrPInfo.Length > 1)
					{

						for (int intCnt = 0; intCnt < arrPInfo.Length; intCnt++)
						{
							pInfo = arrPInfo[intCnt];
							time = string.Format(timeFormat
								, arrTime[0].InnerText.Replace("\r\n", "").Replace("\t", "")
								, arrTime[1].ChildNodes.Where(n => n.Name.Equals("p")).ToArray<HtmlNode>()[intCnt].InnerText.Replace("\r\n", "").Replace("\t", "")
							);
							title = pInfo.InnerText.Replace("\r\n", "").Replace("\t", "");
							ageLimit = pInfo.ChildNodes.First(n => n.Name.Equals("b")).ChildNodes.First(n => n.Name.Equals("img")).Attributes["alt"].Value;
							hnStrong = GetNodeByClassName(pInfo, "strong", "online").FirstOrDefault();
							bool BeOnAir = (hnStrong != null);

							lstTmp.Add(new OllehProgram
							{
								time = time
								, title = title
								, ageLimit = ageLimit
								, BeOnAir = BeOnAir
								, classify = ""
							});
						}
					}
					else
					{
						pInfo = arrPInfo[0];

						time = string.Format(timeFormat
								, arrTime[0].InnerText.Replace("\r\n", "").Replace("\t", "")
								, arrTime[1].InnerText.Replace("\r\n", "").Replace("\t", ""));
						title = pInfo.InnerText.Replace("\r\n", "").Replace("\t", "");
						ageLimit = pInfo.ChildNodes.First(n => n.Name.Equals("b")).ChildNodes.First(n => n.Name.Equals("img")).Attributes["alt"].Value;
						hnStrong = GetNodeByClassName(pInfo, "strong", "online").FirstOrDefault();
						bool BeOnAir = (hnStrong != null);

						lstTmp.Add(new OllehProgram
						{
							time = time
							, title = title
							, ageLimit = ageLimit
							, BeOnAir = BeOnAir
							, classify = ""
						});
					}

				}
			}

			return lstTmp;
		}
		#endregion

		#region GetNodeByClassName
		private static HtmlNode[] GetNodeByClassName(HtmlNode docNode, string nodeName, string className)
		{
			HtmlNode[] arr = docNode.Descendants(nodeName)
				.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains(className))
				.ToArray<HtmlNode>();

			return arr;
		}
		#endregion

		#region GetUlByClassName
		private static HtmlNode[] GetUlByClassName(HtmlNode docNode, string className)
		{
			HtmlNode[] arr = docNode.Descendants("ul")
						.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains(className))
						.ToArray<HtmlNode>();

			return arr;
		} 
		#endregion

		#region GetDivByClassName
		private static HtmlNode[] GetDivByClassName(HtmlNode docNode, string className)
		{
			HtmlNode[] arr = docNode.Descendants("div")
						.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains(className))
						.ToArray<HtmlNode>();

			return arr;
		}
		#endregion

		#region GetTableById
		private static HtmlNode[] GetTableById(HtmlNode docNode, string id)
		{
			HtmlNode[] arr = docNode.Descendants("table")
						.Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains(id))
						.ToArray<HtmlNode>();

			return arr;
		} 
		#endregion

		#region GetDocument
		private HtmlDocument GetDocument(string url)
		{
			//string content = GetHttpConent(url);
			string postParam = "ch_type={0}&product_cd={1}&parent_menu_id={2}&view_type={3}&service_ch_no={4}";
			postParam = string.Format(postParam, "2", "", "0", "", "");

			string content = GetHttpPostContent(url, postParam);
			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(content);
			return doc;
		}

		private HtmlDocument GetDocument(string baseUrl, string param)
		{
			string content = GetHttpPostContent(baseUrl, param);
			string[] arrCont = content.Split('\n');
			for (int intCnt = 0; intCnt < arrCont.Length; intCnt++)
			{
				string line = arrCont[intCnt];
				if (line.IndexOf("<p>") != -1 && line.IndexOf("</p>") == -1)
				{
					line = line.Replace("<", "[").Replace(">", "]");
					line = line.Replace("[p]", "<p>").Replace("[strong class=\"online\"]", " <strong class=\"online\">")
						.Replace("[/strong]", "</strong>")
						.Replace("[img]", "<img>");
					arrCont[intCnt] = line;
				}
			}

			content = string.Join(Environment.NewLine, arrCont);

			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(content);
			return doc;
		}
		#endregion

		#region GetHttpPostContent
		private string GetHttpPostContent(string url, string queStr)
		{
			string strRtn = "";
			byte[] bySendData = UTF8Encoding.UTF8.GetBytes(queStr);
			HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
			req.Referer = m_Url;
			req.Method = "POST";
			req.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.97 Safari/537.11";
			req.ContentType = "application/x-www-form-urlencoded";
			req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
			req.ContentLength = bySendData.Length;
			using (Stream reqStrm = req.GetRequestStream())
			{
				reqStrm.Write(bySendData, 0, bySendData.Length);
				using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
				{
					using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding("euc-kr")))
					{
						strRtn = sr.ReadToEnd();
					}
				}
			}

			return strRtn;
		} 
		#endregion
	}
}
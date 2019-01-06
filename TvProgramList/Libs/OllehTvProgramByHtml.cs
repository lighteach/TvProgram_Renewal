using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Net;
using HtmlAgilityPack;
using TvProgramList.Models.TvProgram;

namespace TvProgramList.Libs
{
	public class OllehTvProgramByHtml
	{
		#region HtmlFileRoot
		private string HtmlFileRoot
		{
			get
			{
				return @"C:\Web\KT_ScheduleUpdate\HtmlSaveDir";
			}
		}
		#endregion

		#region GetProgramHtml
		private string GetProgramHtml(int chNum)
		{
			string htmlPath = $"{HtmlFileRoot}\\chn_{chNum}.html";
			string html = File.ReadAllText(htmlPath, Encoding.GetEncoding("euc-kr"));
			return html;
		}
		#endregion

		#region IsHtmlFileAvailable : KT_TvScheduleUpdate가 작동하여 html 파일이 생성되었는지 확인한다.
		public bool IsHtmlFileAvailable(int chNum)
		{
			return File.Exists($"{HtmlFileRoot}\\chn_{chNum}.html");
		}
		#endregion

		#region GetProgramList : OllehTvProgram 라이브러리와는 달리 KT_TvScheduleUpdate가 생성한 html 파일에서 프로그램 리스트를 추출한다.
		public List<OllehProgram> GetProgramList(string chNum, string chName)
		{
			List<OllehProgram> lstTmp = new List<OllehProgram>();

			string html = GetProgramHtml(int.Parse(chNum));

			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(html);
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


		#region HTML 노드 핸들링 메서드

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

		#endregion
	}
}
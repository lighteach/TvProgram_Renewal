using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using TvProgramList.Models.YouTube;

namespace TvProgramList.Libs
{
	public class YouTube
	{
		#region YouTubeApiInfo : 유튜브 API 
		private YouTubeApiInfoModel YouTubeApiInfo
		{
			get
			{
				YouTubeApiInfoModel model = new YouTubeApiInfoModel();
				string localPath = $"{HttpContext.Current.Server.MapPath("/")}\\YouTubeApiInfo.js";
				if (File.Exists(localPath))
				{
					string jsonContent = File.ReadAllText(localPath);
					model = JsonConvert.DeserializeObject<YouTubeApiInfoModel>(jsonContent);
				}
				else
				{
					throw new Exception("YouTubeApiInfo.js not exist");
				}

				return model;
			}
		}
		#endregion

		#region GetYoutubeTop3 : 유튜브 비디오 TOP3 가져오기
		public YouTubeVideoTop3Model GetYoutubeVideoTop3()
		{
			YouTubeVideoTop3Model model = new YouTubeVideoTop3Model();
			YouTubeApiInfoModel apiInfo = YouTubeApiInfo;
			string apiURI = apiInfo.ApiURI.Top3VideoList;
			string apiKey = apiInfo.ApiKey;

			apiURI = ApiUriWithKeyBuilder(apiURI, apiKey);

			HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(apiURI);
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
			{
				string strRtn = reader.ReadToEnd();
				model = JsonConvert.DeserializeObject<YouTubeVideoTop3Model>(strRtn);
			}

			return model;
		}
		#endregion

		#region ApiUriWithKeyBuilder : API URI와 키코드를 합친 URI를 생성해주는 메쏘드
		private string ApiUriWithKeyBuilder(string uri, string key)
		{
			string strRtn = "";
			Uri u = new Uri(uri);
			if (u.Query.Split('&').ToList().Exists(s => s.Contains("key=")))
			{
				strRtn = uri;
			}
			else
			{
				strRtn = $"{uri}&key={key}";
			}

			return strRtn;
		} 
		#endregion
	}
}
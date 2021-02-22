using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using SMELuckyDraw.Model;
using SMELuckyDraw.Util;
using System.Data;
using System.IO;
using System.Windows.Media;

namespace SMELuckyDraw.Logic
{
	public class DrawLogic
	{
		public Dictionary<string, Candidate> _candidateList = new Dictionary<string, Candidate>();
		public Dictionary<string, Candidate> _exceptionList = new Dictionary<string, Candidate>();
		public Dictionary<int, CandidateRandom> _candidateListRandom = new Dictionary<int, CandidateRandom>();

		public MediaPlayer _mediaApp = new MediaPlayer();
		public MediaPlayer _mediaStart = new MediaPlayer();
		public MediaPlayer _mediaStop = new MediaPlayer();
		private static DrawLogic _instance = new DrawLogic();
		private bool isInitialized = false;
		private DrawLogic() { }

		public static DrawLogic Instance()
		{
			return _instance;
		}

		public void Init()
		{
			if (isInitialized)
			{
				return;
			}

			try
			{
				prepareCandidateList();
				prepareExceptionList();
				isInitialized = true;

				string currDir = System.AppDomain.CurrentDomain.BaseDirectory;
				string mediaName = ConfigHelper.Instance().GetAppSettings("SoundApp");
				string excelPath = Path.Combine(currDir, "excel");
				excelPath = Path.Combine(excelPath, mediaName);

				_mediaApp.Open(new Uri(excelPath));

				mediaName = ConfigHelper.Instance().GetAppSettings("SoundStart");
				excelPath = Path.Combine(currDir, "excel");
				excelPath = Path.Combine(excelPath, mediaName);

				_mediaStart.Open(new Uri(excelPath));

				mediaName = ConfigHelper.Instance().GetAppSettings("SoundStop");
				excelPath = Path.Combine(currDir, "excel");
				excelPath = Path.Combine(excelPath, mediaName);

				_mediaStop.Open(new Uri(excelPath));
			}
			catch (Exception e)
			{
				throw;
			}
		}

		public void Close()
		{
			ConfigHelper.Instance().CopyAppSettings();
			_mediaStart.Close();
			_mediaStart.Close();
		}

		private void prepareCandidateList()
		{
			//STEP 1, read from excel to DataTable
			string currDir = System.AppDomain.CurrentDomain.BaseDirectory;
			string excelName = ConfigHelper.Instance().GetAppSettings("excelName");
			string excelPath = Path.Combine(currDir, "excel");
			excelPath = Path.Combine(excelPath, excelName);
			DataTable dtCandidates = ExcelHelper.ExcelToDatatable(excelPath, true, false);

			//STEP2, loop DataTable, put each candidate to list
			if (dtCandidates != null)
			{
				int id = 0;

				foreach (DataRow row in dtCandidates.Rows)
				{
					Candidate cdt = new Candidate();
					cdt.MSNV = row["MSNV"].ToString();
					cdt.Name = row["Name"].ToString();
					_candidateList.Add(row["MSNV"].ToString(), cdt);

					CandidateRandom candidateRandom = new CandidateRandom();

					candidateRandom.MSNV = row["MSNV"].ToString();
					candidateRandom.Name = row["Name"].ToString();
					candidateRandom.RandomID = 0;
					_candidateListRandom.Add(id++, candidateRandom);
				}
			}
		}

		private void prepareExceptionList()
		{
			string strExcp = ConfigHelper.Instance().GetAppSettings("exceptionList");
			strExcp.TrimEnd();
			strExcp.TrimEnd(',');
			string[] listExcp = strExcp.Split(',');

			foreach (string str in listExcp)
			{
				if (string.IsNullOrWhiteSpace(str))
				{
					continue;
				}

				_exceptionList.Add(str, _candidateList[str]);
			}
		}

		private string getNextNumberFromList()
		{
			if (_exceptionList.Count >= _candidateList.Count)
			{
				return "-1";
			}

			int maxCount = 0;

			Dictionary<int, CandidateRandom> _candidateListUnlucky = new Dictionary<int, CandidateRandom>();
			Dictionary<int, CandidateRandom> _candidateListTemp = new Dictionary<int, CandidateRandom>();
			int res = 0;
			int id = 0;
			Random random = new Random();

			_candidateListUnlucky =
				_candidateListRandom.Where(c => !_exceptionList.ContainsKey(c.Value.MSNV)).ToDictionary(c => c.Key, c => c.Value);

			foreach (CandidateRandom candidateUnlucky in _candidateListUnlucky.Values)
			{
				CandidateRandom candidateRandomTemp = new CandidateRandom();
				double randomID = random.NextDouble();

				candidateRandomTemp.MSNV = candidateUnlucky.MSNV;
				candidateRandomTemp.Name = candidateUnlucky.Name;
				candidateRandomTemp.RandomID = randomID;

				_candidateListTemp.Add(id++, candidateRandomTemp);
			}

			id = 0;
			_candidateListRandom = _candidateListTemp.OrderBy(c => c.Value.RandomID).ToDictionary(c => id++, c => c.Value);

			maxCount = _candidateListRandom.Count;

			Random randomMSNV = new Random();
			res = randomMSNV.Next(0, maxCount);

			return _candidateListRandom[res].MSNV;
		}

		public bool IsAbleToDraw()
		{
			return _candidateList.Count > 0 && _candidateList.Count > _exceptionList.Count;
		}

		public Candidate DoDraw()
		{
			string msnv = getNextNumberFromList();

			//id = -1, no candidate left
			if (msnv == "-1")
			{
				return null;
			}

			string winner = _candidateList[msnv].MSNV + "  " + _candidateList[msnv].Name;
			_exceptionList.Add(msnv, _candidateList[msnv]);
			ConfigHelper.Instance().AppendAppSettings("exceptionList", msnv + ", ");

			return _candidateList[msnv];
		}

		//public Candidate FreeDraw()
		//{
		//    Random random = new Random();
		//    int idx = random.Next(_maxCount);
		//    return _candidateList[idx];
		//}

		public int GetExceptionCount()
		{
			return _exceptionList.Count;
		}

		/// <summary>
		/// Reset app to init state
		/// </summary>
		public void ResetApp()
		{
			_exceptionList.Clear();
			ConfigHelper.Instance().UpdateAppSettings("exceptionList", "");
		}
	}
}

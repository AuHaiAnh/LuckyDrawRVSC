using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

using SMELuckyDraw.Logic;
using SMELuckyDraw.Model;

namespace SMELuckyDraw
{
	/// <summary>
	/// Interaction logic for DanhSach.xaml
	/// </summary>
	public partial class DanhSach : Window
	{
		private DrawLogic _logic = null;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="uPara">
		/// <newpara>uPara[0]: List ALL NV</newpara>
		/// <newpara>uPara[0]: List NV Trung Thuong</newpara>
		/// </param>
		public DanhSach(DrawLogic logic)
		{
			InitializeComponent();

			_logic = logic;

			Init();
		}

		private void Init()
		{
			FormatGridTT();
			FormatGridCTT();
		}

		private void FormatGridTT()
		{
			List<GridCandidate> listNV = new List<GridCandidate>();

			int stt = 0;
			foreach (Candidate nv in _logic._exceptionList.Values)
			{
				stt++;
				GridCandidate gridCandidate = new GridCandidate();

				gridCandidate.STT = stt;
				gridCandidate.MSNV = nv.MSNV;
				gridCandidate.Name = nv.Name;

				if (stt <= 3)
				{
					gridCandidate.Note = "Giải " + stt.ToString();
				}
				else
				{
					gridCandidate.Note = "";
				}

				listNV.Add(gridCandidate);
			}

			this.gridNVTT.CanUserAddRows = false;
			this.gridNVTT.CanUserDeleteRows = false;
			this.gridNVTT.CanUserSortColumns = false;
			this.gridNVTT.ItemsSource = listNV;
		}

		private void FormatGridCTT()
		{
			List<GridCandidate> listNV = new List<GridCandidate>();
			int stt = 0;

			foreach (Candidate nv in _logic._candidateList.Values)
			{
				if (_logic._exceptionList.ContainsValue(nv))
				{
					continue;
				}

				stt++;
				GridCandidate gridCandidate = new GridCandidate();

				gridCandidate.STT = stt;
				gridCandidate.MSNV = nv.MSNV;
				gridCandidate.Name = nv.Name;
				gridCandidate.Note = "";

				listNV.Add(gridCandidate);
			}

			this.gridNVCTT.CanUserAddRows = false;
			this.gridNVCTT.CanUserDeleteRows = false;
			this.gridNVCTT.CanUserSortColumns = false;
			this.gridNVCTT.ItemsSource = listNV;
		}
	}
}

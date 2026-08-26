using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PooiBg
{
	/// <summary>
	/// Mod 设置。继承 Verse.ModSettings 后自动保存到 Config 文件夹（所有存档共用）。
	/// </summary>
	public class CustomBackgroundSettings : ModSettings
	{
		/// <summary>是否启用自定义背景。</summary>
		public bool useCustomBackground = true;

		/// <summary>选中的图片文件名；空字符串表示“多图随机轮换”。</summary>
		public string selectedFile = "";

		/// <summary>随机轮换的间隔秒数。</summary>
		public float cycleSeconds = 30f;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref useCustomBackground, "useCustomBackground", true);
			Scribe_Values.Look(ref selectedFile, "selectedFile", "");
			Scribe_Values.Look(ref cycleSeconds, "cycleSeconds", 30f);
		}

		/// <summary>设置界面绘制入口，由 Mod.DoSettingsWindowContents 调用。</summary>
		public void DoWindowContents(Rect inRect, CustomBackgroundMod _)
		{
			Listing_Standard ls = new Listing_Standard();
			ls.Begin(inRect);

			ls.Gap(6f);
			ls.CheckboxLabeled("启用自定义主菜单背景", ref useCustomBackground, null, 26f, 1f);

			ls.Gap(8f);
			ls.Label("图片文件夹：" + (BackgroundManager.Folder ?? "（未找到 Mod 目录）"));
			ls.Label("把 png / jpg 图片放进上面的文件夹，或点“添加图片”。");

			if (ls.ButtonText("添加图片…（游戏内文件浏览器，可多选）"))
			{
				Find.WindowStack.Add(new Dialog_BackgroundFileBrowser());
			}

			if (ls.ButtonText("重新扫描背景图片文件夹"))
			{
				BackgroundManager.Rescan();
			}

			ls.Gap(8f);
			string display = selectedFile == "" ? "多图随机轮换" : selectedFile;
			if (ls.ButtonText("当前背景：" + display))
			{
				List<FloatMenuOption> opts = new List<FloatMenuOption>();
				opts.Add(new FloatMenuOption("多图随机轮换", delegate { selectedFile = ""; }));
				foreach (string name in BackgroundManager.AvailableFileNames())
				{
					string n = name;
					opts.Add(new FloatMenuOption(n, delegate { selectedFile = n; }));
				}
				Find.WindowStack.Add(new FloatMenu(opts));
			}

			ls.Gap(4f);
			cycleSeconds = ls.SliderLabeled("随机轮换间隔（秒）：" + cycleSeconds, cycleSeconds, 3f, 300f);

			ls.Gap(12f);
			ls.Label("提示：建议使用 16:9 左右的横图，效果最佳。");

			ls.End();
		}
	}
}
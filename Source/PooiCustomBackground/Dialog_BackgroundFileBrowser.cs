using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace PooiBg
{
	/// <summary>
	/// 游戏内“添加图片”文件浏览器。
	/// 纯引擎实现（不依赖系统对话框），让玩家浏览到任意 png/jpg，
	/// 勾选后复制到 Backgrounds 文件夹并自动重扫。
	/// </summary>
	public class Dialog_BackgroundFileBrowser : Window
	{
		public override Vector2 InitialSize => new Vector2(640f, 520f);

		private static readonly string[] Exts = { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };

		private string currentDir;
		private readonly HashSet<string> selectedFiles = new HashSet<string>();
		private string statusMsg = "";
		private Vector2 scrollPos;

		public Dialog_BackgroundFileBrowser()
		{
			doCloseX = true;
			absorbInputAroundWindow = true;
			currentDir = InitialDir();
		}

		private static string InitialDir()
		{
			string d = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			if (string.IsNullOrEmpty(d) || !Directory.Exists(d))
			{
				d = Directory.GetCurrentDirectory();
			}
			return d;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 30f), "添加背景图片");
			Text.Font = GameFont.Small;

			// 顶部：上一级 + 当前路径
			Rect bar = new Rect(inRect.x, inRect.y + 34f, inRect.width, 24f);
			if (Widgets.ButtonText(new Rect(bar.x, bar.y, 70f, 22f), "上一级"))
			{
				GoUp();
			}
			Widgets.Label(new Rect(bar.x + 78f, bar.y, bar.width - 78f, 22f),
				"当前文件夹：" + (currentDir == "" ? "（磁盘根目录）" : currentDir));

			// 帮助
			Rect help = new Rect(inRect.x, bar.y + 28f, inRect.width, 22f);
			Widgets.Label(help, "点“文件夹/”进入目录，勾选图片后点下方“添加选中的图片”。");

			// 文件列表区
			Rect listRect = new Rect(inRect.x, help.yMax + 4f, inRect.width, inRect.height - (help.yMax - inRect.y) - 40f);

			List<string> dirs = new List<string>();
			List<string> images = new List<string>();
			// 注意：不能把 Path.GetDirectoryName(currentDir)==null 当作“在根目录”，
			// 因为 Windows 上 "C:\" 的父目录为 null，会被误判成盘符根目录，导致无法进入任何盘。
			bool atRoot = (currentDir == "");
			if (atRoot)
			{
				try { foreach (string d in Environment.GetLogicalDrives()) dirs.Add(d); } catch { }
			}
			else
			{
				try
				{
					foreach (string d in Directory.GetDirectories(currentDir)) dirs.Add(d);
					foreach (string f in Directory.GetFiles(currentDir))
						if (Exts.Contains(Path.GetExtension(f).ToLowerInvariant())) images.Add(f);
				}
				catch { }
			}

			Widgets.DrawMenuSection(listRect);

			// 内容高度 = 文件夹按钮(30/行) + 图片勾选行(26/行)，超出可视区域用滚动条下滑浏览。
			float contentHeight = dirs.Count * 30f + images.Count * 26f + 8f;
			Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, contentHeight);
			Widgets.BeginScrollView(listRect, ref scrollPos, viewRect, true);

			Listing_Standard ls = new Listing_Standard();
			ls.Begin(viewRect);
			ls.ColumnWidth = viewRect.width;

			bool nav = false;
			foreach (string d in dirs.OrderBy(x => x))
			{
				string label = Path.GetFileName(d) + "/";
				if (atRoot) label = d + "/";
				if (ls.ButtonText(label))
				{
					currentDir = d;
					selectedFiles.Clear();
					nav = true;
					break;
				}
			}

			if (!nav)
			{
				foreach (string img in images.OrderBy(x => x))
				{
					string name = Path.GetFileName(img);
					bool sel = selectedFiles.Contains(img);
					ls.CheckboxLabeled(name, ref sel, null, 26f, 1f);
					if (sel) selectedFiles.Add(img); else selectedFiles.Remove(img);
				}
			}

			ls.End();
			Widgets.EndScrollView();

			// 底部：状态 + 添加 + 关闭
			Rect bottom = new Rect(inRect.x, inRect.y + inRect.height - 34f, inRect.width, 26f);
			Widgets.Label(new Rect(bottom.x, bottom.y, bottom.width * 0.45f, 24f), statusMsg);
			if (Widgets.ButtonText(new Rect(bottom.x + bottom.width - 130f, bottom.y, 130f, 24f),
				"添加选中的图片 (" + selectedFiles.Count + ")"))
			{
				AddSelected();
			}
		}

		private void GoUp()
		{
			string parent = currentDir == "" ? null : Path.GetDirectoryName(currentDir);
			currentDir = parent ?? "";
			selectedFiles.Clear();
		}

		private void AddSelected()
		{
			List<string> toCopy = selectedFiles.ToList();
			if (toCopy.Count == 0)
			{
				statusMsg = "请先勾选要添加的图片。";
				return;
			}
			string dir = BackgroundManager.Folder;
			if (dir == null)
			{
				statusMsg = "未找到 Mod 目录。";
				return;
			}

			Directory.CreateDirectory(dir);
			int copied = 0;
			foreach (string f in toCopy)
			{
				try
				{
					File.Copy(f, Path.Combine(dir, Path.GetFileName(f)), true);
					copied++;
				}
				catch (Exception e)
				{
					statusMsg = "部分图片添加失败：" + e.Message;
				}
			}
			selectedFiles.Clear();
			BackgroundManager.Rescan();
			statusMsg = "已添加 " + copied + " 张图片。";
		}
	}
}
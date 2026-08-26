using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace PooiBg
{
	/// <summary>
	/// 背景图片管理器：扫描 mod 内 Backgrounds 文件夹、按需加载并缓存 Texture2D、
	/// 根据设置决定当前应显示哪一张。
	/// </summary>
	public static class BackgroundManager
	{
		private static readonly string[] Exts = { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };

		private static bool scanned = false;
		private static List<string> files = new List<string>();
		private static Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();
		private static int activeIndex = -1;
		private static double lastSwapRealTime = -1.0;

		/// <summary>用户放置背景图片的文件夹绝对路径。</summary>
		public static string Folder
		{
			get
			{
				string root = CustomBackgroundMod.Instance?.Content.RootDir;
				return string.IsNullOrEmpty(root) ? null : Path.Combine(root, "Backgrounds");
			}
		}

		/// <summary>重新扫描文件夹（例如用户加入了新图后调用）。</summary>
		public static void Rescan()
		{
			scanned = true;
			string dir = Folder;
			if (dir == null) { files.Clear(); return; }

			try { Directory.CreateDirectory(dir); }
			catch { files.Clear(); return; }

			// 清理已不存在的文件的缓存。
			var toRemove = cache.Keys.Where(k => !File.Exists(k)).ToList();
			foreach (var k in toRemove)
			{
				Texture2D t = cache[k];
				cache.Remove(k);
				UnityEngine.Object.DestroyImmediate(t);
			}

			List<string> found = new List<string>();
			try
			{
				found = Directory.GetFiles(dir)
					.Where(f => Exts.Contains(Path.GetExtension(f).ToLowerInvariant()))
					.OrderBy(f => f).ToList();
			}
			catch { files.Clear(); return; }

			files = found;
			activeIndex = -1;
			lastSwapRealTime = -1.0;
		}

		private static void EnsureScan()
		{
			if (!scanned) Rescan();
		}

		private static Texture2D LoadTexture(string path)
		{
			if (cache.TryGetValue(path, out Texture2D tex) && tex != null) return tex;
			try
			{
				byte[] data = File.ReadAllBytes(path);
				tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
				tex.LoadImage(data);
				tex.name = Path.GetFileName(path);
				cache[path] = tex;
				return tex;
			}
			catch (Exception e)
			{
				Log.Warning("[PooiBg CustomBackground] 无法加载图片 " + path + "：" + e.Message);
				return null;
			}
		}

		/// <summary>返回当前应显示的背景纹理；未启用/无图片时返回 null（表示用原版背景）。</summary>
		public static Texture2D GetActiveTexture()
		{
			CustomBackgroundSettings s = CustomBackgroundMod.Settings;
			if (s == null || !s.useCustomBackground) return null;

			EnsureScan();
			if (files.Count == 0) return null;

			double now = Time.realtimeSinceStartup;
			bool keepCurrent = (now - lastSwapRealTime < (double)s.cycleSeconds);

			if (s.selectedFile == "") // 随机轮换模式
			{
				if (activeIndex < 0 || activeIndex >= files.Count || !keepCurrent)
				{
					activeIndex = UnityEngine.Random.Range(0, files.Count);
					lastSwapRealTime = now;
				}
			}
			else // 手动指定某一张
			{
				int idx = files.FindIndex(f => string.Equals(Path.GetFileName(f), s.selectedFile, StringComparison.OrdinalIgnoreCase));
				if (idx >= 0)
				{
					activeIndex = idx;
				}
				else // 所选图片已被删除，退化为随机轮换
				{
					if (activeIndex < 0 || activeIndex >= files.Count || !keepCurrent)
					{
						activeIndex = UnityEngine.Random.Range(0, files.Count);
						lastSwapRealTime = now;
					}
				}
			}

			if (activeIndex < 0 || activeIndex >= files.Count) return null;
			return LoadTexture(files[activeIndex]);
		}

		/// <summary>当前文件夹里的所有图片文件名，供设置界面选择。</summary>
		public static List<string> AvailableFileNames()
		{
			EnsureScan();
			return files.Select(Path.GetFileName).ToList();
		}
	}
}
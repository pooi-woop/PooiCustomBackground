using System;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace PooiBg
{
	/// <summary>
	/// Mod 主类：负责创建设置对象，并在游戏 Mod 设置界面显示设置页。
	/// </summary>
	public class CustomBackgroundMod : Mod
	{
		/// <summary>全局唯一实例，供运行时读取 mod 根目录等。</summary>
		public static CustomBackgroundMod Instance;

		/// <summary>全局设置（保存到 Config）</summary>
		public static CustomBackgroundSettings Settings;

		public CustomBackgroundMod(ModContentPack content) : base(content)
		{
			Instance = this;
			Settings = GetSettings<CustomBackgroundSettings>();
		}

		/// <summary>游戏 Mod 设置列表里显示的标题。</summary>
		public override string SettingsCategory()
		{
			return "自定义主菜单背景";
		}

		/// <summary>游戏 Mod 设置界面内容。</summary>
		public override void DoSettingsWindowContents(Rect inRect)
		{
			Settings.DoWindowContents(inRect, this);
		}
	}

	/// <summary>
	/// Harmony 补丁初始化入口。[StaticConstructorOnStartup] 保证游戏启动、主菜单显示前自动执行。
	/// </summary>
	[StaticConstructorOnStartup]
	public static class HarmonyInit
	{
		static HarmonyInit()
		{
			new Harmony("pooiwop.custombackground").PatchAll();
			Log.Message("[PooiBg CustomBackground] Harmony 补丁已加载 / Harmony patches applied.");
		}
	}
}
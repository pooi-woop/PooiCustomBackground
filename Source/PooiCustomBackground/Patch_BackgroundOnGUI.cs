using RimWorld;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace PooiBg
{
	/// <summary>
	/// 主菜单背景绘制补丁。
	/// 在 UI_BackgroundMain.BackgroundOnGUI() 真正绘制前，把我们选择的纹理赋给 overrideBGImage；
	/// 置空时走原版背景，非空时原版代码会用它作为主菜单背景图。
	/// </summary>
	[HarmonyPatch(typeof(UI_BackgroundMain), nameof(UI_BackgroundMain.BackgroundOnGUI))]
	public static class Patch_BackgroundOnGUI
	{
		static void Prefix(UI_BackgroundMain __instance)
		{
			__instance.overrideBGImage = BackgroundManager.GetActiveTexture();
		}
	}
}
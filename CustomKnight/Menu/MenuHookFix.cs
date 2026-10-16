using System.Reflection;
using Satchel.BetterMenus;

namespace CustomKnight
{
    /// <summary>
    /// Satchel's BetterMenus <see cref="Menu"/> constructor does `On.UIManager.ShowMenu += ShowMenu`
    /// and never removes it. That handler is an iterator that calls orig lazily (inside the coroutine),
    /// which breaks on the mobile ModdingAPI (native self pointer can't be converted to a managed
    /// UIManager, so orig runs with a null self after the bridge has returned) and freezes every menu
    /// transition. It also leaks one hook per menu instance on every platform.
    /// We detach it right after construction; the menu still builds and navigates normally.
    /// </summary>
    internal static class MenuHookFix
    {
        internal static Menu Create(string name, Element[] elements)
        {
            var menu = new Menu(name, elements);
            Detach(menu);
            return menu;
        }

        internal static void Detach(Menu menu)
        {
            if (menu == null) { return; }
            try
            {
                var method = typeof(Menu).GetMethod("ShowMenu", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (method == null) { return; }
                var hook = (On.UIManager.hook_ShowMenu)System.Delegate.CreateDelegate(typeof(On.UIManager.hook_ShowMenu), menu, method);
                On.UIManager.ShowMenu -= hook;
            }
            catch (System.Exception e)
            {
                CustomKnight.Instance?.LogError("Could not detach BetterMenus ShowMenu hook: " + e.Message);
            }
        }
    }
}

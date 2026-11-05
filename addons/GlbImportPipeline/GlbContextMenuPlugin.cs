using Godot;

namespace CookingSim.Addons.GlbImportPipeline;

[Tool]
public partial class GlbContextMenuPlugin : EditorContextMenuPlugin
{
    public override void _PopupMenu(string[] paths)
    {
        foreach (string path in paths)
        {
            if (path.GetExtension() != "glb") continue;

            AddContextMenuItem(
                "Create Inherited Scene from GLB",
                Callable.From<string[]>(_OnMenuItemPressed),
                icon: null
            );

            break;
        }
    }

    private static void _OnMenuItemPressed(string[] paths)
    {
        foreach (string path in paths) {
            if (path.GetExtension() != "glb") continue;
            
            GlbPipeline pipeline = new GlbPipeline();
            pipeline.ProcessGlb(path);
        }
    }
}
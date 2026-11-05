#if TOOLS
using Godot;

namespace CookingSim.Addons.GlbImportPipeline;

[Tool]
public partial class GlbImportPipelinePlugin : EditorPlugin
{
    private readonly GlbContextMenuPlugin _contextMenuPlugin = new();

    public override void _EnterTree()
    {
        // GD.Print("GlbImportPipelinePlugin entered tree");
        
        AddContextMenuPlugin(
            EditorContextMenuPlugin.ContextMenuSlot.Filesystem,
            _contextMenuPlugin
        );
    }

    public override void _ExitTree()
    {
        // GD.Print("GlbImportPipelinePlugin exited tree");
        
        RemoveContextMenuPlugin(_contextMenuPlugin);
    }
}
#endif
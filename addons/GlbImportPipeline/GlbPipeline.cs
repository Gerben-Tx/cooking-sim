using Godot;

namespace CookingSim.Addons.GlbImportPipeline;

[Tool]
public partial class GlbPipeline : Node {
    private const string ImportScriptPath = "res://ImportScripts/GlbPostImport.cs";

    private readonly System.Collections.Generic.Dictionary<
        string,
        bool
    > _pendingGlbs = new();

    public void ProcessGlb(
        string glbPath
    ) {
        EditorFileSystem fileSystem = EditorInterface.Singleton.GetResourceFilesystem();

        fileSystem.ResourcesReimported += OnResourcesReimported;

        string importFilePath = glbPath + ".import";

        ConfigFile config = new ConfigFile();

        if (FileAccess.FileExists(importFilePath)) {
            config.Load(importFilePath);
        }

        config.SetValue(
            "params",
            "import_script/path",
            ImportScriptPath
        );

        Error saveError = config.Save(importFilePath);
        if (saveError != Error.Ok) {
            GD.PushError($"Failed to update import settings for: {glbPath}");
            return;
        }

        _pendingGlbs[glbPath] = true;

        fileSystem.ReimportFiles([glbPath]);
    }

    private void OnResourcesReimported(
        string[] resources
    ) {
        foreach (string resourcePath in resources) {
            if (!_pendingGlbs.Remove(resourcePath)) {
                continue;
            }

            CreateInheritedScene(resourcePath);

            EditorInterface.Singleton
                .GetResourceFilesystem()
                .ResourcesReimported -= OnResourcesReimported;

            QueueFree();
            break;
        }
    }

    private void CreateInheritedScene(
        string glbPath
    ) {
        PackedScene baseScene = ResourceLoader.Load<PackedScene>(glbPath);
        if (baseScene == null) {
            GD.PushError($"Could not load imported scene: {glbPath}");
            return;
        }

        Node inheritedRoot = baseScene.Instantiate(
            PackedScene.GenEditState.Main
        );

        PackedScene packedScene = new PackedScene();

        Error packError = packedScene.Pack(inheritedRoot);
        if (packError != Error.Ok) {
            inheritedRoot.QueueFree();
            GD.PushError($"Could not pack inherited scene: {glbPath}");
            return;
        }

        string outputScenePath = glbPath.GetBaseName() + "_inherited.tscn";

        Error saveError = ResourceSaver.Save(
            packedScene,
            outputScenePath
        );
        inheritedRoot.QueueFree();
        if (saveError != Error.Ok) {
            GD.PushError(
                $"Could not save inherited scene: {outputScenePath}"
            );
            return;
        }

        EditorInterface.Singleton
            .GetResourceFilesystem()
            .UpdateFile(outputScenePath);

        GD.Print($"Created inherited scene: {outputScenePath}");
    }
}
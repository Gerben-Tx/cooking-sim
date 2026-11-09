using System;
using System.IO;
using CookingSim.ImportScripts;
using Godot;

namespace CookingSim.Objects.BurgerBunTop;

[Tool] // DONT FORGET THE TOOL
public partial class PostImport : GlbPostImport {
    private const string ModelName = "burger_bun_top-rigid";
    private const string RawMaterialPath = "material.tres";
    
    public override Node _PostImport(Node node) {
        Node ret = base._PostImport(node);
        if (ret is not Node3D node3D) {
            return ret;
        }
        
        Configure(node3D);
        
        return ret;
    }
    
    private void Configure(Node3D node3D) {
        if (node3D.GetNode<RigidBody3D>(ModelName) is not RigidBody3D rigidBody3D) {
            throw new Exception("Could not find RigidBody3D");
        }

        if (rigidBody3D.GetNode<MeshInstance3D>(ModelName) is not MeshInstance3D meshInstance3D) {
            throw new Exception("Could not find MeshInstance3D");
        }
        
        // Set raw material
        string cwd = GetSourceFile().Substring(0, GetSourceFile().Length - Path.GetFileName(GetSourceFile()).Length);
        meshInstance3D.Mesh.SurfaceSetMaterial(0, GD.Load<BaseMaterial3D>(cwd + RawMaterialPath));
    }
}
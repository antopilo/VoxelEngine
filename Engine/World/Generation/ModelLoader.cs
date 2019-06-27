using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum Models
{
    Grass, Flower, Fern
}

public class ModelLoader
{
    private static string ModelPath = "res://Content/Models/";

    private static Dictionary<Models, ArrayMesh> m_Palette = new Dictionary<Models, ArrayMesh>();
    

    public static ArrayMesh GetModel(Models model)
    {
        return m_Palette[model];
    }

    public static void LoadModels()
    {
        LoadModel(Models.Grass, "Grass.tres");
        LoadModel(Models.Flower, "Flowers.tres");
        LoadModel(Models.Fern, "Fern.tres");
    }

    private static void LoadModel(Models model, string path)
    {
        m_Palette.Add(model, (ArrayMesh)ResourceLoader.Load(ModelPath + path));
    }
}

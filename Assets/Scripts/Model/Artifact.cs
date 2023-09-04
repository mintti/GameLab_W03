using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artifact
{
    public ArtifactType Type { get;}
    public string Name { get;}
    public string Description { get; }
    public Sprite Sprite { get; }
    public bool HasArtifact { get; }


    public Artifact(ArtifactType type, string name, string description, Sprite sprite)
    {
        Type = type;
        Name = name;
        Description = description;
        Sprite = sprite;
        HasArtifact = false;
    }
}

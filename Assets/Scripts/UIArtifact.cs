using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifact : MonoBehaviour
{
    GameManager _gameManager;
    Artifact _baseArtifact;
    
    public Image image;
    public TextMeshProUGUI name;
    public TextMeshProUGUI description;


    public void Init( Artifact artifact)
    {
        _gameManager = GameObject.Find(nameof(GameManager)).GetComponent<GameManager>();
        _baseArtifact = artifact;
        image.sprite = artifact.Sprite;
        name.text = artifact.Name;
        description.text = artifact.Description;
    }


    public void B_Selected()
    {
        _gameManager.GetArtifact(_baseArtifact);
        _gameManager.CompleteSelectArtifact();
    }
}

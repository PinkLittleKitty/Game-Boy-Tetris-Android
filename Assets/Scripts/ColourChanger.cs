#pragma warning disable CS0108
#pragma warning disable CS0108
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ColourChanger : MonoBehaviour
{
  public static ColourChanger instance;
  public Material mat;
  public Color Darkest, Dark, Light, Lightest;
  public ColorPalette[] colorPalettes;
  private MeshRenderer renderer;
  public int num;

  void Start()
  {
    if (instance == null || instance != this)
    {
      instance = this;
    }
    
    renderer = this.gameObject.GetComponent<MeshRenderer>();
  }

  public void ChangeColour(int n)
  {
    num = n;

    renderer.material.SetColor("_Darkest", colorPalettes[num].Darkest);
    renderer.material.SetColor("_Dark", colorPalettes[num].Dark);
    renderer.material.SetColor("_Light", colorPalettes[num].Light);
    renderer.material.SetColor("_Lightest", colorPalettes[num].Lightest);
  }
}
using UnityEngine;
using TMPro;

public class VersionDisplayer : MonoBehaviour
{
    public TextMeshProUGUI text;
    void Start()
    {
        text.text = "v" + Application.version;
    }
}

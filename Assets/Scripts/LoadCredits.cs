using LogicUI.FancyTextRendering;
using UnityEngine;

public class LoadCredits : MonoBehaviour
{
    [SerializeField]
    private MarkdownRenderer markdownRenderer;

    void Start()
    {
        markdownRenderer.Source = LoadCreditsText();
    }

    public string LoadCreditsText()
    {
        TextAsset mytxtData = (TextAsset)Resources.Load("CREDITS");
        return mytxtData.text;
    }
}

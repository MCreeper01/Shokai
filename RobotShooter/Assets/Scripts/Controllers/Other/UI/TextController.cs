using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextController : MonoBehaviour
{
    public Text text;

    public TextAsset textFile;    
    public string[] textLines;

    [HideInInspector] public int currentLine;
    [HideInInspector] public int endAtLine;

    // Start is called before the first frame update
    void Start()
    {
        if (textFile != null) textLines = textFile.text.Split('\n');
    }

    // Update is called once per frame
    void Update()
    {
        text.text = textLines[currentLine];
    }
}

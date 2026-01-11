using UnityEngine;
using UnityEngine.UI;
public class HolaMundo : MonoBehaviour
{
    public Text textoUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Hola Mundo desde Unity");
        textoUI.text = "Hola Mundo desde Unity";
    }
}

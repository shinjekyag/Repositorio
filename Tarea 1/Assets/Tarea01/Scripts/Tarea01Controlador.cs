using UnityEngine;

public class Tarea01Controlador : MonoBehaviour
{
    public GameObject objetoParaEncenderApagar;
    public Transform objetoParaMover;

    public void EncenderOApagarObjeto()
    {
        if (objetoParaEncenderApagar == null)
        {
            return;
        }

        objetoParaEncenderApagar.SetActive(!objetoParaEncenderApagar.activeSelf);
    }

    public void CambiarObjetoAPosicionRandom()
    {
        if (objetoParaMover == null)
        {
            return;
        }

        objetoParaMover.position = new Vector3(
            Random.Range(0f, 20f),
            Random.Range(0f, 20f),
            Random.Range(0f, 20f));
    }
}

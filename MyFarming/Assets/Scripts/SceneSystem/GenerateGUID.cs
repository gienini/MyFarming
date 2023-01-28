using UnityEngine;

//Sirve para asignar una ID unica a los gameObject a los que lo asignemos
//Este script funciona en modo edicion cuando se asigna, no solo en juego
[ExecuteAlways]
public class GenerateGUID : MonoBehaviour
{
    [SerializeField] private string _gUID = "";

    public string GUID { get => _gUID; set => _gUID = value; }


    private void Awake()
    {
        //Solo se ejecuta si no esta en modo "jugar"
        if (!Application.IsPlaying(gameObject))
        {
            if (_gUID == "")
            {
                _gUID = System.Guid.NewGuid().ToString();
            }
        }
    }
}

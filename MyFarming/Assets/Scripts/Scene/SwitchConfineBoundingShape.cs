using UnityEngine;
using Cinemachine;

public class SwitchConfineBoundingShape : MonoBehaviour
{
    // CAMBIAMOS la manera en que tomamos los gameObject, hay que esperar a que la escena este cargada
    // Start is called before the first frame update
    //
    //void Start()
    //{
    //    SwitchBoundingShape();
        
    //}
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SwitchBoundingShape;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SwitchBoundingShape;
    }
    /// <summary>
    /// Elige el collider que el cinemachine usara como limites de la pantalla 
    /// </summary>
    private void SwitchBoundingShape()
    {
        //Obtenemos el polygonCollider correspondiente a "BoundsConfiner" para que cinemachine
        //no salga de los limites de la pantalla
        PolygonCollider2D polygonCollider2D = GameObject.FindGameObjectWithTag(Tags.BoundsConfiner).GetComponent<PolygonCollider2D>();
        //Obtenemos objeto (aparentemente unico) de CinemachineConfiner
        CinemachineConfiner cinemachineConfiner = GetComponent<CinemachineConfiner>();
        //Le asignamos el polygonCollider a la propiedad boundingShape2D
        cinemachineConfiner.m_BoundingShape2D = polygonCollider2D;

        //El cinemachine esta en cache, de manera que toca recargar
        cinemachineConfiner.InvalidatePathCache();

    }

    

}

using UnityEngine;
/// <summary>
/// Este script se le pone al player para disparar los eventos de oscurecimiento/fade cuando colisiona con otros sprites que lo tapen
/// </summary>
public class TriggerObscuringItemFader : MonoBehaviour
{
    /// <summary>
    /// Se dispara cuando el player colisiona con un Collider2D
    /// </summary>
    /// <param name="collision"> El Collider2D con el que colisiona</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Obtenemos todos los ObscuringItemFader que tenga el objeto colisionado y sus hijos
        ObscuringItemFader[] obscuringItemFaders = collision.gameObject.GetComponentsInChildren<ObscuringItemFader>();
        //Disparamos el fadeOut en cada ObscuringItemFader que hayamos obtenido
        if (obscuringItemFaders.Length > 0)
        {
            for (int i = 0; i < obscuringItemFaders.Length; i++)
            {
                obscuringItemFaders[i].FadeOut();
            }
        }
    }
    /// <summary>
    /// Se dispara cuando el player deja de colisionar con un Collider2D
    /// </summary>
    /// <param name="collision"> El Collider2D con el que colisiona</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        //Obtenemos todos los ObscuringItemFader que tenga el objeto colisionado y sus hijos
        ObscuringItemFader[] obscuringItemFaders = collision.gameObject.GetComponentsInChildren<ObscuringItemFader>();
        //Disparamos el fadeOut en cada ObscuringItemFader que hayamos obtenido
        if (obscuringItemFaders.Length > 0)
        {
            for (int i = 0; i < obscuringItemFaders.Length; i++)
            {
                obscuringItemFaders[i].FadeIn();
            }
        }
    }
}

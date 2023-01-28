using System.Collections.Generic;
using UnityEngine;

public static class HelperMethods
{
    /// <summary>
    /// Comprueba si existe un objeto collider de tipo T en "positionToCheck" retorna true si es asi y rellena la lista con los objetos collider encontrados
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="componentsAtPositionList"></param>
    /// <param name="positionToCheck"></param>
    /// <returns></returns>
    public static bool GetComponentsAtCursorLocation<T>(out List<T> componentsAtPositionList, Vector3 positionToCheck)
    {
        bool found = false;
        List<T> componentList = new List<T>();
        //Retorna todos los collider en la posicion
        Collider2D[] collider2DArray = Physics2D.OverlapPointAll(positionToCheck);
        //LLamamos al constructor generico sin argumentos de T
        T tComponentPadre = default(T);
        T tComponentHijo = default(T);
        for (int i = 0; i < collider2DArray.Length; i++)
        {
            tComponentPadre = collider2DArray[i].gameObject.GetComponentInParent<T>();
            if (tComponentPadre != null)
            {
                found = true;
                componentList.Add(tComponentPadre);
            } else
            {
                tComponentHijo = collider2DArray[i].gameObject.GetComponentInChildren<T>();
                if (tComponentHijo != null)
                {
                    found = true;
                    componentList.Add(tComponentHijo);
                }
            }
        }


        componentsAtPositionList = componentList;
        return found;
    }

    public static bool GetComponentsAtBoxLocation<T>(out List<T> listComponentsAtBoxPosition, Vector2 point, Vector2 size, float angle)
    {
        bool found = false;
        List<T> componentList = new List<T>();
        Collider2D[] collider2DArray = Physics2D.OverlapBoxAll(point, size, angle);
        for (int i = 0; i < collider2DArray.Length; i++)
        {
            T tComponent = collider2DArray[i].gameObject.GetComponentInParent<T>();
            if (tComponent != null)
            {
                found = true;
                componentList.Add(tComponent);
            }else
            {
                tComponent = collider2DArray[i].gameObject.GetComponentInChildren<T>();
                if (tComponent != null)
                {
                    found = true;
                    componentList.Add(tComponent);
                }
            }
        }

        listComponentsAtBoxPosition = componentList;
        return found;
    }
    /// <summary>
    /// Retorna todos los components del tipo T en una caja con el centro en "point" y del tamaño "size" hasta llegar al numero "numberOfCollidersToTest"
    /// </summary>
    /// <returns></returns>
    public static T[] GetComponentsAtBoxLocation<T>(int numberOfCollidersToTest, Vector2 point, Vector2 size, float angle)
    {
        Collider2D[] collider2DArray = new Collider2D[numberOfCollidersToTest];
        //Metodo que retorna los objetos dentro de una caja. "NonAlloc" se refiere a el uso que hace de memoria que es mas eficiente
        Physics2D.OverlapBoxNonAlloc(point, size, angle, collider2DArray);

        T tComponent = default(T);
        T[] componentArray = new T[collider2DArray.Length];

        for (int i = 0; i < collider2DArray.Length; i++)
        {
            if (collider2DArray[i] != null)
            {
                tComponent = collider2DArray[i].gameObject.GetComponent<T>();
                if (tComponent != null)
                {
                    componentArray[i] = tComponent;
                }
            }
        }
        return componentArray;
    }
}

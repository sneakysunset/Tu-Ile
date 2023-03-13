using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VieBateau : MonoBehaviour
{
    private float maxPointDeVie = 100f;
    [Range (0f,100f)] public float pointDeVie;
    public float pertePointDeVie = 1f; 
    private MeshRenderer rendu;

    Color lerpedColor = Color.white;

    private void Awake()
    {
        rendu = GetComponent<MeshRenderer>();
    }
    void Update()
    {
        //Change de couleur en fonction de point de vie
        lerpedColor = Color.Lerp(Color.red, Color.green, pointDeVie/maxPointDeVie);
        rendu.material.color = lerpedColor;

        //Perd des points vie en fonction du temps
        pointDeVie -= pertePointDeVie * Time.deltaTime;

        //Limite des points de vie : Destruire l'objet si plus de point de vie / Si au dessus de 100, reste à 100
        if(pointDeVie <= 0)
        {
            Destroy(gameObject);
        }
        if(pointDeVie > maxPointDeVie)
        {
            pointDeVie = 100f;
        }

        //Faire apparaitre cube sur bato
        //Instantiate()
    }
}

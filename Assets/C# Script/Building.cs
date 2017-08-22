using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour {

    public int MaxLife = 30;        // Maximum de vie que le bâtiment peut avoir.
    public int Life = 30;           // Current life (can take dammage and heal).

    public int GoldCost = 30;
    public int StoneCost = 45;
    public int WoodCost = 80;

    public int NbTileX = 1;         // Nombre de tuile libre que nécessite ce building pour être créer dans l'axe des X.
    public int NbTileY = 1;         // Nombre de tuile libre que nécessite ce building pour être créer dans l'axe des Y.

    public string Name = "";
    public string Description = "";

    public Sprite Icon;

    private void Start ()
    {
		
	}

	private void Update ()
    {
		
	}
}

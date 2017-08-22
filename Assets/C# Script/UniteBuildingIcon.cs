using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UniteBuildingIcon : MonoBehaviour {

    public Building BuildingPrefab;
    public GameObject PanelInstruction;

    public bool IsAccessible = false;

    private Image ImageComponent;

	private void Start ()
    {
        GameObject objChild = null;
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            objChild = gameObject.transform.GetChild(i).gameObject;
            if (objChild.name == "imgBuilding")
            {
                ImageComponent = objChild.GetComponent<Image>();
                break;
            }
        }

        if (BuildingPrefab != null && ImageComponent != null)
        {
            ImageComponent.sprite = BuildingPrefab.Icon;
        }
    }

    public void OnMouseEnter()
    {
        PanelInstruction.SetActive(true);
        afficherInfosDansBulleInstruction();
    }

    public void OnMouseExit()
    {
        PanelInstruction.SetActive(false);
    }

    private void afficherInfosDansBulleInstruction()
    {
        if (PanelInstruction != null)
        {
            Text txtTitre = null;
            Text txtDescription = null;
            Text txtRessource1 = null;
            Text txtRessource2 = null;
            Text txtRessource3 = null;
            Image imgRessource1 = null;
            Image imgRessource2 = null;
            Image imgRessource3 = null;

            GameObject objChild = null;
            GameObject objChild2 = null;

            for (int i = 0; i < PanelInstruction.transform.childCount; i++)
            {
                objChild = PanelInstruction.transform.GetChild(i).gameObject;
                if (objChild.name.ToLower() == "txttitre")
                    txtTitre = objChild.GetComponent<Text>();
                else if (objChild.name.ToLower() == "txtdescription")
                    txtDescription = objChild.GetComponent<Text>();
                else if (objChild.name.ToLower() == "pnncout")
                {
                    for (int j = 0; j < objChild.transform.childCount; j++)
                    {
                        objChild2 = objChild.transform.GetChild(j).gameObject;
                        if (objChild2.name.ToLower() == "imgressource1")
                            imgRessource1 = objChild2.GetComponent<Image>();
                        else if (objChild2.name.ToLower() == "imgressource2")
                            imgRessource2 = objChild2.GetComponent<Image>();
                        else if (objChild2.name.ToLower() == "imgressource3")
                            imgRessource3 = objChild2.GetComponent<Image>();
                        else if (objChild2.name.ToLower() == "txtcoutressource1")
                            txtRessource1 = objChild2.GetComponent<Text>();
                        else if (objChild2.name.ToLower() == "txtcoutressource2")
                            txtRessource2 = objChild2.GetComponent<Text>();
                        else if (objChild2.name.ToLower() == "txtcoutressource3")
                            txtRessource3 = objChild2.GetComponent<Text>();
                    }
                }
            }

            txtTitre.text = BuildingPrefab.Name;
            txtDescription.text = BuildingPrefab.Description + "\r\n\r\nAmélirations possibles...\r\n\r\nStatistiques...";

            txtRessource1.text = BuildingPrefab.WoodCost.ToString();
            txtRessource2.text = BuildingPrefab.GoldCost.ToString();
            txtRessource3.text = BuildingPrefab.StoneCost.ToString();

            imgRessource1.color = new Color32(255, 255, 255, (byte)(BuildingPrefab.WoodCost > 0 ? 255 : 0));
            imgRessource2.color = new Color32(255, 255, 255, (byte)(BuildingPrefab.GoldCost > 0 ? 255 : 0));
            imgRessource3.color = new Color32(255, 255, 255, (byte)(BuildingPrefab.StoneCost > 0 ? 255 : 0));

            txtRessource1.color = new Color32(0, 0, 0, (byte)(BuildingPrefab.WoodCost > 0 ? 255 : 0));
            txtRessource2.color = new Color32(0, 0, 0, (byte)(BuildingPrefab.GoldCost > 0 ? 255 : 0));
            txtRessource3.color = new Color32(0, 0, 0, (byte)(BuildingPrefab.StoneCost > 0 ? 255 : 0));
        }
    }
}

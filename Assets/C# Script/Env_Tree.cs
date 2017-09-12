using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_Tree : IEnvironment {

    public int WoodAmount = 150;

	// Use this for initialization
	void Start () {
        _woodAmount = WoodAmount;
        _stoneAmount = 0;
        _goldAmount = 0;
        _foodAmount = 0;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public new void TakeRessource(int Amount)
    {
        _woodAmount -= Amount;

        if (_woodAmount <= 0)
            Destroy(gameObject);
    }
}
